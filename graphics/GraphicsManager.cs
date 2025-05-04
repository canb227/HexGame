using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicManager : Node3D
{
    Dictionary<int, GraphicObject> graphicObjectDictionary = new();
    Game game;
    Layout layout;
    public GraphicManager(Game game, Layout layout)
    {
        this.game = game;
        this.layout = layout;
        game.graphicManager = this;
        if (game.mainGameBoard != null)
        {
            NewGameBoard(game.mainGameBoard);
        }
    }

    public void NewGameBoard(GameBoard gameBoard)
    {
        GraphicGameBoard graphicGameBoard = new GraphicGameBoard(gameBoard, layout);
        AddChild(graphicGameBoard);
        graphicObjectDictionary.Add(graphicGameBoard.gameBoard.id, graphicGameBoard);
    }
}
