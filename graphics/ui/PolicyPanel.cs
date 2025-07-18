using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

public partial class PolicyPanel : Control
{
    public bool governmentPickerOpen = false;
    public GovernmentType targetGovernmentType;

    public Control policyControl;

    private FlowContainer AssignedMilitaryPolicyCards;
    private FlowContainer AssignedEconomicPolicyCards;
    private FlowContainer AssignedDiplomaticPolicyCards;
    private FlowContainer AssignedHeroicPolicyCards;

    private FlowContainer UnassignedPolicyCards;

    //government section
    private Label CurrentGovernmentLabel;
    private TextureRect CurrentGovernmentIcon;
    private Label CurrentGovernmentTitle;
    private Label CurrentGovernmentDescription;

    private Label AvaliableGovernmentLabel;
    private VBoxContainer AvaliableGovernmentVBox;

    //government popup
    private Control GovernmentSwitchPanel;

    private Label ToLabel;
    private TextureRect TargetGovernmentIcon;
    private Label TargetGovernmentTitle;
    private Label TargetGovernmentDescription;

    private Label FromLabel;
    private TextureRect FromGovernmentIcon;
    private Label FromGovernmentTitle;
    private Label FromGovernmentDescription;

    private Button acceptButton;
    private Button declineButton;


    private Button closeButton;

    public PolicyPanel()
    {
        policyControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/PolicyCardPanel.tscn").Instantiate<Control>();
        AddChild(policyControl);

        //policy section
        AssignedMilitaryPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedMilitaryVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");
        AssignedEconomicPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedEconomicVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");
        AssignedDiplomaticPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedDiplomaticVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");
        AssignedHeroicPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/AssignedPolicyMarginBox/ScrollContainer/VBoxContainer/AssignedHeroicVBox/AssignedPolicyInnerMarginBox/AssignedPolicyFlowBox");

        UnassignedPolicyCards = policyControl.GetNode<FlowContainer>("PolicyPanel/PolicyHBox/UnassignedPolicyMarginBox/UnassingedPolicyScroll/VBoxContainer/UnassignedPolicyBox");

        closeButton = policyControl.GetNode<Button>("CloseButton");

        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();


        //government section 
        CurrentGovernmentLabel = policyControl.GetNode<Label>("PolicyPanel/PolicyHBox/GovernmentMarginBox/VBoxContainer/CurrentGovernmentLabel");
        CurrentGovernmentIcon = policyControl.GetNode<TextureRect>("PolicyPanel/PolicyHBox/GovernmentMarginBox/VBoxContainer/CurrentGovernmentIcon");
        CurrentGovernmentTitle = policyControl.GetNode<Label>("PolicyPanel/PolicyHBox/GovernmentMarginBox/VBoxContainer/CurrentGovernmentTitle");
        CurrentGovernmentDescription = policyControl.GetNode<Label>("PolicyPanel/PolicyHBox/GovernmentMarginBox/VBoxContainer/CurrentGovernmentDescription");

        AvaliableGovernmentLabel = policyControl.GetNode<Label>("PolicyPanel/PolicyHBox/GovernmentMarginBox/VBoxContainer/GarovernmentSelectionScroll/AvaliableGovernmentVBox/AvaliableGovernmentLabel");
        AvaliableGovernmentVBox = policyControl.GetNode<VBoxContainer>("PolicyPanel/PolicyHBox/GovernmentMarginBox/VBoxContainer/GarovernmentSelectionScroll/AvaliableGovernmentVBox");

        //we make the buttons for avaliable governments

        //government popup
        GovernmentSwitchPanel = policyControl.GetNode<Control>("GovernmentSwitchPanelControl");

        ToLabel = policyControl.GetNode<Label>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/TargetGovernment/ToLabel");
        TargetGovernmentIcon = policyControl.GetNode<TextureRect>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/TargetGovernment/TargetGovernmentIcon");
        TargetGovernmentTitle = policyControl.GetNode<Label>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/TargetGovernment/TargetGovernmentTitle");
        TargetGovernmentDescription = policyControl.GetNode<Label>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/TargetGovernment/TargetGovernmentDescription");

        FromLabel = policyControl.GetNode<Label>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/CurrentGovernment/FromLabel");
        FromGovernmentIcon = policyControl.GetNode<TextureRect>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/CurrentGovernment/CurrentGovernmentIcon");
        FromGovernmentTitle = policyControl.GetNode<Label>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/CurrentGovernment/CurrentGovernmentTitle");
        FromGovernmentDescription = policyControl.GetNode<Label>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/Governments/CurrentGovernment/CurrentGovernmentDescription");

        acceptButton = policyControl.GetNode<Button>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/MarginContainer/HBoxContainer/Button");
        acceptButton.Pressed += () => AcceptGovernmentChange();
        declineButton = policyControl.GetNode<Button>("GovernmentSwitchPanelControl/GovernmentSwitchPanel/GovernmentSwitchVBox/MarginContainer/HBoxContainer/Button2");
        declineButton.Pressed += () => DeclineGovernmentChange();

        UpdatePolicyPanel();
    }
    
