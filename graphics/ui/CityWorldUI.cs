using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityWorldUI : Node3D
{
    private GraphicManager graphicManager;
    private City city;

    private Node3D node;
    private SubViewport subViewPort;
    private Sprite3D sprite;

    public PanelContainer cityWorldUI;
    private SplitContainer citySizeBox;
    private Label citySizeLabel;
    private ProgressBar cityGrowthBar;

    private SplitContainer productionBox;
    private TextureRect productionIcon;
    private Label productionTurnsLeft;
    private ProgressBar productionBar;

    private Label teamLabel;
    private TextureRect happinessIcon;
    private TextureRect cityIcon;
    private Label cityName;
    public CityWorldUI(GraphicManager graphicManager, City city)
    {
        this.graphicManager = graphicManager;
        this.city = city;

        node = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityWorldUI.tscn").Instantiate<Node3D>();
        subViewPort = node.GetNode<SubViewport>("SubViewport");
        sprite = node.GetNode<Sprite3D>("Sprite3D");


        cityWorldUI = node.GetNode<PanelContainer>("SubViewport/CityWorldUI");

        citySizeBox = cityWorldUI.GetNode<SplitContainer>("VBoxContainer/CitySizeBox");
        citySizeLabel = cityWorldUI.GetNode<Label>("VBoxContainer/CitySizeBox/CitySizeLabel");
        cityGrowthBar = cityWorldUI.GetNode<ProgressBar>("VBoxContainer/CitySizeBox/CityGrowth");

        productionBox = cityWorldUI.GetNode<SplitContainer>("VBoxContainer/ProductionBox");
        productionIcon = cityWorldUI.GetNode<TextureRect>("VBoxContainer/ProductionBox/ProductionIcon");
        productionTurnsLeft = cityWorldUI.GetNode<Label>("VBoxContainer/ProductionBox/HSplitContainer/ProductionTurnsLeft");
        productionBar = cityWorldUI.GetNode<ProgressBar>("VBoxContainer/ProductionBox/HSplitContainer/ProductionBar");

        teamLabel = cityWorldUI.GetNode<Label>("VBoxContainer/HBoxContainer/TeamLabel");
        happinessIcon = cityWorldUI.GetNode<TextureRect>("VBoxContainer/HBoxContainer/HappinessIcon");
        cityIcon = cityWorldUI.GetNode<TextureRect>("VBoxContainer/HBoxContainer/CityIcon");
        cityName = cityWorldUI.GetNode<Label>("VBoxContainer/CityName");
        AddChild(node);
        Transform3D newTransform = Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(city.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        Transform = newTransform;
        Update();
    }

    public override void _Input(InputEvent IEvent)
    {
        if (IEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            GD.Print("hello");
            subViewPort.SetInputAsHandled();
            cityWorldUI.AcceptEvent();
        }
    }

    public void Update()
    {
        citySizeLabel.Text = city.citySize.ToString() + "(" + Math.Ceiling(((city.foodToGrow - city.foodStockpile) / city.yields.food)).ToString() + ")";
        cityGrowthBar.Value = (city.foodStockpile / city.foodToGrow) * 100.0f;

        if (city.productionQueue.Count > 0)
        {
            productionIcon = city.productionQueue.First().productionIcon;
            productionTurnsLeft.Text = Math.Ceiling(city.productionQueue.First().productionLeft / city.yields.production).ToString();
            productionBar.Value = (city.productionQueue.First().productionLeft / city.productionQueue.First().productionCost)*100.0f;
        }
        

        teamLabel.Text = "Team " + city.teamNum.ToString();
        if (city.isCapital)
        {
            cityIcon = new TextureRect();
            cityIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/star.png");
            cityIcon.Visible = true;
        }
        else
        {
            cityIcon.Visible = false;
        }
        cityName.Text = city.name;
    }
}