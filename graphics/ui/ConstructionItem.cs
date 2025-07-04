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
        float prodCost = unitInfo.ProductionCost;
        if (name == "Settler")
        {
            prodCost = unitInfo.ProductionCost + 30 * Global.gameManager.game.playerDictionary[city.teamNum].settlerCount;
        }
        turnsToBuild.Text = Math.Ceiling(prodCost / (city.yields.production + city.productionOverflow)).ToString();
        ProductionCost.Text = prodCost.ToString();
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

    public void UpdateBuildingItem(BuildingInfo buildingInfo, String itemName)
    {
        if (BuildingLoader.buildingsDict[itemName].PerCity != 0)
        {
            int count = city.CountString(itemName);
            foreach (ProductionQueueType queueItem in city.productionQueue)
            {
                if (queueItem.itemName == itemName)
                {
                    count += 1;
                }
            }
            if (itemName != "" && (count >= BuildingLoader.buildingsDict[itemName].PerCity))
            {
                constructionItem.Disabled = true;
            }
        }
        if (Global.gameManager.game.builtWonders.Contains(itemName))
        {
            constructionItem.Disabled = true;
        }
        List<Hex> validHexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, buildingInfo.DistrictType, 3, itemName);
        List<Hex> toRemove = new();
        foreach (ProductionQueueType queueItem in city.productionQueue)
        {
            foreach (Hex hex in validHexes)
            {
                if (queueItem.itemName == itemName && queueItem.targetHex.Equals(hex))
                {
                    toRemove.Add(hex);
                }
            }
        }
        foreach(Hex hex in toRemove)
        {
            validHexes.Remove(hex);
        }
        if (validHexes == null || validHexes.Count == 0)
        {
            constructionItem.Disabled = true;
        }
        else
        {
/*            foreach (ProductionQueueType queueItem in city.productionQueue)
            {
                if(validHexes.Contains(queueItem.targetHex) && BuildingLoader.buildingsDict.ContainsKey(queueItem.itemName))
                {
                    validHexes.Remove(queueItem.targetHex);
                }
            }*/
            if (validHexes.Count == 0)
            {
                constructionItem.Disabled = true;
            }
            else
            {
                constructionItem.Disabled = false;
            }
        }

        turnsToBuild.Text = Math.Ceiling(buildingInfo.ProductionCost / (city.yields.production + city.productionOverflow)).ToString();
        ProductionCost.Text = buildingInfo.ProductionCost.ToString();

        objectIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + buildingInfo.IconPath);
        objectName.Text = itemName;

        foreach (Control child in EffectListBox.GetChildren())
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

    private void AddItemToQueue(String name, bool isBuilding, bool isUnit)
    {
        if (Global.gameManager.game.localPlayerRef.turnFinished)
        {
            return;
        }
        if (isBuilding)
        {
            ((GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[city.id]).GenerateBuildingTargetingPrompt(name);
        }
        if (isUnit)
        {
            //city.AddToQueue(name, city.hex);
            Global.gameManager.AddToProductionQueue(city.id, name, city.hex); //networked command
        }
    }

}

