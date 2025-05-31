using Godot;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;

public partial class ConstructionItem : PanelContainer
{
    public City city;

    public Button constructionItem;
    private TextureRect objectIcon;
    private Label objectName;
    private Label turnsToBuild;
    private HBoxContainer EffectListBox;
    private Label ProductionCost;

    


    public ConstructionItem(City city, String name, bool isBuilding, bool isUnit)
    {
        this.city = city;
        constructionItem = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ProductionItem.tscn").Instantiate<Button>();
        objectIcon = constructionItem.GetNode<TextureRect>("ObjectIcon");
        objectName = constructionItem.GetNode<Label>("ObjectName");
        turnsToBuild = constructionItem.GetNode<Label>("TurnsToBuildBox/TurnsToBuild");
        EffectListBox = constructionItem.GetNode<HBoxContainer>("EffectListBox");
        ProductionCost = constructionItem.GetNode<Label>("HBoxContainer/ProductionCost");

        constructionItem.Pressed += () => AddItemToQueue(name, isBuilding, isUnit);


        if (isBuilding)
        {
            BuildingInfo buildingInfo = BuildingLoader.buildingsDict[name];
            UpdateBuildingItem(buildingInfo, name);
        }
        if(isUnit)
        {
            UnitInfo unitInfo = UnitLoader.unitsDict[name];
            UpdateUnitItem(unitInfo, name);
        }
        //constructionItem.Pressed += () => city.AddToQueue();

        AddChild(constructionItem);
    }

    public void UpdateUnitItem(UnitInfo unitInfo, String name)
    {
        if (Global.gameManager.game.mainGameBoard.gameHexDict[city.hex].ValidHexToSpawn(unitInfo, false, true))
        {
            constructionItem.Disabled = false;
        }
        else
        {
            constructionItem.Disabled = true;
        }
        objectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + unitInfo.IconPath);
        objectName.Text = name;
        turnsToBuild.Text = Math.Ceiling(unitInfo.ProductionCost / (city.yields.production + city.productionOverflow)).ToString();
        ProductionCost.Text = unitInfo.ProductionCost.ToString();
        if (unitInfo.MovementSpeed > 0)
        {
            HBoxContainer effectBox = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EffectBox.tscn").Instantiate<HBoxContainer>();
            TextureRect effectIcon = effectBox.GetNode<TextureRect>("EffectIcon");
            effectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/moveicon.png");
            Label effectValue = effectBox.GetNode<Label>("EffectValue");
            effectValue.Text = unitInfo.MovementSpeed.ToString();
            EffectListBox.AddChild(effectBox);
        }
        if (unitInfo.CombatPower > 0)
        {
            HBoxContainer effectBox = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EffectBox.tscn").Instantiate<HBoxContainer>();
            TextureRect effectIcon = effectBox.GetNode<TextureRect>("EffectIcon");
            effectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/sword.png");
            Label effectValue = effectBox.GetNode<Label>("EffectValue");
            effectValue.Text = unitInfo.CombatPower.ToString();
            EffectListBox.AddChild(effectBox);
        }
    }

    public void UpdateBuildingItem(BuildingInfo buildingInfo, String name)
    {
        if(city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes) == null || city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes).Count == 0)
        {
            constructionItem.Disabled = true;
        }
        else
        {
            constructionItem.Disabled = false;
        }
        objectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + buildingInfo.IconPath);
        objectName.Text = name;
        turnsToBuild.Text = Math.Ceiling(buildingInfo.ProductionCost / (city.yields.production + city.productionOverflow)).ToString();
        ProductionCost.Text = buildingInfo.ProductionCost.ToString();
        System.Type yieldType = buildingInfo.yields.GetType();

        foreach (FieldInfo field in yieldType.GetFields())
        {
            float value = (float)field.GetValue(buildingInfo.yields);
            if (value != 0)
            {
                string yieldName = field.Name; // Get the name of the yield (e.g., "food", "production")

                HBoxContainer effectBox = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EffectBox.tscn").Instantiate<HBoxContainer>();

                TextureRect effectIcon = effectBox.GetNode<TextureRect>("EffectIcon");
                effectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>($"res://graphics/ui/icons/{yieldName}.png");

                Label effectValue = effectBox.GetNode<Label>("EffectValue");
                effectValue.Text = value.ToString();

                EffectListBox.AddChild(effectBox);
            }
        }
    }

    private void AddItemToQueue(String name, bool isBuilding, bool isUnit)
    {
        if(isBuilding)
        {
            ((GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[city.id]).GenerateBuildingTargetingPrompt(name);
        }
        if (isUnit)
        {
            city.AddUnitToQueue(name);
        }
    }

}

