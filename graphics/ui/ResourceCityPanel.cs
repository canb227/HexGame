using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class ResourceCityPanel : Control
{
    private Control CityPanel;
    private Label CityName;
    private Label FoodLabel;
    private Label ProductionLabel;
    private Label GoldLabel;
    private Label ScienceLabel;
    private Label CultureLabel;
    private Label HappinessLabel;
    private Label InfluenceLabel;
    private HFlowContainer ResourceSlotsBox;

    private City city;
    private ResourcePanel resourcePanel;


    public ResourceCityPanel(City city, ResourcePanel resourcePanel)
    {
        this.city = city;
        this.resourcePanel = resourcePanel;
        CityPanel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourceCityPanel.tscn").Instantiate<Control>();
        AddChild(CityPanel);

        CityName = CityPanel.GetNode<Label>("MarginContainer/CityBox/CityName");
        FoodLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/FoodLabel");
        ProductionLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/ProductionLabel");
        GoldLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/GoldLabel");
        ScienceLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/ScienceLabel");
        CultureLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/CultureLabel");
        HappinessLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/HappinessLabel");
        InfluenceLabel = CityPanel.GetNode<Label>("MarginContainer/CityBox/Resources/InfluenceLabel");
        ResourceSlotsBox = CityPanel.GetNode<HFlowContainer>("MarginContainer/CityBox/ResourceSlotsBox");
        UpdateResourceCityPanel();
    }


    public void Update(UIElement element)
    {
    }

    public void UpdateResourceCityPanel()
    {
        CityName.Text = city.name;
        FoodLabel.Text = city.yields.food.ToString();
        ProductionLabel.Text = city.yields.production.ToString();
        GoldLabel.Text = city.yields.gold.ToString();
        ScienceLabel.Text = city.yields.science.ToString();
        CultureLabel.Text = city.yields.culture.ToString();
        HappinessLabel.Text = city.yields.happiness.ToString();
        InfluenceLabel.Text = city.yields.influence.ToString();

        foreach (Control child in ResourceSlotsBox.GetChildren())
        {
            child.QueueFree();
        }
        int i = 0;
        foreach (KeyValuePair<Hex, ResourceType> resource in city.heldResources)
        {
            ResourceSlotsBox.AddChild(new ResourceButton(resource.Key, resource.Value, resourcePanel));
            i++;
        }
        for (; i < city.maxResourcesHeld; i++)
        {
            Button emptyButton = new Button();
            emptyButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://.godot/imported/health.png-838c7fec5d46427d7a66a7729e2f7b98.ctex");
            emptyButton.Pressed += () => resourcePanel.AssignCurrentResource();
            ResourceSlotsBox.AddChild(emptyButton);
        }
    }
}