    public void PolicyCardPressed(GraphicPolicyCard policyCard)
    {
        if(currentCard != null && !policyCard.Equals(currentCard))
        {
            if(policyCard.isBlank && policyCard.isForUnassignment)
            {
                UnassignCurrentCard(Global.gameManager.game.localPlayerTeamNum, policyCard);
            }
            else if(policyCard.isBlank)
            {
                AssignCurrentPolicyCard(Global.gameManager.game.localPlayerTeamNum, policyCard);
            }
            else if(policyCard.Equals(currentCard) && policyCard.isAssigned)
            {
                UnassignCurrentCard(Global.gameManager.game.localPlayerTeamNum, policyCard);
            }
            else
            {
                SelectNewPolicy(policyCard);
            }
        }
        else
        {
            SelectNewPolicy(policyCard);
        }
    }

    public void UnassignCurrentCard(int teamNum, GraphicPolicyCard policyCard)
    {
        if(currentCard.isAssigned)
        {
            currentCard.isAssigned = false;
            //networked message
            Global.gameManager.UnassignPolicyCard(teamNum, PolicyCardLoader.policyCardXMLDictionary[currentCard.policyCard.title]);
            currentCard = null;
            UpdatePolicyPanel();
        }
    }
    public void AssignCurrentPolicyCard(int teamNum, GraphicPolicyCard targetCard)
    {
        if (currentCard != null && targetCard.isBlank && (currentCard.policyCard.SameType(targetCard.policyCard) || targetCard.policyCard.isHeroic))
        {
            currentCard.isAssigned = true;
            //networked message
            Global.gameManager.AssignPolicyCard(teamNum, PolicyCardLoader.policyCardXMLDictionary[currentCard.policyCard.title]);
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
            Global.Log("Selected: " + policyCard.policyCard.title);
            currentCard = policyCard;
        }
    }

    private void AcceptGovernmentChange()
    {
        //networked message
        Global.gameManager.SetGovernment(Global.gameManager.game.localPlayerTeamNum, targetGovernmentType);
        UpdatePolicyPanel();
        CloseGovernmentSwitchPanel();
    }

    private void DeclineGovernmentChange()
    {
        CloseGovernmentSwitchPanel();
    }

    private void OpenGovernmentSwitchPanel(GovernmentType targetGovernmentType)
    {
        this.targetGovernmentType = targetGovernmentType;
        FromGovernmentIcon.Texture = PlayerEffect.GetGovernmentTypeIcon(Global.gameManager.game.localPlayerRef.government);
        FromGovernmentTitle.Text = PlayerEffect.GetGovernmentTypeTitle(Global.gameManager.game.localPlayerRef.government);
        FromGovernmentDescription.Text = PlayerEffect.GetGovernmentTypeDescription(Global.gameManager.game.localPlayerRef.government);

        TargetGovernmentIcon.Texture = PlayerEffect.GetGovernmentTypeIcon(targetGovernmentType);
        TargetGovernmentTitle.Text = PlayerEffect.GetGovernmentTypeTitle(targetGovernmentType);
        TargetGovernmentDescription.Text = PlayerEffect.GetGovernmentTypeDescription(targetGovernmentType);

        GovernmentSwitchPanel.Visible = true;
        governmentPickerOpen = true;
    }

