using Godot;
using System;
using System.Collections.Generic;


public partial class MenuManager : Node
{

	Control CurrentMenu = null;
    Control CurrentPopup = null;
    public Lobby lobby = null;

    public Dictionary<string,Control> loadedMenus = new Dictionary<string,Control>();

    public const string UI_Mainmenu = "res://graphics/ui/menus/mainmenu.tscn";
    public const string UI_Pause = "res://graphics/ui/menus/PauseMenu.tscn";
    public const string UI_Lobby = "res://graphics/ui/menus/lobby.tscn";
    public const string UI_LoadingScreen = "res://graphics/ui/menus/LoadingScreen.tscn";


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Global.menuManager = this;
        ChangeMenu(UI_Mainmenu);
	}


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause") && Global.gameManager.gameStarted)
        {
            Global.menuManager.ChangeMenu(UI_Pause);
        }
        
    }


    public void LoadLobby()
    {
        ChangeMenu(UI_Lobby);
        lobby = ((Lobby)CurrentMenu);
        lobby.CreateLobby(); 
    }

    public void JoinLobby(ulong id)
    {
        ChangeMenu(UI_Lobby);
        lobby = ((Lobby)CurrentMenu);
        lobby.JoinLobby(id);
    }

    public void ChangeMenu(string scenePath)
    {
        ClearMenus();
        if (loadedMenus.ContainsKey(scenePath))
        {
            CurrentMenu = loadedMenus[scenePath];
            CurrentMenu.Show();
            CurrentMenu.MoveToFront();
        }
        else
        {
            CurrentMenu = (Control)Godot.ResourceLoader.Load<PackedScene>(scenePath).Instantiate();
            AddChild(CurrentMenu);
            CurrentMenu.Show();
            CurrentMenu.MoveToFront();
        }
    }

    internal void ClearMenus()
    {
        
        foreach (Control child in GetChildren())
        {
            child.Hide();
        }
    }

    internal void SpawnPopup(string scenePath)
    {
        if (CurrentPopup != null)
        {
            RemoveChild(CurrentPopup);
        }
        CurrentPopup = (Control)Godot.ResourceLoader.Load<PackedScene>(scenePath).Instantiate();
        AddChild(CurrentPopup);
    }


    public void ClearPopup()
    {
        if (CurrentPopup != null)
        {
            RemoveChild(CurrentPopup);
        }
    }
}
