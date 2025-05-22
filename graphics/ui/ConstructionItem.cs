using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;

public partial class ConstructionItem : PanelContainer
{
    private GraphicManager graphicManager;
    public City city;

    public Button constructionItem;
    private TextureRect objectIcon;
    private Label objectName;
    private Label turnsToBuild;
    private HBoxContainer EffectListBox;

    


    public ConstructionItem(GraphicManager graphicManager, City city, String name, bool isBuilding, bool isUnit)
    {
        this.city = city;
        this.graphicManager = graphicManager;
        constructionItem = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ProductionItem.tscn").Instantiate<Button>();
        objectIcon = constructionItem.GetNode<TextureRect>("ObjectIcon");
        objectName = constructionItem.GetNode<Label>("ObjectName");
        turnsToBuild = constructionItem.GetNode<Label>("TurnsToBuild");
        EffectListBox = constructionItem.GetNode<HBoxContainer>("EffectListBox");

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
        objectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + unitInfo.IconPath);
        objectName.Text = name;
        turnsToBuild.Text = Math.Ceiling(unitInfo.ProductionCost / city.yields.production).ToString();
        if(unitInfo.MovementSpeed > 0)
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
        objectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + buildingInfo.IconPath);
        objectName.Text = name;
        turnsToBuild.Text = Math.Ceiling(buildingInfo.ProductionCost / city.yields.production).ToString();
    }

    private void AddItemToQueue(String name, bool isBuilding, bool isUnit)
    {
        if(isBuilding)
        {
            throw new NotImplementedException("Need a placement UI");
            //targetGameHex = ;
            //city.AddBuildingToQueue(name, targetGameHex);
        }
        if (isUnit)
        {
            city.AddUnitToQueue(name);
        }
    }

}

