using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityInfoPanel : Node3D
{
    private Game game;
    private GraphicManager graphicManager;
    public City city;

    public HBoxContainer cityInfoPanel;
    public ProductionQueueUIItem productionQueueUIItem;

    public CityInfoPanel(GraphicManager graphicManager, Game game)
    {
        this.game = game;
        this.graphicManager = graphicManager;
        cityInfoPanel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityInfoPanel.tscn").Instantiate<HBoxContainer>();


        AddChild(cityInfoPanel);
    }


    public void Update(UIElement element)
    {

    }

    public void CitySelected(City city)
    {
        this.city = city;
        cityInfoPanel.Visible = true;
        UpdateCityPanelInfo();
    }

    public void CityUnselected(City city)
    {
        this.city = null;
        cityInfoPanel.Visible = false;
    }

    public void UpdateCityPanelInfo()
    {
        if (cityInfoPanel.Visible && city != null)
        {
            //update the ui stuff
        }
    }

}
