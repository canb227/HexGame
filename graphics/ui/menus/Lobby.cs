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

        NetworkPeer.PlayerJoinedEvent += OnPlayerJoinEvent;


	}

    public void OnLoadButtonPressed()
    {
        Godot.FileDialog dialog = new Godot.FileDialog();
        dialog.Mode = Godot.FileDialog.ModeEnum.Windowed;
        dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        dialog.Title = "Load Game";
        dialog.OkButtonText = "Load";
        dialog.UseNativeDialog = true;
        dialog.Show();
        
        AddChild(dialog);
        dialog.MoveToCenter();
        dialog.FileSelected += OnFileSelected;

    }

    private void OnFileSelected(string path)
    {
        string trimmedPath = path.Substring(path.LastIndexOf("/") + 1);
        Global.debugLog("File selected: " + trimmedPath);
        Global.gameManager.game = Global.gameManager.LoadGame(trimmedPath);
        Global.gameManager.startGame();
    }

    private void OnPlayerJoinEvent(ulong playerID)
    {
        throw new NotImplementedException();
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
        Global.gameManager.startGame();

        
    }
}
