using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class TradeRoutePickerPanel : Control
{
    public Control tradeRoutePickerPanel;
    private Label ActiveExportsLabel;

    private VBoxContainer TradeRouteVBox;


    private VBoxContainer CityList;

    private Button closeButton;

    public TradeRoutePickerPanel()
    {
        tradeRoutePickerPanel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/TradeRoutePickerPanel.tscn").Instantiate<Control>();
        AddChild(tradeRoutePickerPanel);

        TradeRouteVBox = tradeRoutePickerPanel.GetNode<VBoxContainer>("TradeRoutePanel/TradeRouteVBox/TradeRouteScrollBox/TradeRouteVBox");

        closeButton = tradeRoutePickerPanel.GetNode<Button>("TradeRoutePanel/TradeRouteVBox/CloseTradeRouteBox/CloseTradeRouteButton"); ;
        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
    }

    public void UpdateTradeRoutePickerPanel(Unit unit)
    {
        foreach(Node child in TradeRouteVBox.GetChildren())
        {
            child.QueueFree();
        }
        foreach (Hex hex in unit.hex.WrappingRange(15, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            //we can see or have seen the hex
            if (Global.gameManager.game.playerDictionary[unit.teamNum].seenGameHexDict.ContainsKey(hex))
            {
                GameHex tempGameHex = Global.gameManager.game.mainGameBoard.gameHexDict[hex];
                //the hex has a city center on it thus is a city
                if (tempGameHex.district != null && tempGameHex.district.isCityCenter)
                {
                    //the city isnt ours or an enemies
                    if(!Global.gameManager.game.teamManager.GetEnemies(unit.teamNum).Contains(Global.gameManager.game.cityDictionary[tempGameHex.district.cityID].teamNum) && Global.gameManager.game.cityDictionary[tempGameHex.district.cityID].teamNum != unit.teamNum)
                    {
                        VBoxContainer tradeBox = new VBoxContainer();
                        Label label = new Label();
                        label.Text = "Player " + Global.gameManager.game.playerDictionary[unit.teamNum].teamNum + " - " + Global.gameManager.game.cityDictionary[tempGameHex.district.cityID].name;
                        tradeBox.AddChild(label);
                        HFlowContainer flowContainer = new HFlowContainer();

                        List<ResourceType> resources = new();
                        foreach (District district in Global.gameManager.game.cityDictionary[tempGameHex.district.cityID].districts)
                        {
                            if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType != ResourceType.None)
                            {
                                resources.Add(Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType);
                            }
                        }
                        foreach (ResourceType resource in resources)
                        {
                            TextureRect rect = new TextureRect();
                            rect.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[resource].IconPath);
                            flowContainer.AddChild(rect);
                        }
                        tradeBox.AddChild(flowContainer);
                        Button button = new Button();
                        button.Text = "Start Trade Route";
                        button.Pressed += () => Global.gameManager.NewTradeRoute(Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].district.cityID, tempGameHex.district.cityID);
                        tradeBox.AddChild(button);
                    }
                }
            }
        }
    }
}
