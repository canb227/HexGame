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
    int nMaxStateMessagesPerFrame = 100;
    int nMaxHandshakeMessagesPerFrame = 100;
    int nMaxLobbyMessagesPerFrame = 100;

    /// <summary>
    /// List of all other network peers we are currently connected to. This will (should) only ever contain non-self peers that have confirmed our connection request. It may contain disconnected, unresponsive, or offline peers.
    /// </summary>
    List<SteamNetworkingIdentity> remotePeers = new List<SteamNetworkingIdentity>();

    /// <summary>
    /// Steamworks.NET/Steamworks API Callback. Is automagically triggered (by Steamworks.NET) when we receive a request from another Steam user to establish a connection. See the Steamworks API
    /// </summary>
    protected Callback<SteamNetworkingMessagesSessionRequest_t> MessageRequest;

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
    public const int STATE_CHANNEL = 0;
    public const int CHAT_CHANNEL = 1;
    public const int COMMAND_CHANNEL = 2;
    public const int HANDSHAKE_CHANNEL = 3;
    public const int LOBBY_CHANNEL = 4;

    public enum GamePrivacyMode { NONE = 0, OFFLINE = 1, PRIVATE = 2, FRIENDS = 3, PUBLIC = 4 };
    public GamePrivacyMode privacyMode = GamePrivacyMode.FRIENDS;

    public delegate void StateMessageReceived(State state);
    public static event StateMessageReceived StateMessageReceivedEvent;

    public delegate void ChatMessageReceived(Chat chat);
    public static event ChatMessageReceived ChatMessageReceivedEvent;

    public delegate void CommandMessageReceived(Command command);
    public static event CommandMessageReceived CommandMessageReceivedEvent;

    public delegate void HandshakeMessageReceived(Handshake handshake);
    public static event HandshakeMessageReceived HandshakeMessageReceivedEvent;

    public delegate void LobbyMessageReceived(LobbyMessage lobbyMessage);
    public static event LobbyMessageReceived LobbyMessageReceivedEvent;


    public delegate void PlayerJoined(ulong playerID);
    public static event PlayerJoined PlayerJoinedEvent;

    public delegate void PlayerLeft(ulong playerID);
    public static event PlayerLeft PlayerLeftEvent;

    // This is a Godot node, so it needs to be in the scenetree to work. There is no reason for this other than I thought it was neat.
    public override void _Ready()
    {
        Global.networkPeer = this;

        //Registers the automagical callback with the function to call when a message is received. See the Steamworks API
        MessageRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnMessageRequest);

        //Since Handshake messages are so closely related to networking, we handle them here
        HandshakeMessageReceivedEvent += OnHandshakeMessageReceived;
    }


    ///////////////////////////////////////////////////////////////////////////////////
    // CORE NETWORKING

    /// <summary>
    /// Triggered by the Steamworks API when we receive a request from another Steam user to establish a connection.
    /// </summary>
    /// <param name="param"></param>
    private void OnMessageRequest(SteamNetworkingMessagesSessionRequest_t param)
    {
        Global.Log("Connection request from: " + param.m_identityRemote.GetSteamID64());
        if (param.m_identityRemote.GetSteamID64() == Global.clientID)
        {
            Global.Log("Connection request from self, Rejected");
            return;
        }
        else if (remotePeers.Contains(param.m_identityRemote))
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
        //This is confusing at first glance. When we get the list of messages on each channel, the Steamworks API returns an array of pointers to the messages, each pointer a raw memory address pointing to the message.
        //nint is a special c# way to store a pointer as the raw value, so "stateMessages" is secretly a List<State>, we just have to remind it of that by casting those memory blocks back to their pre-transmission types.
        nint[] stateMessages = new nint[nMaxStateMessagesPerFrame];

        //ReceiveMessagesOnChannel has side effects. It returns the number of messages waiting on the channel (up to the max), then fills the given array (stateMessages, our buffer we made above) with the pointers to those messages.
        //Thus, this functions iterates through each message in stateMessages after filling it with messages.
        for (int i = 0; i < ReceiveMessagesOnChannel(STATE_CHANNEL, stateMessages, nMaxStateMessagesPerFrame); i++)
        {
            //Dereference each pointer, fetching the C# object Steamworks.NET stashed there when it was recieved. Steamworks.NET provides the function for this transformation, in both directions.
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(stateMessages[i]);

            //The Steamworks.net message wrapper object contains a pointer to a blob of raw bytes and an int indicating its size.
            //Dereference the pointer and aquire the blob
            byte[] messageBytes = IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize);

            //On the sender send of the equation we used the protobuf library to serialize the message object into a byte array. Now we use the same library to deserialize the byte array back into a C# object.
            State state = State.Parser.ParseFrom(messageBytes);

            //This is where we actually do something with the message. We can use the message as a normal C# object, and pass it to whatever function we want.
            //At the moment this is done using C# events but could be done with a function call.
            StateMessageReceivedEvent?.Invoke(state);

            //Release the message back to the Steamworks API. This frees up the memory used by the message.
            SteamNetworkingMessage_t.Release(stateMessages[i]);
        }

        //Identical to above, but for Chat messages on the Chat Channel
        nint[] chatMessages = new nint[nMaxChatMessagesPerFrame];
        for (int i = 0; i < ReceiveMessagesOnChannel(CHAT_CHANNEL, chatMessages, nMaxChatMessagesPerFrame); i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(chatMessages[i]); //Converts the message to a C# object
            Chat chat = Chat.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            ChatMessageReceivedEvent?.Invoke(chat);
            SteamNetworkingMessage_t.Release(chatMessages[i]);
        }

        //Identical to above, but for Command Messages on the Messages Channel
        nint[] commandMessages = new nint[nMaxCommandMessagesPerFrame];
        for (int i = 0; i < ReceiveMessagesOnChannel(COMMAND_CHANNEL, commandMessages, nMaxCommandMessagesPerFrame); i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(commandMessages[i]); //Converts the message to a C# object
            Command command = Command.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            CommandMessageReceivedEvent?.Invoke(command);
            SteamNetworkingMessage_t.Release(commandMessages[i]);
        }

        //Identical to above, but for Handshake messages on the Handshake Channel
        nint[] handshakeMessages = new nint[nMaxHandshakeMessagesPerFrame];
        for (int i = 0; i < ReceiveMessagesOnChannel(HANDSHAKE_CHANNEL, handshakeMessages, nMaxHandshakeMessagesPerFrame); i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(handshakeMessages[i]); //Converts the message to a C# object
            Handshake handshake = Handshake.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            HandshakeMessageReceivedEvent?.Invoke(handshake);
            SteamNetworkingMessage_t.Release(handshakeMessages[i]);
        }

        //Identical to above, but for Lobby messages on the Lobby Channel
        nint[] lobbyMessages = new nint[nMaxLobbyMessagesPerFrame];
        for (int i = 0; i < ReceiveMessagesOnChannel(LOBBY_CHANNEL, lobbyMessages, nMaxLobbyMessagesPerFrame); i++)
        {
            SteamNetworkingMessage_t steamMsg = SteamNetworkingMessage_t.FromIntPtr(lobbyMessages[i]); //Converts the message to a C# object
            LobbyMessage lobbyMessage = LobbyMessage.Parser.ParseFrom(IntPtrToBytes(steamMsg.m_pData, steamMsg.m_cbSize));
            LobbyMessageReceivedEvent?.Invoke(lobbyMessage);
            SteamNetworkingMessage_t.Release(lobbyMessages[i]);
        }

    }

    /// <summary>
    /// Sever all connections to all peers and reset the list of peers we're tracking. This resets the state of the networking to a blank slate.
    /// </summary>
    public void DisconnectFromAllPeers()
    {
        foreach (SteamNetworkingIdentity i in remotePeers)
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
        Global.Log("Attempting to join peer: " + id);
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
    public void SendMessageToPeer(SteamNetworkingIdentity remotePeer, IMessage message, int channel = STATE_CHANNEL, int sendFlags = k_nSteamNetworkingSend_ReliableNoNagle)
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
    }

    /// <summary>
    /// Helper function to send a message to all peers in the remotePeers list. This is a wrapper around SendMessageToPeer that iterates through the list of peers and sends the message to each one.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="sendFlags"></param>
    public void MessageAllPeers(IMessage message, int channel = STATE_CHANNEL, int sendFlags = k_nSteamNetworkingSend_ReliableNoNagle)
    {
        foreach (SteamNetworkingIdentity i in remotePeers)
        {
            SendMessageToPeer(i, message, channel, sendFlags);
        }
    }


    public List<ulong> GetConnectedPeers()
    {
        List<ulong> peers = new List<ulong>();
        foreach (SteamNetworkingIdentity i in remotePeers)
        {
            peers.Add(i.GetSteamID64());
        }
        return peers;
    }
    ///////////////////////////////////////////////////////////////////////////////////
    // HANDSHAKE MANAGER

    private void OnHandshakeMessageReceived(Handshake handshake)
    {
        Global.Log("Handshake received from: " + handshake.Sender);
        SteamNetworkingIdentity id = new SteamNetworkingIdentity();
        id.SetSteamID64(handshake.Sender);
        switch (handshake.Status)
        {
            case "JoinRequest":
                Global.Log("Join request received from: " + handshake.Sender);
                remotePeers.Add(id);
                PlayerJoinedEvent?.Invoke(handshake.Sender);
                HandshakePeer(id, "JoinAccepted");
                break;
            case "JoinAccepted":
                Global.Log("Join request accepted by: " + handshake.Sender);
                remotePeers.Add(id);
                PlayerJoinedEvent?.Invoke(handshake.Sender);
                HandshakePeer(id, "PeersRequest");
                break;
            case "PeersRequest":
                Global.Log("Peers list request received from: " + handshake.Sender);
                Handshake handshake1 = new Handshake() { Sender = Global.clientID, Timestamp = Time.GetUnixTimeFromSystem(), Tick = Global.getTick(), Status = "PeersList" };
                foreach (var remotePeer in remotePeers)
                {
                    handshake1.Peers.Add(remotePeer.GetSteamID64());
                }
                SendMessageToPeer(id, handshake1, HANDSHAKE_CHANNEL);
                break;
            case "PeersList":
                Global.Log("Peers list (total peers: " + handshake.Peers.Count + " ) received from: " + handshake.Sender);
                foreach (ulong peer in handshake.Peers)
                {
                    SteamNetworkingIdentity id2 = new SteamNetworkingIdentity();
                    id2.SetSteamID64(peer);
                    if (!remotePeers.Contains(id2) && peer != Global.clientID)
                    {
                        JoinToPeer(peer);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void HandshakePeer(SteamNetworkingIdentity remotePeer, string status)
    {
        Handshake handshake = new Handshake() { Sender = Global.clientID, Timestamp = Time.GetUnixTimeFromSystem(), Tick = Global.getTick(), Status = status };
        SendMessageToPeer(remotePeer, handshake, HANDSHAKE_CHANNEL);
    }

    public void HandshakeAllPeers(string status)
    {
        foreach (SteamNetworkingIdentity i in remotePeers)
        {
            HandshakePeer(i, status);
        }
    }


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
}


