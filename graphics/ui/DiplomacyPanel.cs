using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class DiplomacyPanel : Control
{
    public Control tradeExportControl;
    private Label ActiveExportsLabel;
    private FlowContainer ActiveExportsFlowBox;
    private FlowContainer ActiveTradeFlowBox;
    private FlowContainer IncomingTradeFlowBox;


    private VBoxContainer CityList;

    private Button closeButton;

    public DiplomacyPanel()
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

            //active trade routes

            //incoming trade routes
        }
    }


    private void ExportFoodCheckBoxed(bool isOn, CheckButton exportFoodCheckBox, int sourceCityID, int targetCityID)
    {
        ActiveExportsLabel.Text = "Active Exports (" + Global.gameManager.game.localPlayerRef.exportCount + "/" + Global.gameManager.game.localPlayerRef.exportCap + ")";
        if (isOn)
        {
            Global.gameManager.game.localPlayerRef.NewExportRoute(sourceCityID, targetCityID, YieldType.food);
        }
        else
        {
            Global.gameManager.game.localPlayerRef.RemoveExportRoute(sourceCityID, targetCityID, YieldType.food);
        }
    }
}
