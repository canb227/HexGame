using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

public partial class ProductionQueueUIItem : PanelContainer
{
    public City city;
    private ProductionQueueType productionQueueItem;

    public PanelContainer productionQueueUIItem;
    private Button ProductionButton;
    private ProgressBar ProgressBar;
    private Label TurnsLeft;
    private Button CancelProduction;
    private TextureRect TextureRect;

    private int queueIndex;

    public ProductionQueueUIItem(City city, int index)
    {
        this.city = city;
        queueIndex = index;
        productionQueueUIItem = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ProductionQueueItem.tscn").Instantiate<PanelContainer>();
        ProductionButton = productionQueueUIItem.GetNode<Button>("ProductionButton");
        ProgressBar = ProductionButton.GetNode<ProgressBar>("ProgressBar");
        TurnsLeft = ProductionButton.GetNode<Label>("TurnsLeft");
        CancelProduction = ProductionButton.GetNode<Button>("CancelProduction");
        TextureRect = ProductionButton.GetNode<TextureRect>("TextureRect");
        if(city.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            ProductionButton.Pressed += () => MoveToFrontOfQueue();
            productionQueueItem = city.productionQueue[index];
            CancelProduction.Pressed += () => RemoveFromQueue(index);
        }

        TextureRect.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + productionQueueItem.productionIconPath);
        UpdateProgress();
        AddChild(productionQueueUIItem);
    }

    public void UpdateProgress()
    {
        ProgressBar.Value = 100.0f - ((productionQueueItem.productionLeft / productionQueueItem.productionCost) * 100.0f);
        if(queueIndex == 0)
        {
            TurnsLeft.Text = Math.Ceiling(productionQueueItem.productionLeft / (city.yields.production + city.productionOverflow)).ToString();
        }
        else
        {
            TurnsLeft.Text = Math.Ceiling(productionQueueItem.productionLeft / city.yields.production).ToString();
        }
    }

    public void UpdateIndex(int newIndex)
    {
        queueIndex = newIndex;
        productionQueueItem = city.productionQueue[newIndex];
        CancelProduction.Pressed += () => city.RemoveFromQueue(newIndex);
    }

    public void MoveToFrontOfQueue()
    {
        city.MoveToFrontOfProductionQueue(queueIndex);
    }

    public void RemoveFromQueue(int index)
    {
        city.RemoveFromQueue(index);
        QueueFree();
    }
}

