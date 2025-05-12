using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicDistrict : GraphicObject
{
    public District district;
    public Layout layout;
    public List<GraphicBuilding> graphicBuildings;
    private GraphicManager graphicManager;
    public GraphicDistrict(District district, Layout layout, GraphicManager graphicManager)
    {
        this.district = district;
        this.layout = layout;
        this.graphicManager = graphicManager;
        InitDistrict(district);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            QueueFree();
        }
    }

    private void InitDistrict(District district)
    {
        //TODO rework for multiple buildings
        foreach(Building building in district.buildings)
        {
            graphicManager.NewBuilding(building);
        }
        //district doesnt have its own mesh currently
    }

    public override void Unselected()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void Selected()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void _Process(double delta)
    {

    }
}