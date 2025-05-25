using Godot;
using System;
using Steamworks;

public partial class Lobby : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SteamFriends.SetRichPresence("status", "In a lobby");
		SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
	}

    public override void _ExitTree()
    {
        SteamFriends.ClearRichPresence();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

	public void OnInviteButtonPressed()
    {
        Global.debugLog("Invite button pressed");
        Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(Steamworks.SteamUser.GetSteamID());
    }

    public void OnStartGameButtonPressed()
    {
        Global.debugLog("Start game button pressed");
        
    }
}
