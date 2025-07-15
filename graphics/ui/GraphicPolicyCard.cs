using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class GraphicPolicyCard : Control
{
    public PolicyCard policyCard;
    public PolicyPanel policyPanel;
    public bool isBlank;
    public bool isForUnassignment;
    public bool isAssigned;

    public GraphicPolicyCard(PolicyCard policyCard, bool isBlank, bool isForUnassignment, bool isAssigned, PolicyPanel policyPanel)
    {
        this.policyCard = policyCard;
        this.isBlank = isBlank;
        this.isForUnassignment = isForUnassignment;
        this.isAssigned = isAssigned;
        this.policyPanel = policyPanel;
        this.CustomMinimumSize = new Vector2(136, 160);

        Button policyCardControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/PolicyCard.tscn").Instantiate<Button>();
        policyCardControl.Pressed += () => policyPanel.PolicyCardPressed(this);

        Label policyTitle = policyCardControl.GetNode<Label>("PolicyVBox/MarginContainer/TitleLabel");
        policyTitle.Text = policyCard.title;
        Label policyDescription = policyCardControl.GetNode<Label>("PolicyVBox/DescriptionMarginBox/DescriptionPanelBox/DescriptionLabel");
        policyDescription.Text = policyCard.description;

        if (policyCard.isMilitary)
        {
            if(isBlank)
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/blankmilitarypolicycard.png");
                policyDescription.Visible = false;

                StyleBoxFlat styleBox = new StyleBoxFlat();
                styleBox.SetBorderWidthAll(4);
                styleBox.BgColor = new Godot.Color(23, 23, 23, 1);
                policyCardControl.AddThemeStyleboxOverride("focus", styleBox);
            }
            else
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/militarypolicycard.png");
            }
        }
        else if(policyCard.isEconomic)
        {
            if (isBlank)
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/blankeconomicpolicycard.png");
                policyDescription.Visible = false;

                StyleBoxFlat styleBox = new StyleBoxFlat();
                styleBox.SetBorderWidthAll(4);
                styleBox.BgColor = new Godot.Color(23, 23, 23, 1);
                policyCardControl.AddThemeStyleboxOverride("focus", styleBox);
            }
            else
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/economicpolicycard.png");
            }
        }
        else if(policyCard.isDiplomatic)
        {
            if (isBlank)
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/blankdiplomaticpolicycard.png");
                policyDescription.Visible = false;

                StyleBoxFlat styleBox = new StyleBoxFlat();
                styleBox.SetBorderWidthAll(4);
                styleBox.BgColor = new Godot.Color(23, 23, 23, 1);
                policyCardControl.AddThemeStyleboxOverride("focus", styleBox);
            }
            else
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/diplomaticpolicycard.png");
            }
        }
        else if(policyCard.isHeroic)
        {
            if (isBlank)
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/blankheroicpolicycard.png");
                policyDescription.Visible = false;

                StyleBoxFlat styleBox = new StyleBoxFlat();
                styleBox.SetBorderWidthAll(4);
                styleBox.BgColor = new Godot.Color(23,23,23,1);
                policyCardControl.AddThemeStyleboxOverride("focus", styleBox);
            }
            else
            {
                policyCardControl.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/heroicpolicycard.png");
            }
        }


        FlowContainer policyEffects = policyCardControl.GetNode<FlowContainer>("PolicyVBox/EffectsMarginBox/EffectsFlowBox");

        //NEED TO FILL FLOW WITH EFFECTS


        AddChild(policyCardControl);
    }


}
