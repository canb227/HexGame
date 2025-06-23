using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class DistrictPickerPanel : Control
{
    public Control control;
    private VBoxContainer districtTypeVBox;
    private Button closeButton;
    GraphicCity targetGraphicCity;
    Hex targetHex;


    public DistrictPickerPanel(Hex targetHex, GraphicCity targetGraphicCity)
    {
        this.targetHex = targetHex;
        this.targetGraphicCity = targetGraphicCity;
        control = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/PickDistrictPanel.tscn").Instantiate<Control>();
        AddChild(control);

        districtTypeVBox = control.GetNode<VBoxContainer>("PickDistrictPanel/DistrictTypeVBox");
        closeButton = control.GetNode<Button>("CloseButton");

        closeButton.Pressed += () => CancelSelection();

        foreach (DistrictType type in Global.gameManager.game.playerDictionary[targetGraphicCity.city.teamNum].allowedDistricts)
        {
            if (type == DistrictType.refinement)
            {
                Button button = new Button();
                button.Text = "Refining District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.refinement);;
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.production)
            {
                Button button = new Button();
                button.Text = "Industrial District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.production);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.gold)
            {
                Button button = new Button();
                button.Text = "Commercial District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.gold);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.science)
            {
                Button button = new Button();
                button.Text = "Campus District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.science);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.culture)
            {
                Button button = new Button();
                button.Text = "Cultural District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.culture);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.happiness)
            {
                Button button = new Button();
                button.Text = "Entertainment District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.happiness);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.influence)
            {
                Button button = new Button();
                button.Text = "Administrative District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.influence);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.dock)
            {
                Button button = new Button();
                button.Text = "Harbor District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.dock);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.military)
            {
                Button button = new Button();
                button.Text = "Militaristic District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.military);
                districtTypeVBox.AddChild(button);
            }
        }

    }

    private void PrepareToBuildOnHex(DistrictType districtType)
    {
        Global.gameManager.DevelopDistrict(targetGraphicCity.city.id, targetHex, districtType);//networked command
        targetGraphicCity.waitingToGrow = false;
        Global.gameManager.graphicManager.Update2DUI(UIElement.endTurnButton);
        Global.gameManager.graphicManager.ClearWaitForTarget();
        this.QueueFree();
    }

    public void CancelSelection()
    {
        this.Visible = false;
    }


    public void Update(UIElement element)
    {
    }
}
