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
    public GraphicDistrict(District district, Layout layout)
    {
        this.district = district;
        this.layout = layout;
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
            Global.gameManager.graphicManager.NewBuilding(building);
        }
        //district doesnt have its own mesh currently
    }

    public override void Unselected()
    {
        GD.PushWarning("NOT IMPLEMENTED UNSELECT DISTRICT");
    }

    public override void Selected()
    {
    }

    public override void ProcessRightClick(Hex hex)
    {
    }

    public override void _Process(double delta)
    {

    }
    public override void RemoveTargetingPrompt()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }
}