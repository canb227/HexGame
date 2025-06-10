using Godot;
using System;


public partial class MenuManager : Control
{

	Control CurrentMenu = null;
    public Control loadingScreen;
    public Control mainMenu;
    public Lobby lobby;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Global.menuManager = this;
        loadingScreen = GetNode<Control>("LoadingScreen");
        mainMenu = GetNode<Control>("Mainmenu");
        lobby = GetNode<Lobby>("Lobby");

        mainMenu.Show();
	}

    
    public void LoadLobby()
    {
        ClearMenus();
        lobby.Show();
        lobby.CreateLobby();
    }

    public void JoinLobby(ulong id)
    {
        ClearMenus();
        lobby.Show();
        lobby.JoinLobby(id);
    }
    public void ChangeMenu(string scenePath)
    {
        if (CurrentMenu != null)
        {
            RemoveChild(CurrentMenu);
        }
        CurrentMenu = (Control)Godot.ResourceLoader.Load<PackedScene>(scenePath).Instantiate();
		AddChild(CurrentMenu);
    }

    internal void ClearMenus()
    {
        
        foreach (Control child in GetChildren())
        {
            child.Hide();
        }
    }
}
