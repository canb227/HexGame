using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class ResourceButton : Control
{
    public Hex hex;
    public ResourceType resourceType;
    public City city;
    private ResourcePanel resourcePanel;

    public ResourceButton(Hex hex, ResourceType resourceType, ResourcePanel resourcePanel, City city)
    {
        this.CustomMinimumSize = new Vector2(64, 64);
        this.resourcePanel = resourcePanel;
        this.resourceType = resourceType;
        this.hex = hex;
        this.city = city;
        Button resourceButton = new Button();
        resourceButton.IconAlignment = HorizontalAlignment.Center;
        resourceButton.ExpandIcon = true;
        resourceButton.CustomMinimumSize = new Vector2(64, 64);
        resourceButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://"+ResourceLoader.resources[resourceType].IconPath);
        resourceButton.Pressed += () => resourcePanel.SelectNewResource(this);
        resourceButton.SetScript(GD.Load("res://addons/tooltips_pro/scripts/tooltip_trigger.gd"));
        resourceButton.Set("origin", 3);
        resourceButton.Set("tooltip_alignment", 1);
        resourceButton.Set("overflow_mode", 1);
        resourceButton.Call("add_tooltipstring", "Hello here is the tooltip is holds information");
        AddChild(resourceButton);
    }
}
