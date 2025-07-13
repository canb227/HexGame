using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Steamworks;
using System.Numerics;
using System.Runtime.InteropServices;
using static Steamworks.SteamNetworkingMessages;
using NetworkMessages;
using Google.Protobuf;
using static Godot.HttpRequest;
using static Godot.WebSocketPeer;
using State = NetworkMessages.State;

[GlobalClass]
public partial class NetworkPeer : Node
{
    //The maximum number of network messages to attempt to process per frame. If we have this many messages waiting for us, don't delay frame processing any more, just delay processing on the messages over the limit till next frame.
    //If for some reason the frame timings are shit, maybe this needs lowered, but if you have this many messages per frame something has gone wrong
    int nMaxCommandMessagesPerFrame = 100;
    int nMaxChatMessagesPerFrame = 100;


    int nMaxHandshakeMessagesPerFrame = 100;
    int nMaxLobbyMessagesPerFrame = 100;

    /// <summary>
    /// List of all other network peers we are currently connected to. This will (should) only ever contain non-self peers that have confirmed our connection request. It may contain disconnected, unresponsive, or offline peers.
    /// </summary>
    Dictionary<SteamNetworkingIdentity, PeerConnectionData> remotePeers = new Dictionary<SteamNetworkingIdentity, PeerConnectionData>();

    public class PeerConnectionData
    {
        public double TimeSinceLastMessage = 0;
        public int AliveCheckCounter = 0;
        public double AliveCheckTimer = 0;
        public double AliveCheckFrequency = 5;
        public int AliveCheckUnresponsiveThreshold = 5;

        public PeerConnectionData()
        {
        }
    }

    /// <summary>
    /// Steamworks.NET/Steamworks API Callback. Is automagically triggered (by Steamworks.NET) when we receive a request from another Steam user to establish a connection. See the Steamworks API
    /// </summary>
    protected Callback<SteamNetworkingMessagesSessionRequest_t> SessionRequest;

    //  Steam Networking takes a bitflag string to configure message settings. Look them up in the steamworks API
    //  These options effectively switch between UDP and TCP-like behavior, and have signifigant implications on the functionality and performance of networking. Handle with care.
    public const int k_nSteamNetworkingSend_NoNagle = 1;
    public const int k_nSteamNetworkingSend_NoDelay = 4;
    public const int k_nSteamNetworkingSend_Unreliable = 0;
    public const int k_nSteamNetworkingSend_Reliable = 8;
    public const int k_nSteamNetworkingSend_UnreliableNoNagle = k_nSteamNetworkingSend_Unreliable | k_nSteamNetworkingSend_NoNagle;
    public const int k_nSteamNetworkingSend_UnreliableNoDelay = k_nSteamNetworkingSend_Unreliable | k_nSteamNetworkingSend_NoDelay | k_nSteamNetworkingSend_NoNagle;
    public const int k_nSteamNetworkingSend_ReliableNoNagle = k_nSteamNetworkingSend_Reliable | k_nSteamNetworkingSend_NoNagle;

    // Steam Networking expects a single int to indicate the channel to send or receive messages on. 

    public const int CHAT_CHANNEL = 1;
    public const int COMMAND_CHANNEL = 2;
    public const int HANDSHAKE_CHANNEL = 3;
    public const int LOBBY_CHANNEL = 4;


    public enum GamePrivacyMode { NONE = 0, OFFLINE = 1, PRIVATE = 2, FRIENDS = 3, PUBLIC = 4 };
    public GamePrivacyMode privacyMode = GamePrivacyMode.FRIENDS;


    public delegate void ChatMessageReceived(Chat chat);
    public static event ChatMessageReceived ChatMessageReceivedEvent;

    public delegate void CommandMessageReceived(Command command);
    public static event CommandMessageReceived CommandMessageReceivedEvent;

    public delegate void HandshakeMessageReceived(Handshake handshake);
    public static event HandshakeMessageReceived HandshakeMessageReceivedEvent;

    public delegate void LobbyMessageReceived(LobbyMessage lobbyMessage);
    public static event LobbyMessageReceived LobbyMessageReceivedEvent;

