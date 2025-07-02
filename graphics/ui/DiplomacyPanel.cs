using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class DiplomacyPanel : Control
{
    public Control diplomacyPanelControl;
    private TextureRect playerImage;
    private TextureRect otherImage;

    private VBoxContainer playerItems;
    private VBoxContainer playerOffer;
    private VBoxContainer otherOffer;
    private VBoxContainer otherItems;

    private Button closeButton;

    private List<DiplomacyAction> playerOffers;
    private List<DiplomacyAction> otherOffers;

    public DiplomacyPanel()
    {
        diplomacyPanelControl = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/DiplomacyPanel.tscn").Instantiate<Control>();
        AddChild(diplomacyPanelControl);

        playerItems = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/PlayerItemsScroll/PlayerItems");
        playerOffer = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/PlayerOfferScroll/PlayerOffer");
        otherOffer = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/OtherOfferScroll/OtherOffer");
        otherItems = diplomacyPanelControl.GetNode<VBoxContainer>("TradeExportPanel/TradeExportHBox/OtherItemsScroll/OtherItems");

        closeButton = diplomacyPanelControl.GetNode<Button>("CloseButton"); ;
        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        //UpdateDiplomacyPanel();
    }
    
    public void Update(UIElement element)
    {
    }

    public void UpdateDiplomacyPanel(int otherTeamNum)
    {
        playerOffers = new();
        otherOffers = new();
        //remove old stuff
        foreach (Control child in playerItems.GetChildren())
        {
            child.QueueFree();
        }
        foreach (Control child in playerOffer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (Control child in otherOffer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (Control child in otherItems.GetChildren())
        {
            child.QueueFree();
        }
        //items
        AddItems(playerItems, playerOffers);
        AddItems(otherItems, otherOffers);
        //offers
        //?

    }

    private void AddItems(VBoxContainer items, List<DiplomacyAction> offers)
    {
        Button goldButton = new Button();
        goldButton.Text = "Give Gold";
        DiplomacyAction gold = new DiplomacyAction(Global.gameManager.game.localPlayerTeamNum, "Give Gold");
        goldButton.Pressed += () => offers.Add(gold);
        items.AddChild(goldButton);

        Button goldPerTurnButton = new Button();
        goldPerTurnButton.Text = "Give Gold Per Turn";
        DiplomacyAction goldPerTurn = new DiplomacyAction(Global.gameManager.game.localPlayerTeamNum, "Give Gold Per Turn");
        goldButton.Pressed += () => offers.Add(goldPerTurn);
        items.AddChild(goldPerTurnButton);

        foreach (DiplomacyAction diplomacyAction in Global.gameManager.game.localPlayerRef.diplomaticActionHashSet)
        {
            //Button
            Button button = new Button();
            button.Text = diplomacyAction.actionName;
            goldButton.Pressed += () => offers.Add(diplomacyAction);
            items.AddChild(button);
        }
    }
}
