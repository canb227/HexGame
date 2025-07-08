using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class PolicyPanel : Control
{
    public Control policyControl;

    private FlowContainer AssignedMilitaryPolicyCards;
    private FlowContainer AssignedEconomicPolicyCards;
    private FlowContainer AssignedDiplomaticPolicyCards;
    private FlowContainer AssignedHeroicPolicyCards;

    private FlowContainer UnassignedPolicyCards;

    private Button UnassignedResourceButton;
    private HFlowContainer GlobalResources;


    private VBoxContainer CityList;

    private Button closeButton;

    public PolicyPanel()
    {
        policyControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/PolicyCardPanel.tscn").Instantiate<Control>();
        AddChild(policyControl);

        AssignedMilitaryPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedMilitaryVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");
        AssignedEconomicPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedEconomicVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");
        AssignedDiplomaticPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedDiplomaticVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");
        AssignedHeroicPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedHeroicVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");

        UnassignedPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/UnassignedPolicyMarginBox/UnassingedPolicyScroll/VBoxContainer/UnassignedPolicyBox");

        closeButton = policyControl.GetNode<Button>("CloseButton");

        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        UpdatePolicyPanel();
    }
    
    public void PolicyCardPressed(GraphicPolicyCard policyCard)
    {
        if(currentCard != null && !policyCard.Equals(currentCard))
        {
            GD.Print("outer");
            if(policyCard.isBlank && policyCard.isForUnassignment)
            {
                GD.Print("unassign");
                UnassignCurrentCard(policyCard);
            }
            else
            {
                GD.Print("assign");
                AssignCurrentPolicyCard(policyCard);
            }
        }
        else
        {
            GD.Print("select");
            SelectNewPolicy(policyCard);
        }
    }

    public void UnassignCurrentCard(GraphicPolicyCard policyCard)
    {
        Global.gameManager.game.localPlayerRef.activePolicyCards.Remove(currentCard.policyCard);
        Global.gameManager.game.localPlayerRef.unassignedPolicyCards.Add(currentCard.policyCard);
        currentCard = null;
        UpdatePolicyPanel();
    }
    public void AssignCurrentPolicyCard(GraphicPolicyCard targetCard)
    {
        Player localPlayer = Global.gameManager.game.localPlayerRef;
        if (currentCard != null && targetCard.isBlank && (currentCard.policyCard.SameType(targetCard.policyCard) || targetCard.policyCard.isHeroic))
        {
            Global.gameManager.game.localPlayerRef.activePolicyCards.Remove(currentCard.policyCard);
            Global.gameManager.game.localPlayerRef.activePolicyCards.Add(currentCard.policyCard);
            Global.gameManager.game.localPlayerRef.unassignedPolicyCards.Remove(currentCard.policyCard);
            currentCard = null;
            UpdatePolicyPanel();
        }

    }

    GraphicPolicyCard currentCard;
    public void SelectNewPolicy(GraphicPolicyCard policyCard)
    {
        if(policyCard.isBlank)
        {
            currentCard = null;
        }
        else
        {
            currentCard = policyCard;
        }
    }



    public void Update(UIElement element)
    {
    }

    public void UpdatePolicyPanel()
    {
        if (policyControl.Visible)
        {
            //update assigned policy cards
            foreach(Control child in AssignedMilitaryPolicyCards.GetChildren())
            {
                child.QueueFree();
            }
            foreach (Control child in AssignedEconomicPolicyCards.GetChildren())
            {
                child.QueueFree();
            }
            foreach (Control child in AssignedDiplomaticPolicyCards.GetChildren())
            {
                child.QueueFree();
            }
            foreach (Control child in AssignedHeroicPolicyCards.GetChildren())
            {
                child.QueueFree();
            }
            foreach(Control child in UnassignedPolicyCards.GetChildren())
            {
                child.QueueFree();
            }

            foreach (PolicyCard policyCard in Global.gameManager.game.localPlayerRef.unassignedPolicyCards)
            {
                GraphicPolicyCard gpc = new GraphicPolicyCard(policyCard, false, false, this);
                UnassignedPolicyCards.AddChild(gpc);
            }
            foreach (PolicyCard policyCard in Global.gameManager.game.localPlayerRef.activePolicyCards)
            {
                GraphicPolicyCard gpc = new GraphicPolicyCard(policyCard, false, false, this);
                if (policyCard.isMilitary)
                {
                    AssignedMilitaryPolicyCards.AddChild(gpc);
                }
                else if (policyCard.isEconomic)
                {
                    AssignedEconomicPolicyCards.AddChild(gpc);
                }
                else if (policyCard.isDiplomatic)
                {
                    AssignedDiplomaticPolicyCards.AddChild(gpc);
                }
                else if (policyCard.isHeroic)
                {
                    AssignedHeroicPolicyCards.AddChild(gpc);
                }
            }
            AssignedMilitaryPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", true), true, false, this));
            AssignedEconomicPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", false, true), true, false, this));
            AssignedDiplomaticPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", false, false, true), true, false, this));
            AssignedHeroicPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", false, false, false, true), true, false, this));
            UnassignedPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", false, false, false, true), true, true, this));

        }
    }
}
