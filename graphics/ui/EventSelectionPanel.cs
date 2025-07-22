using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class EventSelectionPanel : Control
{
    public Control eventSelectionPanel;
    private VBoxContainer optionsVBox;
    private Label title;
    private Label description;

    public AncientRuins ancientRuins;

    public EventSelectionPanel()
    {
        eventSelectionPanel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EventSelectionPanel.tscn").Instantiate<Control>();
        AddChild(eventSelectionPanel);

        title = eventSelectionPanel.GetNode<Label>("EventPanel/EventScroll/EventMargin/VBoxContainer/title");
        description = eventSelectionPanel.GetNode<Label>("EventPanel/EventScroll/EventMargin/VBoxContainer/description");

        optionsVBox = eventSelectionPanel.GetNode<VBoxContainer>("EventPanel/EventScroll/EventMargin/VBoxContainer/options");
    }


    public void UpdateEventSelectionPanel(AncientRuins ancientRuins)
    {
        foreach (Control child in optionsVBox.GetChildren())
        {
            child.Free();
        }
        this.ancientRuins = ancientRuins;
        if(this.ancientRuins.activeEvent)
        {
            RuinsEvent currentEvent = AncientRuinsLoader.ruinsEventDict[ancientRuins.nextEventID];
            title.Text = currentEvent.title;
            description.Text = currentEvent.description;
            foreach (EventOption eventOption in currentEvent.options)
            {
                /*            MarginContainer marginContainer = new MarginContainer();
                            marginContainer.AddThemeConstantOverride*/
                Button button = new Button();
                button.Text = eventOption.optionText;
                button.Pressed += () => SetNextEventIDAndClose(eventOption);
                optionsVBox.AddChild(button);
            }
        }
        else
        {
            title.Text = "Mysterious Ruins";
            description.Text = "Use a Hero's Exploration Ability to delve into the secrets of these ancient ruins.";

            Button button = new Button();
            button.Text = "Close.";
            button.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
            optionsVBox.AddChild(button);
        }
    }

    private void SetNextEventIDAndClose(EventOption eventOption)
    {
        //networked message TODO

        //activate any effects from our choice
        eventOption.eventEffects.Invoke(Global.gameManager.game.localPlayerRef);
        //if random selection do it or just select the only result and set it
        RuinsEvent chosenEvent = AncientRuinsLoader.PickWeightedEvent(eventOption.nextEvents, ancientRuins);
        ancientRuins.nextEventID = chosenEvent.eventID; 

        //close window
        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
    }
}
