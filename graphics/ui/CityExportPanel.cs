using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityExportPanel : Node3D
{
    public City city;

    private HBoxContainer cityExports;
    private VBoxContainer cityList;

    public CityExportPanel()
    {
        cityExports = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityExportPanel.tscn").Instantiate<HBoxContainer>();
        cityExports.Visible = false;
        cityList = cityExports.GetNode<VBoxContainer>("CityExportPanel/CityExportBox/ExportScrollContainer/ExportVBox");

        AddChild(cityExports);
    }
    public void Update(UIElement element)
    {
    }
    public void UpdateCityExportPanel()
    {
        foreach (Control child in cityList.GetChildren())
        {
            child.QueueFree();
        }
        foreach(int cityID in Global.gameManager.game.localPlayerRef.cityList)
        {

        }
    }
}