    public delegate void JoinRequestTimeout(ulong playerID);
    public static event JoinRequestTimeout JoinRequestTimeoutEvent;

    public delegate void PeerRequestTimeout(ulong playerID);
    public static event PeerRequestTimeout PeerRequestTimeoutEvent;

    public delegate void JoinRequestReceived(ulong playerID);
    public static event JoinRequestReceived JoinRequestReceivedEvent;

    public delegate void PeerListRequestReceived(ulong playerID);
    public static event PeerListRequestReceived PeerListRequestReceivedEvent;

    public delegate void PeerListStabilityChanged(bool isStable);
    public static event PeerListStabilityChanged PeerListStabilityChangedEvent;


    public delegate void PlayerJoined(ulong playerID);
    public static event PlayerJoined PlayerJoinedEvent;

    public delegate void JoinedToPlayer(ulong playerID);
    public static event JoinedToPlayer JoinedToPlayerEvent;

    public delegate void HandshakeCompleted(ulong playerID);
    public static event HandshakeCompleted HandshakeCompletedEvent;

    public delegate void PlayerLeft(ulong playerID);
    public static event PlayerLeft PlayerLeftEvent;

    public delegate void PlayerTimeout(ulong playerID);
    public static event PlayerTimeout PlayerTimeoutEvent;


    public Dictionary<SteamNetworkingIdentity, double> JoinRequestsCurrentlyOutTo = new();
    public Dictionary<SteamNetworkingIdentity, double> PeerRequestsCurrentlyOutTo = new();

    public bool PeersListStable = false;
    private double JoinRequestTimeoutTime = 30;
    private double PeerRequestTimeoutTime = 30;
    private double PeersListStabilityTimer = 0;
    private double PeersListStabilityThreshold = 1;



    // This is a Godot node, so it needs to be in the scenetree to work. There is no reason for this other than I thought it was neat.
    public override void _Ready()
    {
        Global.networkPeer = this;

        //Registers the automagical callback with the function to call when a message is received. See the Steamworks API
        SessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);

        //Since Handshake messages are so closely related to networking, we handle them here
        HandshakeMessageReceivedEvent += OnHandshakeMessageReceived;

