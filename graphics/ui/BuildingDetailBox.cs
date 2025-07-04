using Godot;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;

public partial class BuildingDetailBox : PanelContainer
{
    public City city;

    public PanelContainer buildingDetailItem;
    private TextureRect objectIcon;
    private Label objectName;
    private HBoxContainer EffectListBox;

    


    public BuildingDetailBox(City city, Building building)
    {
        this.city = city;
        buildingDetailItem = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/BuildingDetailItem.tscn").Instantiate<PanelContainer>();
        objectIcon = buildingDetailItem.GetNode<TextureRect>("HBoxContainer/ObjectIcon");
        objectName = buildingDetailItem.GetNode<Label>("HBoxContainer/VBoxContainer/ObjectName");
        EffectListBox = buildingDetailItem.GetNode<HBoxContainer>("HBoxContainer/VBoxContainer/EffectListBox");
        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[building.name];
        UpdateBuildingItem(buildingInfo, building.name);

        AddChild(buildingDetailItem);
    }

    public void UpdateBuildingItem(BuildingInfo buildingInfo, String name)
    {
        objectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + buildingInfo.IconPath);
        objectName.Text = name;
        System.Type yieldType = buildingInfo.yields.GetType();
        foreach(Control child in EffectListBox.GetChildren())
        {
            child.QueueFree();
        }

        Dictionary<string, float> yields = new Dictionary<string, float>
        {
            { "food", buildingInfo.yields.food },
            { "production", buildingInfo.yields.production },
            { "gold", buildingInfo.yields.gold },
            { "science", buildingInfo.yields.science },
            { "culture", buildingInfo.yields.culture },
            { "happiness", buildingInfo.yields.happiness },
            { "influence", buildingInfo.yields.influence }
        };

        foreach (KeyValuePair<string, float> kvp in yields)
        {
            if (kvp.Value != 0)
            {
                HBoxContainer effectBox = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EffectBox.tscn").Instantiate<HBoxContainer>();

                TextureRect effectIcon = effectBox.GetNode<TextureRect>("EffectIcon");
                effectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>($"res://graphics/ui/icons/{kvp.Key}.png");

                Label effectValue = effectBox.GetNode<Label>("EffectValue");
                effectValue.Text = kvp.Value.ToString();

                EffectListBox.AddChild(effectBox);

                //GD.Print($"{kvp.Key}: {kvp.Value}");
            }
        }
    }
}

