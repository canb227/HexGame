using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class ResourcePanel : Control
{
    public Control resourceControl;
    private HFlowContainer UnassignedResourcesBox;
    private Button UnassignedResourceButton;
    private HFlowContainer GlobalResources;


    private VBoxContainer CityList;

    private Button closeButton;

    public ResourcePanel()
    {
        resourceControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourcePanel.tscn").Instantiate<Control>();
        AddChild(resourceControl);

        UnassignedResourcesBox = resourceControl.GetNode<HFlowContainer>("ResourcePanel/ResourceHBox/UnassignedResourcesMarginBox/VBoxContainer/UnassingedResourcesScroll/UnassignedResourcesBox");
        //UnassignedResourceButton = resourceControl.GetNode<Button>("");
        GlobalResources = resourceControl.GetNode<HFlowContainer>("ResourcePanel/ResourceHBox/MarginContainer3/VBoxContainer/MarginContainer2/GlobalResources");

        CityList = resourceControl.GetNode<VBoxContainer>("ResourcePanel/ResourceHBox/MarginContainer3/VBoxContainer/MarginContainer/CityScroll/CityList");

        foreach(int cityIndex in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].cityList)
        {
            ResourceCityPanel rCP = new ResourceCityPanel(Global.gameManager.game.cityDictionary[cityIndex], this);
            CityList.AddChild(rCP);
        }

        closeButton = resourceControl.GetNode<Button>("CloseButton"); ;

        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        UpdateResourcePanel();
    }
    

    public void AssignCurrentResource(City city)
    {
        Player localPlayer = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum];
        if (currentButton != null && currentButton.resourceType != ResourceType.None)
        {
            if(currentButton.city != null)
            {
                //localPlayer.RemoveResource(currentButton.hex);
                Global.gameManager.RemoveResourceAssignment(Global.gameManager.game.localPlayerTeamNum, currentButton.hex);
            }
            if(city != null)
            {
                //localPlayer.AddResource(currentButton.hex, currentButton.resourceType, city);
                Global.gameManager.AddResourceAssignment(city.id, currentButton.resourceType, currentButton.hex);
            }
            currentButton.hex = new Hex(0, 0, 0);
            currentButton.resourceType = ResourceType.None;
            currentButton.city = null;
            UpdateResourcePanel();

        }

    }

    ResourceButton currentButton;

    public void SelectNewResource(ResourceButton resourceButton)
    {
        currentButton = resourceButton;
    }


    public void Update(UIElement element)
    {
    }

    public void UpdateResourcePanel()
    {
        if (resourceControl.Visible)
        {
            //update settlements list
            foreach (Control child in CityList.GetChildren())
            {
                child.QueueFree();
            }
            foreach (int cityIndex in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].cityList)
            {
                ResourceCityPanel rCP = new ResourceCityPanel(Global.gameManager.game.cityDictionary[cityIndex], this);
                CityList.AddChild(rCP);
            }

            //Update Unassigned Resources
            foreach (Control child in UnassignedResourcesBox.GetChildren())
            {
                child.QueueFree();
            }
            foreach (KeyValuePair<Hex, ResourceType> unassignedResource in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].unassignedResources)
            {
                ResourceButton rb = new ResourceButton(unassignedResource.Key, unassignedResource.Value, this, null);
                UnassignedResourcesBox.AddChild(rb);
            }
            Button emptyButton = new Button();
            emptyButton.IconAlignment = HorizontalAlignment.Center;
            emptyButton.ExpandIcon = true;
            emptyButton.CustomMinimumSize = new Vector2(64, 64);
            emptyButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://.godot/imported/health.png-838c7fec5d46427d7a66a7729e2f7b98.ctex");
            emptyButton.Pressed += () => AssignCurrentResource(null);
            UnassignedResourcesBox.AddChild(emptyButton);

            //Update Global Resources
            foreach (Control child in GlobalResources.GetChildren())
            {
                child.QueueFree();
            }
            foreach (KeyValuePair<Hex, ResourceType> globalResource in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].globalResources)
            {
                TextureRect globalIcon = new TextureRect();//Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourceGlobalIcon.tscn").Instantiate<TextureRect>();
                globalIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[globalResource.Value].IconPath);
                globalIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
                globalIcon.StretchMode = TextureRect.StretchModeEnum.Scale;
                GlobalResources.AddChild(globalIcon);
            }
        }
    }
}