        PeerListStabilityChangedEvent += OnPeerListStabilityChangedEvent;
    }

    private void OnPeerListStabilityChangedEvent(bool isStable)
    {
        PeersListStable = isStable;
    }


    ///////////////////////////////////////////////////////////////////////////////////
    // CORE NETWORKING

    /// <summary>
    /// Triggered by the Steamworks API when we receive a request from another Steam user to establish a connection.
    /// </summary>
    /// <param name="param"></param>
    private void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t param)
    {
        Global.Log("Connection request from: " + param.m_identityRemote.GetSteamID64());
        if (param.m_identityRemote.GetSteamID64() == Global.clientID)
        {
            Global.Log("Connection request from self, Rejected");
            return;
        }
        else if (remotePeers.Keys.Contains(param.m_identityRemote))
        {
            Global.Log("Already connected to this peer.");
            AcceptSessionWithUser(ref param.m_identityRemote);
            return;
        }
        else if (privacyMode == GamePrivacyMode.OFFLINE || privacyMode == GamePrivacyMode.NONE)
        {
            Global.Log("In offline mode, rejecting session.");
            SteamNetworkingMessages.CloseSessionWithUser(ref param.m_identityRemote);
            return;
        }
        else if (privacyMode == GamePrivacyMode.FRIENDS)
        {
            if (SteamFriends.GetFriendRelationship(param.m_identityRemote.GetSteamID()) == EFriendRelationship.k_EFriendRelationshipFriend)
            {
                AcceptSessionWithUser(ref param.m_identityRemote);

                Global.Log("Accepting session request from friend: " + param.m_identityRemote.GetSteamID64());
            }
        }
        else if (privacyMode == GamePrivacyMode.PUBLIC)
        {
            Global.Log("Accepting connection request, public mode.");
            AcceptSessionWithUser(ref param.m_identityRemote);
        }
    }

    /// <summary>
    /// This runs once per frame, and pulls messages from each channel to fill array buffers. Channels allow us to skip using a message wrapper to declare message types, which is not strictly required, but makes it a bit easier and allows for channel priority.
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {

        HandleTimeoutsAndConnections(delta);

        //Identical to above, but for Chat messages on the Chat Channel
        nint[] chatMessages = new nint[nMaxChatMessagesPerFrame];
        for (int i = 0; i < ReceiveMessagesOnChannel(CHAT_CHANNEL, chatMessages, nMaxChatMessagesPerFrame); i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(chatMessages[i]); //Converts the message to a C# object
            Chat chat = Chat.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            ChatMessageReceivedEvent?.Invoke(chat);
            SteamNetworkingMessage_t.Release(chatMessages[i]);
            FlagSessionAlive(steamMsg.m_identityPeer);
        }

        //Identical to above, but for Command Messages on the Messages Channel
        nint[] commandMessages = new nint[nMaxCommandMessagesPerFrame];
        int commandMessageNum = ReceiveMessagesOnChannel(COMMAND_CHANNEL, commandMessages, nMaxCommandMessagesPerFrame);
        if(commandMessageNum > 0)
        {
            Global.Log($"Network Peer Received {commandMessageNum} Command messages this frame.");
        }
        for (int i = 0; i < commandMessageNum; i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(commandMessages[i]); //Converts the message to a C# object
            Command command = Command.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            CommandMessageReceivedEvent?.Invoke(command);
            SteamNetworkingMessage_t.Release(commandMessages[i]);
            FlagSessionAlive(steamMsg.m_identityPeer);
        }

        //Identical to above, but for Handshake messages on the Handshake Channel
        nint[] handshakeMessages = new nint[nMaxHandshakeMessagesPerFrame];
        for (int i = 0; i < ReceiveMessagesOnChannel(HANDSHAKE_CHANNEL, handshakeMessages, nMaxHandshakeMessagesPerFrame); i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(handshakeMessages[i]); //Converts the message to a C# object
            Handshake handshake = Handshake.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            HandshakeMessageReceivedEvent?.Invoke(handshake);
            SteamNetworkingMessage_t.Release(handshakeMessages[i]);
            FlagSessionAlive(steamMsg.m_identityPeer);
        }

        //Identical to above, but for Lobby messages on the Lobby Channel
        nint[] lobbyMessages = new nint[nMaxLobbyMessagesPerFrame];
        int lobbyMessageNum = ReceiveMessagesOnChannel(LOBBY_CHANNEL, lobbyMessages, nMaxLobbyMessagesPerFrame);
        if (lobbyMessageNum > 0)
        {
            Global.Log($"Network Peer Received {lobbyMessageNum} Lobby messages this frame.");
        }

        for (int i = 0; i < lobbyMessageNum; i++)
        {

            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(lobbyMessages[i]); //Converts the message to a C# object
            LobbyMessage lobbyMessage = LobbyMessage.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            LobbyMessageReceivedEvent?.Invoke(lobbyMessage);
            SteamNetworkingMessage_t.Release(lobbyMessages[i]);
            FlagSessionAlive(steamMsg.m_identityPeer);
        }

    }

    private void HandleTimeoutsAndConnections(double delta)
    {
        if (remotePeers.Keys.Count <= 1)
        {
            return;
        }
        if (!PeersListStable)
        {
            if (PeersListStabilityTimer > PeersListStabilityThreshold)
            {
                PeerListStabilityChangedEvent.Invoke(true);
                PeersListStabilityTimer = 0;
                Global.Log("Peers list is now stable(ish).");
            }
            else
            {
                PeersListStabilityTimer += delta;
            }
        }

        foreach (var id in remotePeers.Keys)
        {
            //Update the time since last message for each peer
            remotePeers[id].TimeSinceLastMessage += delta;
            remotePeers[id].AliveCheckTimer += delta;

            //If the peer has not sent a message in a while, increment the alive check counter
            if (remotePeers[id].AliveCheckTimer > remotePeers[id].AliveCheckFrequency)
            {
                Global.Log($"No network messages from {remotePeers[id]} in {remotePeers[id].AliveCheckFrequency} seconds. Pinging.");
                HandshakePeer(id, "Ping");
                remotePeers[id].AliveCheckCounter++;
                remotePeers[id].AliveCheckTimer = 0;
            }

            //If the alive check counter exceeds the threshold, consider the peer unresponsive and remove it from the list
            if (remotePeers[id].AliveCheckCounter > remotePeers[id].AliveCheckUnresponsiveThreshold)
            {
                Global.Log("Peer " + id.GetSteamID64() + " is unresponsive, removing from list.");
                remotePeers.Remove(id);
                PlayerTimeoutEvent?.Invoke(id.GetSteamID64());
            }

        }
        foreach (var id in JoinRequestsCurrentlyOutTo.Keys)
        {
            JoinRequestsCurrentlyOutTo[id] += delta;
            if (JoinRequestsCurrentlyOutTo[id] > JoinRequestTimeoutTime)
            {
                Global.Log("NETWORK ERROR: Outgoing Join Request Timed Out.");
                JoinRequestsCurrentlyOutTo.Remove(id);
                JoinRequestTimeoutEvent?.Invoke(id.GetSteamID64());
            }
        }
        foreach (var id in PeerRequestsCurrentlyOutTo.Keys)
        {
            PeerRequestsCurrentlyOutTo[id] += delta;
            if (PeerRequestsCurrentlyOutTo[id] > PeerRequestTimeoutTime)
            {
                Global.Log("NETWORK ERROR: Outgoing Peer Request Timed Out.");
                PeerRequestsCurrentlyOutTo.Remove(id);
                PeerRequestTimeoutEvent?.Invoke(id.GetSteamID64());
            }
        }
    }

    /// <summary>
    /// Sever all connections to all peers and reset the list of peers we're tracking. This resets the state of the networking to a blank slate.
    /// </summary>
    public void DisconnectFromAllPeers()
    {
        foreach (SteamNetworkingIdentity i in remotePeers.Keys)
        {
            var i2 = i;
            SteamNetworkingMessages.CloseSessionWithUser(ref i2);
        }
        remotePeers.Clear();
    }

    /// <summary>
    /// Helper function to construct a handshake message that requests to join a peer.
    /// </summary>
    /// <param name="id">SteamID ulong to attempt to join to</param>
    public void JoinToPeer(ulong id)
    {
        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID64(id);
        JoinToPeer(identity);
    }

    /// <summary>
    /// Helper function to construct a handshake message that requests to join a peer.
    /// </summary>
    /// <param name="identity">Steamworks.NET formatted identity object corresponding to the peer you want to join.</param>
    private void JoinToPeer(SteamNetworkingIdentity identity)
    {
        Global.Log("Attempting to join peer: " + identity.GetSteamID64());
        JoinRequestsCurrentlyOutTo.Add(identity, 0);
        PeerListStabilityChangedEvent.Invoke(false);
        SendMessageToPeer(identity, new Handshake() { Sender = Global.clientID, Timestamp = Time.GetUnixTimeFromSystem(), Tick = Global.getTick(), Status = "JoinRequest" }, HANDSHAKE_CHANNEL);
    }

    /// <summary>
    /// Simple dereference function. 
    /// 
    /// (almost) All data that comes over the network gets pulled into usable memory here, prime candidate for performance concerns
    /// </summary>
    /// <param name="ptr"></param>
    /// <param name="cbSize"></param>
    /// <returns></returns>
    public static byte[] IntPtrToBytes(IntPtr ptr, int cbSize)
    {
        byte[] retval = new byte[cbSize];
        Marshal.Copy(ptr, retval, 0, cbSize);
        return retval;
    }

    /// <summary>
    /// 
    /// 
    /// (almost) All data that gets sent over the network gets pushed out of our application memory here, prime candidate for performance concerns
    /// </summary>
    /// <param name="remotePeer">peer to send to</param>
    /// <param name="message">Any protobuf formatted object</param>
    /// <param name="channel">message type channel to send on</param>
    /// <param name="sendFlags">IMPORTANT, indicates UDP or TCP-like behavior. handle with care.</param>
    public void SendMessageToPeer(SteamNetworkingIdentity remotePeer, IMessage message, int channel, int sendFlags = k_nSteamNetworkingSend_ReliableNoNagle)
    {
        //Convert the Protobuf object to a byte array, function provided by the protobuf library
        byte[] data = message.ToByteArray();

        //Allocate a block of memory to hold the message. I'm not sure this is the best way to do this. Steamworks.NET needs the pointer to be a nint.
        nint ptr = Marshal.AllocHGlobal(data.Length);

        //Fill that memory with the byte array-ified protobuf object
        Marshal.Copy(data, 0, ptr, data.Length);
        int size = data.Length;

        //Steamworks.Net/ Steamworks API function to send the message. See the Steamworks API
        EResult result = SendMessageToUser(ref remotePeer, ptr, (uint)data.Length, sendFlags, channel);

        if (result!=EResult.k_EResultOK)
        {
            Global.Log($"Error sending message to: {remotePeer.GetSteamID64()}. Error Status: " + result.ToString());
        }
    }

    /// <summary>
    /// Helper function to send a message to all peers in the remotePeers list. This is a wrapper around SendMessageToPeer that iterates through the list of peers and sends the message to each one.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="sendFlags"></param>
    public void MessageAllPeers(IMessage message, int channel, int sendFlags = k_nSteamNetworkingSend_ReliableNoNagle)
    {
        foreach (SteamNetworkingIdentity i in remotePeers.Keys)
        {
            SendMessageToPeer(i, message, channel, sendFlags);
        }
    }

    public List<ulong> GetConnectedPeers()
    {
        List<ulong> peers = new List<ulong>();
        foreach (SteamNetworkingIdentity i in remotePeers.Keys)
        {
            peers.Add(i.GetSteamID64());
        }
        return peers;
    }

    ///////////////////////////////////////////////////////////////////////////////////
    // HANDSHAKE MANAGER

    private void OnHandshakeMessageReceived(Handshake handshake)
    {
        //Global.Log("Handshake received from: " + handshake.Sender);
        SteamNetworkingIdentity id = new SteamNetworkingIdentity();
        id.SetSteamID64(handshake.Sender);
        switch (handshake.Status)
        {
            case "JoinRequest":
                Global.Log($"Join request received from: {handshake.Sender}, automatically accepting");
                PeerListStabilityChangedEvent.Invoke(false);
                remotePeers.Add(id, new PeerConnectionData());
                PlayerJoinedEvent?.Invoke(handshake.Sender);
                HandshakePeer(id, "JoinAccepted");
                break;
            case "JoinAccepted":
                PeerListStabilityChangedEvent.Invoke(false);
                Global.Log($"Join request accepted by: {handshake.Sender}. Requesting their peer list.");
                JoinRequestsCurrentlyOutTo.Remove(id);
                remotePeers.Add(id, new PeerConnectionData());
                JoinedToPlayerEvent?.Invoke(handshake.Sender);
                HandshakePeer(id, "PeersRequest");
                PeerRequestsCurrentlyOutTo.Add(id, 0);
                break;
            case "PeersRequest":
                Global.Log("Peers list request received from: " + handshake.Sender);
                Handshake handshake1 = new Handshake() { Sender = Global.clientID, Timestamp = Time.GetUnixTimeFromSystem(), Tick = Global.getTick(), Status = "PeersList" };
                foreach (var remotePeer in remotePeers.Keys)
                {
                    handshake1.Peers.Add(remotePeer.GetSteamID64());
                }
                SendMessageToPeer(id, handshake1, HANDSHAKE_CHANNEL);
                break;
            case "PeersList":
                PeerRequestsCurrentlyOutTo.Remove(id);
                PeerListStabilityChangedEvent.Invoke(false);
                HandshakeCompletedEvent?.Invoke(handshake.Sender);
                Global.Log("Peers list (total peers: " + handshake.Peers.Count + " ) received from: " + handshake.Sender);
                foreach (ulong peer in handshake.Peers)
                {
                    SteamNetworkingIdentity id2 = new SteamNetworkingIdentity();
                    id2.SetSteamID64(peer);
                    if (!remotePeers.Keys.Contains(id2) && peer != Global.clientID)
                    {
                        Global.Log($"Unhandshook peer (id:{peer}) reported - handshaking them.");
                        JoinToPeer(peer);
                    }
                }
                break;
            case "Leaving":
                Global.Log("Peer leaving: " + handshake.Sender);
                PeerListStabilityChangedEvent.Invoke(false);
                remotePeers.Remove(id);
                PlayerLeftEvent?.Invoke(handshake.Sender);
                break;
            case "Ping":
                Global.Log("Ping received from: " + handshake.Sender);
                HandshakePeer(id, "Pong");
                break;
            case "Pong":
                Global.Log("Pong received from: " + handshake.Sender);
                break;

            default:
                break;
        }
    }

    private void FlagSessionAlive(SteamNetworkingIdentity id)
    {
        if (remotePeers.ContainsKey(id))
        {
            remotePeers[id].TimeSinceLastMessage = 0;
            remotePeers[id].AliveCheckCounter = 0;
            remotePeers[id].AliveCheckTimer = 0;
        }
    }

    public void HandshakePeer(SteamNetworkingIdentity remotePeer, string status)
    {
        Handshake handshake = new Handshake() { Sender = Global.clientID, Timestamp = Time.GetUnixTimeFromSystem(), Tick = Global.getTick(), Status = status };
        SendMessageToPeer(remotePeer, handshake, HANDSHAKE_CHANNEL);
    }

    public void HandshakeAllPeers(string status)
    {
        foreach (SteamNetworkingIdentity i in remotePeers.Keys)
        {
            HandshakePeer(i, status);
        }
    }


    ///////////////////////////////////////////////////////////////////////////////////
    /// COMMAND MANAGER

    public void CommandAllPeers(Command cmd)
    {
        //CommandMessageReceivedEvent?.Invoke(cmd);
        MessageAllPeers(cmd, COMMAND_CHANNEL);
    }

    ///////////////////////////////////////////////////////////////////////////////////
    /// CHAT MANAGER

    public void ChatAllPeers(string message)
    {
        Chat chat = new Chat() { Message = message, Sender = Global.clientID };
        //ChatMessageReceivedEvent?.Invoke(chat);
        MessageAllPeers(chat, CHAT_CHANNEL);
    }

    ///////////////////////////////////////////////////////////////////////////////////
    /// LOBBY MANAGER



///////////////////////////////////////////////////////////////////////////////////
/// COMMAND MANAGER

    public void CommandAllPeersAndSelf(Command cmd)
    {
        CommandMessageReceivedEvent?.Invoke(cmd);
        MessageAllPeers(cmd, COMMAND_CHANNEL);
    }

    ///////////////////////////////////////////////////////////////////////////////////
    /// CHAT MANAGER

    public void ChatAllPeersAndSelf(string message)
    {
        Chat chat = new Chat() { Message = message, Sender = Global.clientID };
        ChatMessageReceivedEvent?.Invoke(chat);
        MessageAllPeers(chat, CHAT_CHANNEL);
    }

    internal void LobbyMessageAllPeersAndSelf(LobbyMessage lobbyMessage)
    {
        LobbyMessageReceivedEvent?.Invoke(lobbyMessage);
        MessageAllPeers(lobbyMessage, LOBBY_CHANNEL);
    }

    internal void SendLobbyMessageToPeer(LobbyMessage lobbyMessage, ulong to)
    {
       
        SteamNetworkingIdentity id = new SteamNetworkingIdentity();
        id.SetSteamID(new CSteamID(to));
        SendMessageToPeer(id, lobbyMessage, LOBBY_CHANNEL);
    }
}


