using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class UIManager : Node3D
{
    public Button endTurnButton;
    private Game game;
    private Layout layout;
    private GraphicManager graphicManager;
    public UIManager(GraphicManager graphicManager, Game game, Layout layout)
    {
        this.game = game;
        this.layout = layout;
        this.graphicManager = graphicManager;
        SetupTurnUI();
    }

    private void SetupTurnUI()
    {
        endTurnButton = new Button();
        endTurnButton.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomRight);
        endTurnButton.OffsetTop = -100f;
        endTurnButton.OffsetLeft = -100f;
        endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/temp.png");
        endTurnButton.Pressed += endTurnButtonPressed;
        //endTurnButton.Size = new Vector2(500f, 500f);
        AddChild(endTurnButton);

    }
    private void endTurnButtonPressed()
    {
        game.turnManager.EndCurrentTurn(game.localPlayerTeamNum);
    }
}
