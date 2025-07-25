using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class UnitWorldUI : Node3D
{
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

    public UnitWorldUI(Unit unit)
    {
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
        unitHealthBar = node.GetNode<ProgressBar>("SubViewport/UnitWorldUI/VBoxContainer/UnitHealthBar");
        unitIcon = node.GetNode<TextureRect>("SubViewport/UnitWorldUI/VBoxContainer/MarginContainer/UnitIcon");
        enemyBorder = node.GetNode<Panel>("SubViewport/UnitWorldUI/EnemyBorder");

        unitIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + UnitLoader.unitsDict[unit.name].IconPath);

        AddChild(node);

        unitWorldUI.Theme = Global.gameManager.game.playerDictionary[unit.teamNum].theme;

        StyleBoxFlat styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Godot.Color(Global.gameManager.game.playerDictionary[unit.teamNum].teamColor);
        unitWorldUI.AddThemeStyleboxOverride("panel", styleBox);

        Transform3D newTransform = Transform;
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(unit.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        Transform = newTransform;
        Update();
    }

    private void UnitWorldUIEvent(Node camera, InputEvent IEvent, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (IEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                if (Global.gameManager.graphicManager.selectedObject != Global.gameManager.graphicManager.graphicObjectDictionary[unit.id])
                {
                    Global.gameManager.graphicManager.ChangeSelectedObject(unit.id, Global.gameManager.graphicManager.graphicObjectDictionary[unit.id]);
                    return;
                }
            }
            //unit clicked on
        }
    }

    private void UnitWorldUIExited()
    {
        Global.gameManager.graphicManager.camera.blockClick = false;
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
        if (Global.gameManager.game.teamManager.GetEnemies(Global.gameManager.game.localPlayerTeamNum).Contains(unit.teamNum))
        {
            enemyBorder.Visible = true;
        }
        else
        {
            enemyBorder.Visible = false;
        }
        unitHealthBar.Value = unit.health;
        if (unit.health < 30.0f)
        {
            StyleBoxFlat styleBox = new StyleBoxFlat();
            styleBox.BgColor = Godot.Colors.Red;
            unitHealthBar.AddThemeStyleboxOverride("fill", styleBox);
        }
        else if (unit.health < 60.0f)
        {
            StyleBoxFlat styleBox = new StyleBoxFlat();
            styleBox.BgColor = Godot.Colors.Yellow;
            unitHealthBar.AddThemeStyleboxOverride("fill", styleBox);
        }
        else
        {
            StyleBoxFlat styleBox = new StyleBoxFlat();
            styleBox.BgColor = Godot.Colors.Green;
            unitHealthBar.AddThemeStyleboxOverride("fill", styleBox);
        }
        Transform3D newTransform = Transform;
        Point hexPoint;
        if ((Global.gameManager.graphicManager.graphicObjectDictionary.ContainsKey(Global.gameManager.game.mainGameBoard.id)))
        {
            hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]).HexToGraphicHex(unit.hex));
        }
        else
        {
            hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(unit.hex);
        }
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        Transform = newTransform;
        if (unit.teamNum == Global.gameManager.game.localPlayerTeamNum && unit.remainingMovement > 0 && unit.currentPath.Count == 0 && !unit.isSleeping && !unit.isSkipping)
        {
            unitWorldUI.Modulate = new Color(1, 1, 1, 1.0f);
        }
        else
        {
            unitWorldUI.Modulate = new Color(1, 1, 1, 0.7f);
        }
    }
}
