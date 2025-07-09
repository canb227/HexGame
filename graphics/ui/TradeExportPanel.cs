using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class TradeExportPanel : Control
{
    public Control tradeExportControl;
    private Label ActiveExportsLabel;
    private FlowContainer ActiveExportsFlowBox;
    private FlowContainer ActiveTradeFlowBox;
    private FlowContainer IncomingTradeFlowBox;


    private VBoxContainer CityList;

    private Button closeButton;

    public TradeExportPanel()
    {
        tradeExportControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/TradeAndExportPanel.tscn").Instantiate<Control>();
        AddChild(tradeExportControl);

        ActiveExportsLabel = tradeExportControl.GetNode<Label>("TradeExportPanel/TradeExportHBox/ActiveExportsMarginContainer/ExportsVBox/ActiveExportsLabel");
        ActiveExportsFlowBox = tradeExportControl.GetNode<HFlowContainer>("TradeExportPanel/TradeExportHBox/ActiveExportsMarginContainer/ExportsVBox/ActiveExportsScrollBox/ActiveExportsFlowBox");
        ActiveTradeFlowBox = tradeExportControl.GetNode<HFlowContainer>("TradeExportPanel/TradeExportHBox/ActiveTradeMarginContainer/TradeVBox/ActiveTradeScrollBox/ActiveTradeFlowBox");
        IncomingTradeFlowBox = tradeExportControl.GetNode<HFlowContainer>("TradeExportPanel/TradeExportHBox/RecievingTradeMarginContainer/TradeVBox/ActiveTradeScrollBox/ActiveTradeFlowBox");

        closeButton = tradeExportControl.GetNode<Button>("CloseButton"); ;
        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        UpdateTradeExportPanel();
    }
    
    public void Update(UIElement element)
    {
    }

    public void UpdateTradeExportPanel()
    {
        if (tradeExportControl.Visible)
        {
            //active exports
            foreach (Control child in ActiveExportsFlowBox.GetChildren())
            {
                child.QueueFree();
            }
            ActiveExportsLabel.Text = "Active Exports ("+Global.gameManager.game.localPlayerRef.exportCount+"/"+Global.gameManager.game.localPlayerRef.exportCap+")";
            foreach (ExportRoute exportRoute in Global.gameManager.game.localPlayerRef.exportRouteList)
            {
                foreach (int cityID in Global.gameManager.game.localPlayerRef.cityList)
                {
                    if (exportRoute.sourceCityID == cityID)
                    {
                        HBoxContainer cityBox = new HBoxContainer();
                        Label cityName = new Label();
                        cityName.Text = Global.gameManager.game.cityDictionary[exportRoute.targetCityID].name;
                        CheckButton exportFoodCheckBox = new CheckButton();
                        exportFoodCheckBox.SetPressedNoSignal(true);
                        exportFoodCheckBox.Text = "Export Surplus Food to this City";
                        cityBox.AddChild(cityName);
                        cityBox.AddChild(exportFoodCheckBox);
                        exportFoodCheckBox.Toggled += (isOn) => ExportFoodCheckBoxed(isOn, exportFoodCheckBox, exportRoute.sourceCityID, exportRoute.targetCityID);
                        ActiveExportsFlowBox.AddChild(cityBox);
                    }
                }
            }

            //active/incoming trade routes
            foreach (TradeRoute tradeRoute in Global.gameManager.game.localPlayerRef.tradeRouteList)
            {
                FlowContainer tradeBox = new();
                foreach (District district in Global.gameManager.game.cityDictionary[tradeRoute.targetCityID].districts)
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType != ResourceType.None)
                    {
                        TextureRect resourceIcon = new();
                        resourceIcon.CustomMinimumSize = new Vector2(64, 64);
                        resourceIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType].IconPath);
                        tradeBox.AddChild(resourceIcon);
                    }
                }
                ActiveTradeFlowBox.AddChild(tradeBox);
            }
            //outgoing trade routes
            foreach (TradeRoute tradeRoute in Global.gameManager.game.localPlayerRef.outgoingTradeRouteList)
            {
                FlowContainer tradeBox = new();
                foreach (District district in Global.gameManager.game.cityDictionary[tradeRoute.targetCityID].districts)
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType != ResourceType.None)
                    {
                        TextureRect resourceIcon = new();
                        resourceIcon.CustomMinimumSize = new Vector2(64, 64);
                        resourceIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType].IconPath);
                        tradeBox.AddChild(resourceIcon);
                    }
                }
                IncomingTradeFlowBox.AddChild(tradeBox);
            }
        }
    }


    private void ExportFoodCheckBoxed(bool isOn, CheckButton exportFoodCheckBox, int sourceCityID, int targetCityID)
    {
        ActiveExportsLabel.Text = "Active Exports (" + Global.gameManager.game.localPlayerRef.exportCount + "/" + Global.gameManager.game.localPlayerRef.exportCap + ")";
        if (isOn)
        {
            Global.gameManager.NewExportRoute(sourceCityID, targetCityID, YieldType.food);
        }
        else
        {
            Global.gameManager.RemoveExportRoute(sourceCityID, targetCityID, YieldType.food);
        }
    }
}
