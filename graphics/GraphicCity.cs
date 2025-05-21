using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicCity : GraphicObject
{
    public City city;
    public Layout layout;
    public List<GraphicBuilding> graphicBuildings;
    private GraphicManager graphicManager;
    public CityWorldUI cityWorldUI;
    public GraphicCity(City city, Layout layout, GraphicManager graphicManager)
    {
        this.city = city;
        this.layout = layout;
        this.graphicManager = graphicManager;
        InitCity(city);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            QueueFree();
        }
        if(graphicUpdateType == GraphicUpdateType.Update)
        {
            cityWorldUI.Update();
        }
    }

    private void InitCity(City city)
    {
        cityWorldUI = new CityWorldUI(graphicManager, city);
        AddChild(cityWorldUI);
    }

    public override void Unselected()
    {
        GD.PushWarning("NOT IMPLEMENTED UNSELECT CITY");
    }

    public override void Selected()
    {
        GD.PushWarning("NOT IMPLEMENTED SELECT CITY");
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void _Process(double delta)
    {

    }
}