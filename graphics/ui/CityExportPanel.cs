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
                Player player = (Player)Global.gameManager.game.playerDictionary[city.teamNum];
                if (player.exportRouteList.Contains(new ExportRoute(city.id, cityID, YieldType.food)))
                {
                    exportFoodCheckBox.SetPressedNoSignal(true);
                }
                if (player.exportCount >= player.exportCap && !exportFoodCheckBox.ButtonPressed)
                {
                    exportFoodCheckBox.Disabled = true;
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
        Player player = (Player)Global.gameManager.game.playerDictionary[city.teamNum];
        if (isOn)
        {
            player.NewExportRoute(city.id, targetCity.id, YieldType.food);
        }
        else
        {
            player.RemoveExportRoute(city.id, targetCity.id, YieldType.food);
        }
    }
}
