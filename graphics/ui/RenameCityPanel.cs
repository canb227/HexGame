using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class RenameCityPanel : Control
{
    private PanelContainer panelContainer;
    private LineEdit lineEdit;
    private City city;

    public RenameCityPanel(City city)
    {
        this.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        this.city = city;
        panelContainer = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/RenameCityPanel.tscn").Instantiate<PanelContainer>();
        lineEdit = panelContainer.GetNode<LineEdit>("LineEdit");
        lineEdit.TextSubmitted += (newText) => TextSubmitted(newText);
        
        AddChild(panelContainer);
    }   

    private void TextSubmitted(string newText)
    {
        city.name = newText;
        Global.gameManager.graphicManager.UpdateGraphic(city.id, GraphicUpdateType.Update);
        Global.gameManager.graphicManager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
        this.Visible = false;
        this.QueueFree();
    }
}
