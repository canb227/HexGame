using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class CityExportPanel : PanelContainer
{
    public City city;

    private HBoxContainer cityExports;
    private VBoxContainer cityList;
    private Button closePanelButton;

    public CityExportPanel()
    {
        //this.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        cityExports = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityExportPanel.tscn").Instantiate<HBoxContainer>();
        this.AnchorBottom = cityExports.AnchorBottom;
        this.AnchorLeft = cityExports.AnchorLeft;
        this.AnchorRight = cityExports.AnchorRight;
        this.AnchorTop = cityExports.AnchorTop;
        this.OffsetBottom = cityExports.OffsetBottom;
        this.OffsetLeft = cityExports.OffsetLeft;
        this.OffsetRight = cityExports.OffsetRight;
        this.OffsetTop = cityExports.OffsetTop;
        cityList = cityExports.GetNode<VBoxContainer>("CityExportPanel/CityExportBox/ExportScrollContainer/ExportVBox");
        closePanelButton = cityExports.GetNode<Button>("CityExportPanel/CityExportBox/CloseCityExportBox/CloseCityExportButton");
        closePanelButton.Pressed += () => Global.gameManager.graphicManager.UnselectObject();
        AddChild(cityExports);
    }
    public void Update(UIElement element)
    {
    }
    public void UpdateCityExportPanel(City city)
    {
        this.city = city;
        foreach (Control child in cityList.GetChildren())
        {
            child.QueueFree();
        }
        foreach (int cityID in Global.gameManager.game.playerDictionary[city.teamNum].cityList)
        {
            if (cityID != city.id)
            {
                City targetCity = Global.gameManager.game.cityDictionary[cityID];
                HBoxContainer cityBox = new HBoxContainer();
                Label cityName = new Label();
                cityName.Text = targetCity.name;
                CheckButton exportFoodCheckBox = new CheckButton();
                GD.Print(city.id + " " + cityID);
                foreach(ExportRoute route in Global.gameManager.game.tradeExportManager.exportRouteHashSet)
                {
                    GD.Print(route.sourceCityID + " " + route.targetCityID + " " + route.exportType);
                }
                GD.Print(Global.gameManager.game.tradeExportManager.exportRouteHashSet.Contains(new ExportRoute(city.id, cityID, YieldType.food)));
                if (Global.gameManager.game.tradeExportManager.exportRouteHashSet.Contains(new ExportRoute(city.id, cityID, YieldType.food)))
                {
                    exportFoodCheckBox.SetPressedNoSignal(true);
                }
                exportFoodCheckBox.Text = "Export Surplus Food to this City";
                cityBox.AddChild(cityName);
                cityBox.AddChild(exportFoodCheckBox);
                exportFoodCheckBox.Toggled += (isOn) => ExportFoodCheckBoxed(isOn, exportFoodCheckBox, targetCity);
                cityList.AddChild(cityBox);
            }
        }
    }

    private void ExportFoodCheckBoxed(bool isOn, CheckButton exportFoodCheckBox, City targetCity)
    {
        if (isOn)
        {
            Global.gameManager.game.tradeExportManager.NewExportRoute(city.id, targetCity.id, YieldType.food);
        }
        else
        {
            Global.gameManager.game.tradeExportManager.RemoveExportRoute(city.id, targetCity.id, YieldType.food);
        }
    }
}
