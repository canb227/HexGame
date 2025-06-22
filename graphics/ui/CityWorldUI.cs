using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityWorldUI : Node3D
{
    private City city;
    private District cityCenter;

    private Node3D node;
    private SubViewport subViewPort;
    private Sprite3D sprite;

    private Area3D area;
    private CollisionShape3D collisionShape;

    public PanelContainer cityWorldUI;
    private Label citySizeLabel;
    private ProgressBar cityGrowthBar;

    private TextureRect productionIcon;
    private Label productionTurnsLeft;
    private ProgressBar productionBar;

    private TextureRect happinessIcon;
    private TextureRect cityIcon;
    private Label cityName;
    private ProgressBar cityHealth;
    public CityWorldUI(City city)
    {
        this.city = city;

        node = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityWorldUI.tscn").Instantiate<Node3D>();
        subViewPort = node.GetNode<SubViewport>("SubViewport");

        area = node.GetNode<Area3D>("MeshInstance3D/Area3D");
        collisionShape = node.GetNode<CollisionShape3D>("MeshInstance3D/Area3D/CollisionShape3D");
        area.InputRayPickable = true;
        area.InputEvent += CityWorldUIEvent;
        area.MouseEntered += CityWorldUIEntered;
        area.MouseExited += CityWorldUIExited;


        cityWorldUI = node.GetNode<PanelContainer>("SubViewport/CityWorldUI");

        citySizeLabel = cityWorldUI.GetNode<Label>("Button/CitySizeLabel");
        cityGrowthBar = cityWorldUI.GetNode<ProgressBar>("Button/CityGrowth");

        productionIcon = cityWorldUI.GetNode<TextureRect>("Button/ProductionIcon");
        productionTurnsLeft = cityWorldUI.GetNode<Label>("Button/ProductionTurnsLeft");
        productionBar = cityWorldUI.GetNode<ProgressBar>("Button/ProductionBar");

        happinessIcon = cityWorldUI.GetNode<TextureRect>("Button/HappinessIcon");
        cityIcon = cityWorldUI.GetNode<TextureRect>("Button/CityIcon");
        cityName = cityWorldUI.GetNode<Label>("Button/CityName");
        cityHealth = cityWorldUI.GetNode<ProgressBar>("Button/CityHealth");

        AddChild(node);

        StyleBoxFlat styleBox = cityWorldUI.GetThemeStylebox("panel") as StyleBoxFlat;
        GD.Print(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].teamColor);
        if (styleBox != null)
        {
            styleBox.BorderColor = new Godot.Color(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].teamColor);
            cityWorldUI.AddThemeStyleboxOverride("panel", styleBox);
        }

        Transform3D newTransform = Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (city.hex.r >> 1) + city.hex.q) % ggb.chunkSize - (city.hex.r >> 1);
        Hex modHex = new Hex(newQ, city.hex.r, -newQ - city.hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        Transform = newTransform;
        Update();
    }

    private void CityWorldUIEvent(Node camera, InputEvent IEvent, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (IEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                if (Global.gameManager.graphicManager.selectedObject != Global.gameManager.graphicManager.graphicObjectDictionary[city.id])
                {
                    Global.gameManager.graphicManager.ChangeSelectedObject(city.id, Global.gameManager.graphicManager.graphicObjectDictionary[city.id]);
                    return;
                }
            }
            //city clicked on
        }
    }

    private void CityWorldUIExited()
    {
        Global.camera.blockClick = false;
    }

    private void CityWorldUIEntered()
    {
        Global.camera.blockClick = true;
    }

    public void Selected()
    {

    }

    public void Update()
    {
        if(cityCenter == null)
        {
            foreach (District district in city.districts)
            {
                if (district.isCityCenter)
                {
                    cityCenter = district;
                }
            }
        }
        citySizeLabel.Text = city.naturalPopulation.ToString() + "(" + Math.Ceiling(((city.foodToGrow - city.foodStockpile) / city.yields.food)).ToString() + ")";
        cityGrowthBar.Value = (city.foodStockpile / city.foodToGrow * 100.0f);
        if (cityCenter != null)
        {
            cityHealth.Value = (cityCenter.health / cityCenter.maxHealth) * 100.0f; //TODO add update to when we take damage
        }
        if (city.teamNum != Global.gameManager.game.localPlayerTeamNum)
        {
            productionIcon.Visible = false;
            productionTurnsLeft.Visible = false;
            productionBar.Visible = false;
            cityGrowthBar.Visible = false;
            citySizeLabel.Text = city.naturalPopulation.ToString();
        }
        else if (city.productionQueue.Count > 0)
        {
            productionIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + city.productionQueue.First().productionIconPath);
            productionIcon.Visible = true;
            productionTurnsLeft.Text = Math.Ceiling(city.productionQueue.First().productionLeft / (city.yields.production + city.productionOverflow)).ToString();
            productionBar.Value = 100.0f - ((city.productionQueue.First().productionLeft / city.productionQueue.First().productionCost) * 100.0f);
        }
        else
        {
            productionIcon.Visible = false;
            productionTurnsLeft.Text = "0";
            productionBar.Value = 0.0f;
        }


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