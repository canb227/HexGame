using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;

public partial class DiplomacyPanel : Control
{
    public Control diplomacyPanelControl;
    private TextureRect playerImage;
    private TextureRect otherImage;

    private VBoxContainer playerItemsBox;
    private VBoxContainer playerOfferBox;
    private VBoxContainer otherOfferBox;
    private VBoxContainer otherItemsBox;

    private Button closeButton;
    private Button acceptButton;
    private Button declineButton;

    private List<DiplomacyAction> playerOffers;
    private List<DiplomacyAction> otherOffers;

    private int otherTeamNum;
    public bool activeOffer;
    private DiplomacyDeal currentOffer;
    public DiplomacyPanel()
    {
        diplomacyPanelControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/DiplomacyPanel.tscn").Instantiate<Control>();
        AddChild(diplomacyPanelControl);

        playerItemsBox = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/PlayerItemsPanel/PlayerItemsScroll/PlayerItems");
        playerOfferBox = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/PlayerOfferPanel/PlayerOfferScroll/PlayerOffer");
        otherOfferBox = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/OtherOfferPanel/OtherOfferScroll/OtherOffer");
        otherItemsBox = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/OtherItemsPanel/OtherItemsScroll/OtherItems");

        closeButton = diplomacyPanelControl.GetNode<Button>("CloseButton");
        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();

        acceptButton = diplomacyPanelControl.GetNode<Button>("AcceptButton");
        acceptButton.Pressed += () => ProcessDeal();

        declineButton = diplomacyPanelControl.GetNode<Button>("DeclineButton");
        declineButton.Pressed += () => DeclineDeal();

        //UpdateDiplomacyPanel();
    }

    public void Update(UIElement element)
    {
    }

    public void UpdateDiplomacyPanel(int otherTeamNum, DiplomacyDeal diplomaticOffer) //item 1 is what they are offering item 2 is what we are offering
    {
        this.otherTeamNum = otherTeamNum;
        playerOffers = new();
        otherOffers = new();
        //remove old stuff
        foreach (Control child in playerItemsBox.GetChildren())
        {
            child.Free();
        }
        foreach (Control child in playerOfferBox.GetChildren())
        {
            child.Free();
        }
        foreach (Control child in otherOfferBox.GetChildren())
        {
            child.Free();
        }
        foreach (Control child in otherItemsBox.GetChildren())
        {
            child.Free();
        }
        //items
        AddItems(Global.gameManager.game.localPlayerTeamNum, playerItemsBox, playerOfferBox, playerOffers);
        AddItems(otherTeamNum, otherItemsBox, otherOfferBox, otherOffers);
        //offers
        if (diplomaticOffer != null)
        {
            acceptButton.Text = "Accept";
            currentOffer = diplomaticOffer;
            activeOffer = true;
            foreach(DiplomacyAction action in diplomaticOffer.offersList)
            {
                AddOffer(action, otherItemsBox, otherOfferBox, otherOffers);
            }
            foreach (DiplomacyAction action in diplomaticOffer.requestsList)
            {
                AddOffer(action, playerItemsBox, playerOfferBox, playerOffers);
            }
            //add the offer to the visuals and set our offer lists
        }
        else
        {
            acceptButton.Text = "Offer";
        }
    }

    private void AddItems(int teamNum, VBoxContainer items, VBoxContainer offersBox, List<DiplomacyAction> offers)
    {
        foreach (DiplomacyAction diplomacyAction in Global.gameManager.game.playerDictionary[teamNum].diplomaticActionHashSet)
        {
            diplomacyAction.targetTeamNum = otherTeamNum;

            Button button = new Button();
            button.Name = diplomacyAction.actionName + "Button";
            button.Text = diplomacyAction.actionName;
            button.Disabled = !diplomacyAction.ActionValid(otherTeamNum);
            button.Pressed += () => AddVoidingOffer(diplomacyAction, items, offersBox, offers);
            items.AddChild(button);
        }
    }

    private void AddVoidingOffer(DiplomacyAction action, VBoxContainer items, VBoxContainer offersBox, List<DiplomacyAction> offers)
    {
        CancelActiveOffer();
        AddOffer(action, items, offersBox, offers);
    }
    private void AddOffer(DiplomacyAction action, VBoxContainer items, VBoxContainer offersBox, List<DiplomacyAction> offers)
    {
        items.GetNode<Button>(action.actionName + "Button").Disabled = true;
        HBoxContainer box = new HBoxContainer();
        box.Name = action.actionName;
        offers.Add(action);
        Button button = new Button();
        button.Text = action.actionName;
        button.Pressed += () => RemoveOffer(action, items, offers, offersBox);
        box.AddChild(button);
        if (action.hasQuantity)
        {
            LineEdit lineEdit = new LineEdit();
            lineEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            lineEdit.Text = action.quantity.ToString();
            lineEdit.TextChanged += (text) => SetActionQuantity(lineEdit, action, text);
            box.AddChild(lineEdit);
        }
        offersBox.AddChild(box);
    }

    private void RemoveOffer(DiplomacyAction action, VBoxContainer items, List<DiplomacyAction> offers, VBoxContainer offersBox)
    {
        items.GetNode<Button>(action.actionName + "Button").Disabled = false;
        CancelActiveOffer();
        offers.Remove(action);
        foreach (Node child in offersBox.GetChildren())
        {
            if(child.Name == action.actionName)
            {
                child.QueueFree();
            }
        }
    }

    public void DeclineDeal(int dealID)
    {
        Global.gameManager.game.teamManager.pendingDeals.Remove(currentOffer.id);
    }

    private void CancelActiveOffer()
    {
        //networked message
        DeclineDeal(currentOffer.id);

        if (activeOffer)
        {
            Global.gameManager.graphicManager.uiManager.RemoveDiplomaticDeal(currentOffer);
            acceptButton.Text = "Offer";
        }
        activeOffer = false;
    }

    private void ProcessDeal()
    {
        if(activeOffer)
        {
            AcceptDeal();
        }
        else
        {
            SendDeal();
        }
    }

    private void SendDeal()
    {

        DiplomacyDeal newOffer = new DiplomacyDeal(Global.gameManager.game.localPlayerTeamNum, otherTeamNum, playerOffers, otherOffers);
        //networked message
        Global.gameManager.game.teamManager.AddPendingDeal(newOffer);
        //

        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
    }

    private void AcceptDeal()
    {
        CancelActiveOffer();
        //networked message
        Global.gameManager.game.teamManager.ExecuteDeal(currentOffer.id);
        //
        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
    }
    private void DeclineDeal()
    {
        CancelActiveOffer();
        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
    }

    private void SetActionQuantity(LineEdit lineEdit, DiplomacyAction action, string text)
    {
        int quantity = text.ToInt();
        if(action.actionName == "Give Gold")
        {
            if (quantity > Global.gameManager.game.playerDictionary[action.teamNum].goldTotal)
            {
                quantity = (int)Global.gameManager.game.playerDictionary[action.teamNum].goldTotal;
                lineEdit.Text = quantity.ToString();
                action.quantity = quantity;
            }
            else
            {
                action.quantity = quantity;
            }
        }

        if(action.actionName == "Give Gold Per Turn")
        {
            if (quantity > Global.gameManager.game.playerDictionary[action.teamNum].GetGoldPerTurn())
            {
                quantity = (int)Global.gameManager.game.playerDictionary[action.teamNum].GetGoldPerTurn();
                lineEdit.Text = quantity.ToString();
                action.quantity = quantity;
            }
            else
            {
                action.quantity = quantity;
            }
        }

    }
}
