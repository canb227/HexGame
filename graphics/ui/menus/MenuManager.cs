using Godot;
using System;


public partial class MenuManager : Control
{

	Control CurrentMenu = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		ChangeMenu("res://graphics/ui/menus/mainmenu.tscn");
	}


	public void ChangeMenu(string scenePath)
    {
        CurrentMenu = (Control)Godot.ResourceLoader.Load<PackedScene>(scenePath).Instantiate();
    }

}
