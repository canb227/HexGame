using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityInfoPanel : Node3D
{
    private Game game;
    private GraphicManager graphicManager;
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
         private ScrollContainer Purchase;
          private VBoxContainer PurchaseBox;
      private PanelContainer ProductionQueuePanel;
       private VBoxContainer ProductionQueue;
        private Label OffsetLabel;



    //private List<ConstructionItem> constructionItems;
    //private List<PurchaseItem> purchaseItems;
    private List<ProductionQueueUIItem> productionQueueUIItems;

    public CityInfoPanel(GraphicManager graphicManager, Game game)
    {
        this.game = game;
        this.graphicManager = graphicManager;
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
        Purchase = cityUI.GetNode<ScrollContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Purchase");
        PurchaseBox = cityUI.GetNode<VBoxContainer>("CityInfoPanel/CityInfoBox/ConstructionTabBox/Purchase/PurchaseBox");
        ProductionQueuePanel = cityUI.GetNode<PanelContainer>("ProductionQueuePanel");
        ProductionQueue = cityUI.GetNode<VBoxContainer>("ProductionQueuePanel/ProductionQueue");
        OffsetLabel = cityUI.GetNode<Label>("ProductionQueuePanel/ProductionQueue/OffsetLabel");

        CloseCityInfoButton.Pressed += () => graphicManager.UnselectObject();
        RenameCityButton.Pressed += () => RenameCity();
    }


    public void Update(UIElement element)
    {
    }

    public void CitySelected(City city)
    {
        this.city = city;
        cityInfoPanel.Visible = true;
        UpdateCityPanelInfo();
    }

    public void CityUnselected(City city)
    {
        this.city = null;
        cityInfoPanel.Visible = false;
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

            /*            foreach(ConstructionItem item in constructionItems)
                        {
                            ProductionBox.AddChild(item);
                        }
                        foreach (PurchaseItem item in purchaseItems)
                        {
                            PurchaseBox.AddChild(item);
                        }*/
            
            foreach (Node3D child in ProductionQueue.GetChildren())
            {
                if(child.Name != "OffsetLabel")
                {
                    child.Free();
                }
            }
            for (int i = 0; i < city.productionQueue.Count; i++)
            {
                ProductionQueue.AddChild(new ProductionQueueUIItem(graphicManager, city, i));
            }
            //update the ui stuff
        }
    }

    public void RenameCity()
    {
        throw new NotImplementedException();
    }
}
