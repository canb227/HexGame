using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class ResourceButton : Control
{
    private Hex hex;
    private ResourceType resourceType;
    private ResourcePanel resourcePanel;

    public ResourceButton(Hex hex, ResourceType resourceType, ResourcePanel resourcePanel)
    {
        this.resourcePanel = resourcePanel;
        this.resourceType = resourceType;
        this.hex = hex;
        Button resourceButton = new Button();
        resourceButton.IconAlignment = HorizontalAlignment.Center;
        resourceButton.ExpandIcon = true;
        resourceButton.CustomMinimumSize = new Vector2(64, 64);
        resourceButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://"+ResourceLoader.resources[resourceType].IconPath);
        resourceButton.Pressed += () => resourcePanel.SelectNewResource();
        AddChild(resourceButton);
    }
}
