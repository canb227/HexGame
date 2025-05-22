using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicManager : Node3D
{
    public Dictionary<int, GraphicObject> graphicObjectDictionary;
    public Game game;
    public Layout layout;
    public GraphicObject selectedObject;
    public int selectedObjectID;
    public UIManager uiManager;
    private bool waitForTargeting = false;
    public UnitAbility waitingAbility;

    public GraphicManager(Game game, Layout layout)
    {
        graphicObjectDictionary = new();
        this.game = game;
        this.layout = layout;
        uiManager = new UIManager(this, game, layout);
        uiManager.Name = "UIManager";
        AddChild(uiManager);
        game.graphicManager = this;
        if (game.mainGameBoard != null)
        {
            NewGameBoard(game.mainGameBoard);
        }
    }

    public void NewGameBoard(GameBoard gameBoard)
    {
        GraphicGameBoard graphicGameBoard = new GraphicGameBoard(gameBoard, this, layout);
        AddChild(graphicGameBoard);
        graphicObjectDictionary.Add(graphicGameBoard.gameBoard.id, graphicGameBoard);
    }

    public void NewUnit(Unit unit)
    {
        GraphicUnit graphicUnit = new GraphicUnit(unit, this);
        graphicObjectDictionary.Add(graphicUnit.unit.id, graphicUnit);
        AddChild(graphicUnit);
    }

    public void NewDistrict(District district)
    {
        GraphicDistrict graphicDistrict = new GraphicDistrict(district, layout, this);
        graphicObjectDictionary.Add(graphicDistrict.district.id, graphicDistrict);
        AddChild(graphicDistrict);
    }

    public void NewBuilding(Building building)
    {
        GraphicBuilding graphicBuilding = new GraphicBuilding(building, layout);
        graphicObjectDictionary.Add(graphicBuilding.building.id, graphicBuilding);
        AddChild(graphicBuilding);
    }

    public void NewCity(City city)
    {
        GraphicCity graphicCity = new GraphicCity(city, layout, this);
        graphicObjectDictionary.Add(graphicCity.city.id, graphicCity);
        AddChild(graphicCity);
    }
    public void UpdateGraphic(int id, GraphicUpdateType graphicUpdateType)
    {
        if (graphicObjectDictionary.ContainsKey(id))
        {
            graphicObjectDictionary[id].UpdateGraphic(graphicUpdateType);
        }
        else
        {
            throw new Exception("No Graphic Object Associated With ID of: " + id);
        }
    }

    public void Update2DUI(UIElement element)
    {
        uiManager.Update(element);
    }

    public void StartNewTurn()
    {
        if(selectedObject != null)
        {
            selectedObject.Unselected();
            selectedObject = null;
        }
    }

    public void ChangeSelectedObject(int newID, GraphicObject newSelectedObject)
    {
        if(selectedObject != null)
        {
            selectedObject.Unselected();
        }
        selectedObject = newSelectedObject;
        selectedObjectID = newID;
        selectedObject.Selected();
    }

    public void UnselectObject()
    {
        if (selectedObject != null)
        {
            selectedObject.Unselected();
        }
        selectedObject = null;
        selectedObjectID = 0;
    }

    public void SetWaitForTargeting(bool waitForTargeting)
    {
        //if we were waiting and now arent, update UI stuff
        if (this.waitForTargeting && !waitForTargeting)
        {
            ((GraphicUnit)graphicObjectDictionary[waitingAbility.usingUnit.id]).RemoveTargetingPrompt();
        }
        this.waitForTargeting = waitForTargeting;
    }

    public bool GetWaitForTargeting()
    {
        return waitForTargeting;
    }
}
