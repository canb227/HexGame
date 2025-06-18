using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityInfoPanel : Node3D
{
    public City city;

     public HBoxContainer cityUI;
      public PanelContainer cityInfoPanel;
       private VBoxContainer CityInfoBox;
        private HBoxContainer CloseCityInfoBox;
         private Button CloseCityInfoButton;
        private HBoxContainer CityNameBox;
         private Label CityName; 
         private Button RenameCityButton;
        private HBoxContainer Yields;
         private VBoxContainer FoodYieldBox;
          private TextureRect FoodIcon;
          private Label FoodYield;
         private VBoxContainer ProductionYieldBox;
          private TextureRect ProductionIcon;
          private Label ProductionYield;
         private VBoxContainer GoldYieldBox;
          private TextureRect GoldIcon;
          private Label GoldYield;
         private VBoxContainer ScienceYieldBox;
          private TextureRect ScienceIcon;
          private Label ScienceYield;
         private VBoxContainer CultureYieldBox;
          private TextureRect CultureIcon;
          private Label CultureYield;
         private VBoxContainer HappinessYieldBox;
          private TextureRect HappinessIcon;
          private Label HappinessYield;
         private VBoxContainer InfluenceYieldBox;
          private TextureRect InfluenceIcon;
          private Label InfluenceYield;
        private TabContainer ConstructionTabBox;
         private ScrollContainer Production;
          private VBoxContainer ProductionBox;
            private Button UnitsButton;
            private TextureRect UnitArrowImage;
            private VBoxContainer UnitsBox;
            private Button BuildingsButton;
            private TextureRect BuildingArrowImage;
            private VBoxContainer BuildingsBox;
         private ScrollContainer Purchase;
          private VBoxContainer PurchaseBox;
      private PanelContainer ProductionQueuePanel;
       private VBoxContainer ProductionQueue;
        private Label OffsetLabel;


    private HBoxContainer cityDetails;
    private VBoxContainer buildingsList;


    private List<ConstructionItem> constructionItems;
    //private List<PurchaseItem> purchaseItems;
    private List<ProductionQueueUIItem> productionQueueUIItems;

    public CityInfoPanel()
    {
        cityUI = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityInfoPanel.tscn").Instantiate<HBoxContainer>();
        AddChild(cityUI);

        cityInfoPanel = cityUI.GetNode<PanelContainer>("CityInfoPanel");
        CityInfoBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox");
        CloseCityInfoBox = cityUI.GetNode<HBoxContainer>("CityInfoPanel/CityInfoBox/CloseCityInfoBox");
        CloseCityInfoButton = cityUI.GetNode<Button>("CityInfoPanel/CityInfoBox/CloseCityInfoBox/CloseCityInfoButton");
        CityNameBox = cityUI.GetNode<HBoxContainer>("CityInfoPanel/CityInfoBox/CityNameBox");
        CityName = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/CityNameBox/CityName");
        RenameCityButton = cityUI.GetNode<Button>("CityInfoPanel/CityInfoBox/CityNameBox/RenameCityButton");
        Yields = cityUI.GetNode<HBoxContainer>("CityInfoPanel/CityInfoBox/Yields");
        FoodYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/FoodYieldBox");
        FoodIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/FoodYieldBox/FoodIcon");
        FoodYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/FoodYieldBox/FoodYield");
        ProductionYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/ProductionYieldBox");
        ProductionIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/ProductionYieldBox/ProductionIcon");
        ProductionYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/ProductionYieldBox/ProductionYield");
        GoldYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/GoldYieldBox");
        GoldIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/GoldYieldBox/GoldIcon");
        GoldYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/GoldYieldBox/GoldYield");
        ScienceYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/ScienceYieldBox");
        ScienceIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/ScienceYieldBox/ScienceIcon");
        ScienceYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/ScienceYieldBox/ScienceYield");
        CultureYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/CultureYieldBox");
        CultureIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/CultureYieldBox/CultureIcon");
        CultureYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/CultureYieldBox/CultureYield");
        HappinessYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/HappinessYieldBox");
        HappinessIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/HappinessYieldBox/HappinessIcon");
        HappinessYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/HappinessYieldBox/HappinessYield");
        InfluenceYieldBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/Yields/InfluenceYieldBox");
        InfluenceIcon = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/Yields/InfluenceYieldBox/InfluenceIcon");
        InfluenceYield = cityUI.GetNode<Label>("CityInfoPanel/CityInfoBox/Yields/InfluenceYieldBox/InfluenceYield");
        ConstructionTabBox = cityUI.GetNode<TabContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox");

        Production = cityUI.GetNode<ScrollContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production");
        ProductionBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox");
        UnitsButton = cityUI.GetNode<Button>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox/UnitsButton");
        UnitArrowImage = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox/UnitsButton/UnitArrowImage");
        UnitsBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox/UnitsBox");
        BuildingsButton = cityUI.GetNode<Button>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox/BuildingsButton");
        BuildingArrowImage = cityUI.GetNode<TextureRect>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox/BuildingsButton/BuildingArrowImage");
        BuildingsBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Production/ProductionBox/BuildingsBox");

        Purchase = cityUI.GetNode<ScrollContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Purchase");
        PurchaseBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Purchase/PurchaseBox");

        ProductionQueuePanel = cityUI.GetNode<PanelContainer>("ProductionQueuePanel");
        ProductionQueue = ProductionQueuePanel.GetNode<VBoxContainer>("ScrollContainer/ProductionQueue");
        OffsetLabel = ProductionQueuePanel.GetNode<Label>("ScrollContainer/ProductionQueue/OffsetLabel");
        
        RenameCityButton.Pressed += () => RenameCity();
        CloseCityInfoButton.Pressed += () => Global.gameManager.graphicManager.UnselectObject();
        BuildingsButton.Pressed += () => ToggleBuildingsBoxVisibility();
        UnitsButton.Pressed += () => ToggleUnitBoxVisibility();

        cityDetails = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityDetailsPanel.tscn").Instantiate<HBoxContainer>();
        cityDetails.Visible = false;
        buildingsList = cityDetails.GetNode<VBoxContainer>("CityDetailsPanel/CityDetailsHBox/BuildingsList");
        AddChild(cityDetails);
    }


    public void Update(UIElement element)
    {
    }

    public void ToggleBuildingsBoxVisibility()
    {
        BuildingsBox.Visible = !BuildingsBox.Visible;
        BuildingArrowImage.FlipV = !BuildingArrowImage.FlipV;
    }

    public void ToggleUnitBoxVisibility()
    {
        UnitsBox.Visible = !UnitsBox.Visible;
        UnitArrowImage.FlipV = !UnitArrowImage.FlipV;
    }

    public void CitySelected(City city)
    {
        this.city = city;
        ShowCityInfoPanel();
    }

    public void ShowCityInfoPanel()
    {
        cityInfoPanel.Visible = true;
        cityDetails.Visible = true;
        Global.gameManager.graphicManager.uiManager.scienceButton.Visible = false;
        Global.gameManager.graphicManager.uiManager.cultureButton.Visible = false;
        Global.gameManager.graphicManager.uiManager.resourceButton.Visible = false;
        UpdateCityPanelInfo();
    }

    public void CityUnselected()
    {
        this.city = null;
        HideCityInfoPanel();
    }

    public void HideCityInfoPanel()
    {
        Global.gameManager.graphicManager.uiManager.scienceButton.Visible = true;
        Global.gameManager.graphicManager.uiManager.cultureButton.Visible = true;
        Global.gameManager.graphicManager.uiManager.resourceButton.Visible = true;
        cityInfoPanel.Visible = false;
        cityDetails.Visible = false;
        foreach (Control child in ProductionQueue.GetChildren())
        {
            if (child.Name != "OffsetLabel")
            {
                child.Free();
            }
        }
    }

    public void UpdateCityPanelInfo()
    {
        if (cityInfoPanel.Visible && city != null)
        {
            CityName.Text = city.name;
            FoodYield.Text = city.yields.food.ToString();
            ProductionYield.Text = city.yields.production.ToString();
            GoldYield.Text = city.yields.gold.ToString();
            ScienceYield.Text = city.yields.science.ToString();
            CultureYield.Text = city.yields.culture.ToString();
            HappinessYield.Text = city.yields.happiness.ToString();
            InfluenceYield.Text = city.yields.influence.ToString();

            if (city.teamNum == Global.gameManager.game.localPlayerTeamNum)
            {
                foreach (Control child in UnitsBox.GetChildren())
                {
                    child.QueueFree();
                }
                foreach (Control child in BuildingsBox.GetChildren())
                {
                    child.QueueFree();
                }
                RenameCityButton.Disabled = false;
                foreach (String itemName in Global.gameManager.game.playerDictionary[city.teamNum].allowedUnits)
                {
                    if (itemName != "")
                    {
                        ConstructionItem item = new ConstructionItem(city, itemName, false, true);
                        UnitsBox.AddChild(item);
                    }
                }
                foreach (String itemName in Global.gameManager.game.playerDictionary[city.teamNum].allowedBuildings)
                {
                    if (itemName != "")
                    {
                        ConstructionItem item = new ConstructionItem(city, itemName, true, false);
                        BuildingsBox.AddChild(item);
                    }
                }
            }
            else
            {
                RenameCityButton.Disabled = true;
            }
            /*foreach (PurchaseItem item in purchaseItems)
            {
                PurchaseBox.AddChild(item);
            }*/

            foreach (Control child in ProductionQueue.GetChildren())
            {
                if(child.Name != "OffsetLabel")
                {
                    child.QueueFree();
                }
            }
            for (int i = 0; i < city.productionQueue.Count; i++)
            {
                ProductionQueue.AddChild(new ProductionQueueUIItem(city, i));
            }

            //update the ui stuff
            foreach(Control child in buildingsList.GetChildren())
            {
                child.QueueFree();
            }
            foreach(District district in city.districts)
            {
                if(district.isUrban)
                {
                    foreach (Building building in district.buildings)
                    {
                        buildingsList.AddChild(new BuildingDetailBox(city, building));
                    }
                }
            }
        }
    }

    public void RenameCity()
    {
        if (city.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            RenameCityPanel renameCityPanel = new RenameCityPanel(city);
            AddChild(renameCityPanel);
        }
    }
}
