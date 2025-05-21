using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class ProductionQueueUIItem : Node3D
{
    private Game game;
    private GraphicManager graphicManager;
    public City city;

    public PanelContainer productionQueueUIItem;

    public ProductionQueueUIItem(GraphicManager graphicManager, Game game)
    {
        this.game = game;
        this.graphicManager = graphicManager;
        productionQueueUIItem = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ProductionQueueUIItem.tscn").Instantiate<PanelContainer>();


        AddChild(productionQueueUIItem);
    }


    public void Update(UIElement element)
    {

    }

    public void CitySelected(City city)
    {
        this.city = city;
        productionQueueUIItem.Visible = true;
        UpdateCityPanelInfo();
    }

    public void CityUnselected(City city)
    {
        this.city = null;
        productionQueueUIItem.Visible = false;
    }

    public void UpdateCityPanelInfo()
    {
        if (productionQueueUIItem.Visible && city != null)
        {
            //update the ui stuff
        }
    }

}

