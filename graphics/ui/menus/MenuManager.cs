using Godot;
using System;


public partial class MenuManager : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		ChangeMenu("res://scenes/Mainmenu.tscn");
	}


	public void ChangeMenu(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

}