    public void CloseGovernmentSwitchPanel()
    {
        GovernmentSwitchPanel.Visible = false;
        governmentPickerOpen = false;
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
                GraphicPolicyCard gpc = new GraphicPolicyCard(policyCard, false, false, false, this);
                UnassignedPolicyCards.AddChild(gpc);
            }
            foreach (PolicyCard policyCard in Global.gameManager.game.localPlayerRef.activePolicyCards)
            {
                GraphicPolicyCard gpc = new GraphicPolicyCard(policyCard, false, false, true, this);
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
            int unassignedMilitarySlots = Global.gameManager.game.localPlayerRef.militaryPolicySlots;
            int unassignedEconomicSlots = Global.gameManager.game.localPlayerRef.economicPolicySlots;
            int unassignedDiplomaticSlots = Global.gameManager.game.localPlayerRef.diplomaticPolicySlots;
            int unassignedHeroicSlots = Global.gameManager.game.localPlayerRef.heroicPolicySlots;
            foreach (PolicyCard policyCard in Global.gameManager.game.localPlayerRef.activePolicyCards)
            {
                if(policyCard.isMilitary)
                {
                    unassignedMilitarySlots--;
                }
                else if(policyCard.isEconomic)
                {
                    unassignedEconomicSlots--;
                }
                else if(policyCard.isDiplomatic)
                {
                    unassignedDiplomaticSlots--;
                }
                else if(policyCard.isHeroic)
                {
                    unassignedHeroicSlots--;
                }
            }
            int count = 0;
            while (count < unassignedMilitarySlots)
            {
                AssignedMilitaryPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", null, null, true), true, false, true, this));
                count++;
            }

            count = 0;
            while (count < unassignedEconomicSlots)
            {
                AssignedEconomicPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", null, null, false, true), true, false, true, this));
                count++;
            }

            count = 0;
            while (count < unassignedDiplomaticSlots)
            {
                AssignedDiplomaticPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", null, null, false, false, true), true, false, true, this));
                count++;
            }

            count = 0;
            while (count < unassignedHeroicSlots)
            {
                AssignedHeroicPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", null, null, false, false, false, true), true, false, true, this));
                count++;
            }
            UnassignedPolicyCards.AddChild(new GraphicPolicyCard(new PolicyCard("", "", null, null, false, false, false, true), true, true, false, this));


            //government section
            CurrentGovernmentIcon.Texture = PlayerEffect.GetGovernmentTypeIcon(Global.gameManager.game.localPlayerRef.government);
            CurrentGovernmentTitle.Text = PlayerEffect.GetGovernmentTypeTitle(Global.gameManager.game.localPlayerRef.government);
            CurrentGovernmentDescription.Text = PlayerEffect.GetGovernmentTypeDescription(Global.gameManager.game.localPlayerRef.government);
            foreach (Control child in AvaliableGovernmentVBox.GetChildren())
            {
                child.QueueFree();
            }
            Label label = new Label();
            label.Text = "Avaliable Governments";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.AutowrapMode = TextServer.AutowrapMode.Word;
            label.CustomMinimumSize = new Vector2(1, 1);
            AvaliableGovernmentVBox.AddChild(label);
            foreach (GovernmentType governmentType in Global.gameManager.game.localPlayerRef.avaliableGovernments)
            {
                if(Global.gameManager.game.localPlayerRef.government != governmentType)
                {
                    Button govButton = new Button();
                    govButton.Icon = GD.Load<CompressedTexture2D>("res://graphics/ui/icons/diplomacy.png");
                    govButton.ExpandIcon = true;
                    govButton.Text = governmentType.ToString();
                    govButton.CustomMinimumSize = new Vector2(64, 64);
                    govButton.SizeFlagsHorizontal = SizeFlags.Fill;
                    govButton.Pressed += () => OpenGovernmentSwitchPanel(governmentType);
                    AvaliableGovernmentVBox.AddChild(govButton);
                }
            }
        }
    }
}
