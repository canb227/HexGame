using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class UnitWorldUI : Node3D
{
    private GraphicManager graphicManager;
    private Unit unit;

    private Node3D node;
    private SubViewport subViewPort;
    private Sprite3D sprite;

    private Area3D area;
    private CollisionShape3D collisionShape;

    public PanelContainer unitWorldUI;
    private ProgressBar unitHealthBar;
    private TextureRect unitIcon;

    private Panel enemyBorder;

    public UnitWorldUI(GraphicManager graphicManager, Unit unit)
    {
        this.graphicManager = graphicManager;
        this.unit = unit;

        node = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/UnitWorldUI.tscn").Instantiate<Node3D>();
        subViewPort = node.GetNode<SubViewport>("SubViewport");

        area = node.GetNode<Area3D>("MeshInstance3D/Area3D");
        collisionShape = node.GetNode<CollisionShape3D>("MeshInstance3D/Area3D/CollisionShape3D");
        area.InputRayPickable = true;
        area.InputEvent += UnitWorldUIEvent;
        area.MouseEntered += UnitWorldUIEntered;
        area.MouseExited += UnitWorldUIExited;


        unitWorldUI = node.GetNode<PanelContainer>("SubViewport/UnitWorldUI");
        unitHealthBar = unitWorldUI.GetNode<ProgressBar>("Button/UnitHealthBar");
        unitIcon = unitWorldUI.GetNode<TextureRect>("Button/UnitIcon");
        enemyBorder = unitWorldUI.GetNode<Panel>("EnemyBorder");

        unitIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + UnitLoader.unitsDict[unit.name].IconPath);

        AddChild(node);
        Transform3D newTransform = Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 12, (float)hexPoint.x);
        Transform = newTransform;
        Update();
    }

    private void UnitWorldUIEvent(Node camera, InputEvent IEvent, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (IEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                if (graphicManager.selectedObject != graphicManager.graphicObjectDictionary[unit.id])
                {
                    graphicManager.ChangeSelectedObject(unit.id, graphicManager.graphicObjectDictionary[unit.id]);
                    return;
                }
            }
            //unit clicked on
        }
    }

    private void UnitWorldUIExited()
    {
        Global.camera.blockClick = false;
    }

    private void UnitWorldUIEntered()
    {
        Global.camera.blockClick = true;
    }

    public void Selected()
    {

    }

    public void Update()
    {
        if (graphicManager.game.teamManager.GetEnemies(graphicManager.game.localPlayerTeamNum).Contains(unit.id))
        {
            enemyBorder.Visible = true;
        }
        else
        {
            enemyBorder.Visible = false;
        }
        unitHealthBar.Value = unit.health;
        Transform3D newTransform = Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        Transform = newTransform;
    }
}
