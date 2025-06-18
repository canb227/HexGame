using Godot;
using NetworkMessages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class ResearchTreePanel : Control
{
    Control researchTree;
    HBoxContainer researchHBox;
    List<VBoxContainer> researchTiers;
    Dictionary<int, List<string>> researchTierDict;
    PackedScene researchButtonScene;
    public PackedScene researchEffectScene;
    Button closeButton;
    List<Button> buttonList = new();
    bool isCultureTree;

    Dictionary<string, ResearchInfo> researchesDict;



    Control[][] tierSlotButtonArray;
    int mostResearchesInATier;
    int numberOfTiers;
    List<Line2D> lineList = new();
    public ResearchTreePanel(Dictionary<string, ResearchInfo> researchesDict, bool isCultureTree)
    {
        this.isCultureTree = isCultureTree;
        this.researchesDict = researchesDict;
        researchTierDict = new();
        researchButtonScene = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResearchButton.tscn");
        researchEffectScene = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResearchEffectIcon.tscn");
        researchTree = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResearchTreePanel.tscn").Instantiate<Control>();
        researchHBox = researchTree.GetNode<HBoxContainer>("ResearchPanel/ResearchScroll/ResearchMargin/ResearchHBox");
        closeButton = researchTree.GetNode<Button>("CloseButton");
        closeButton.Pressed += () => Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();

        int highestTier = 0;
        foreach (KeyValuePair<string, ResearchInfo> info in researchesDict)
        {
            if(info.Value.Tier > highestTier)
            {
                highestTier = info.Value.Tier;
            }
        }
        for (int i = 0; i <= highestTier; i++)
        {
            researchTierDict[i] = new List<string>();
        }
        foreach (KeyValuePair<string, ResearchInfo> info in researchesDict)
        {
            researchTierDict[info.Value.Tier].Add(info.Key);
        }

        mostResearchesInATier = 0;
        numberOfTiers = researchTierDict.Keys.Count;
        foreach (int key in researchTierDict.Keys.OrderByDescending(k => k))
        {
            if (researchTierDict[key].Count > mostResearchesInATier)
            {
                mostResearchesInATier = researchTierDict[key].Count;
            }

            foreach (string researchName in researchTierDict[key])
            {
                if (!researchName.Contains("BLANK"))
                {
                    ResearchInfo info = researchesDict[researchName];
                    foreach (string requirement in info.Requirements)
                    {
                        ResearchInfo requirementInfo = researchesDict[requirement];
                        int tierDelta = info.Tier - requirementInfo.Tier;
                        while (tierDelta > 1)
                        {
                            tierDelta--;
                            researchTierDict[info.Tier - tierDelta].Add(researchName + "|" + requirement + "\\" + "BLANK" + (info.Tier - tierDelta));
                        }
                    }
                }
            }
        }

        //fill each tier with blanks to hit max size
        int index = 0;
        foreach (int key in researchTierDict.Keys.OrderByDescending(k => k))
        {
            while (researchTierDict[key].Count < mostResearchesInATier)
            {
                researchTierDict[key].Add("BLANK"+index);
                index++;
            }
        }

        tierSlotButtonArray = new Control[numberOfTiers][];
        for (int i = 0; i < numberOfTiers; i++)
        {
            tierSlotButtonArray[i] = new Control[mostResearchesInATier];
        }

        PlaceResearches();
        AddChild(researchTree);
    }

    public void ResearchButtonPressed(string researchName)
    {
        if(isCultureTree)
        {
            Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].SelectCultureResearch(researchName);
        }
        else
        {
            Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].SelectResearch(researchName);
        }
        Global.gameManager.graphicManager.Update2DUI(UIElement.researchTree);
    }

    public void UpdateResearchUI()
    {
        List<ResearchQueueType> queuedResearch = new();
        HashSet<String> completedResearches = new();
        if(isCultureTree)
        {
            queuedResearch = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch;
            completedResearches = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].completedCultureResearches;
        }
        else
        {
            queuedResearch = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch;
            completedResearches = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].completedResearches;
        }
        foreach(Button button in buttonList)
        {
            int index = 0;
            bool inProgress = false;
            for (int i = 0; i < queuedResearch.Count; i++)
            {
                ResearchQueueType researchItem = queuedResearch[i];
                if(researchItem.researchType == button.Name)
                {
                    inProgress = true;
                    index = i + 1;
                    break;
                }
            }

            bool canResearch = true;
            ResearchInfo info = researchesDict[button.Name];
            foreach(string requirement in info.Requirements)
            {
                if(!completedResearches.Contains(requirement))
                {
                    canResearch = false;
                    break;
                }
            }


            if (completedResearches.Contains(button.Name))
            {
                button.Disabled = true;
                button.AddThemeColorOverride("icon_disabled_color", Godot.Colors.Goldenrod);
                button.GetNode<TextureRect>("ResearchQueue").Visible = false;
            }
            else if (inProgress)
            {
                button.Disabled = false;
                button.AddThemeColorOverride("icon_normal_color", new Godot.Color(0.690196f, 0.768627f, 0.870588f));
                button.AddThemeColorOverride("icon_hover_color", new Godot.Color(0.7592156f, 0.8454897f, 0.9576468f));
                button.GetNode<TextureRect>("ResearchQueue").Visible = true;
                button.GetNode<Label>("ResearchQueue/ResearchQueueValue").Text = index.ToString();
            }
            else if (canResearch)
            {
                button.Disabled = false;
                button.AddThemeColorOverride("icon_normal_color", new Godot.Color(0.745098f, 0.745098f, 0.745098f));
                button.AddThemeColorOverride("icon_hover_color", new Godot.Color(0.815098f, 0.815098f, 0.815098f));
                button.GetNode<TextureRect>("ResearchQueue").Visible = false;
            }
            else
            {
                button.AddThemeColorOverride("icon_normal_color", new Godot.Color(0.411765f, 0.411765f, 0.411765f));
                button.AddThemeColorOverride("icon_hover_color", new Godot.Color(0.471765f, 0.471765f, 0.471765f));
                button.GetNode<TextureRect>("ResearchQueue").Visible = false;
            }
        }
    }

    public void PlaceResearches()
    {
        researchTiers = new();
        //here is our shuffle then build
        foreach (List<string> list in researchTierDict.Values)
        {
            Shuffle(list);
        }

        for (int i = numberOfTiers - 1; i >= 0; i--)
        {
            for (int j = 0; j < researchTierDict[i].Count; j++)
            {
                string research = researchTierDict[i][j];
                //researchTierDict[i].Remove(research);
                if (research.Contains("BLANK"))
                {
                    Control researchBlank = new Control();
                    researchBlank.CustomMinimumSize = new Vector2(256.0f, 64.0f);
                    researchBlank.Name = research;
                    tierSlotButtonArray[i][j] = researchBlank;
                }
                else
                {
                    Button researchButton = ConfigureResearchButton(research);
                    researchButton.Pressed += () => ResearchButtonPressed(research);
                    tierSlotButtonArray[i][j] = researchButton;
                    buttonList.Add(researchButton);
                }

            }
        }



        for (int i = numberOfTiers - 1; i >= 0; i--)
        {
            VBoxContainer researchTier = new VBoxContainer();
            researchTier.Alignment = BoxContainer.AlignmentMode.Center;
            for (int j = 0; j < researchTierDict[i].Count; j++)
            {
                researchTier.AddChild(tierSlotButtonArray[i][j]);
            }
            researchTiers.Add(researchTier);
        }

        for (int i = researchTiers.Count - 1; i >= 0; i--)
        {
            researchHBox.AddChild(researchTiers[i]);
            Control temp = new Control();
            temp.CustomMinimumSize = new Vector2(128.0f, 0.0f);
            researchHBox.AddChild(temp);
        }
    }

    public Button ConfigureResearchButton(string research)
    {
        ResearchInfo researchInfo = researchesDict[research];
        Button researchButton = researchButtonScene.Instantiate<Button>();
        RichTextLabel rt = researchButton.GetNode<RichTextLabel>("ResearchName");
        TextureRect researchIcon = researchButton.GetNode<TextureRect>("ResearchIcon");
        rt.Text = research;
        researchButton.Name = research;
        researchIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + researchInfo.IconPath);

        HBoxContainer researchEffects = researchButton.GetNode<HBoxContainer>("ResearchResultBox");
        foreach(String unitName in researchInfo.UnitUnlocks)
        {
            TextureRect unitIcon = researchEffectScene.Instantiate<TextureRect>();
            unitIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + UnitLoader.unitsDict[unitName].IconPath);
            researchEffects.AddChild(unitIcon);
        }
        foreach (String buildingName in researchInfo.BuildingUnlocks)
        {
            TextureRect buildingIcon = researchEffectScene.Instantiate<TextureRect>();
            buildingIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + BuildingLoader.buildingsDict[buildingName].IconPath);
            researchEffects.AddChild(buildingIcon);
        }

        return researchButton;
    }

    public void AddLines()
    {
        lineList = new();
        for (int i = numberOfTiers - 1; i > 0; i--)
        {
            for (int j = 0; j < mostResearchesInATier; j++)
            {
                String researchName = tierSlotButtonArray[i][j].Name;
                //ignore truely blank slots
                if (!researchName.StartsWith("BLANK"))
                {
                    //for a given research find all buttons in the next tier and check all of them for one called "(CURRENTRESEARCHNAME)-"
                    if (researchName.Contains("\\BLANK"))
                    {
                        //if we are a blank placeholder find our next jump using the word after the | and before \
                        string target = researchName.Split(new char[] { '|', '\\' })[1];
                        string source = researchName.Split(new char[] { '|', '\\' })[0];
                        for (int x = 0; x < mostResearchesInATier; x++)
                        {
                            if (tierSlotButtonArray[i - 1][x].Name == target)
                            {
                                AddLine(researchName, i, j, x, true, source);
                            }
                            else if (((string)tierSlotButtonArray[i - 1][x].Name).Contains("|"))
                            {
                                string slotButtonTarget = ((string)tierSlotButtonArray[i - 1][x].Name).Split(new char[] { '|', '\\' })[1];
                                string slotButtonSource = ((string)tierSlotButtonArray[i - 1][x].Name).Split(new char[] { '|', '\\' })[0];
                                if(slotButtonTarget == target && slotButtonSource == source)
                                {
                                    AddLine(researchName, i, j, x, true, source);
                                }
                            }
                        }
                    }
                    else
                    {
                        ResearchInfo info = researchesDict[researchName];
                        foreach (string requirement in info.Requirements)
                        {
                            for (int x = 0; x < mostResearchesInATier; x++)
                            {
                                if (tierSlotButtonArray[i - 1][x].Name == requirement || ((String)tierSlotButtonArray[i - 1][x].Name).Split(new char[] { '|', '\\' })[0] == researchName)
                                {
                                    AddLine(researchName, i, j, x, false, researchName);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void AddLine(String researchName, int i, int j, int x, bool isBlank, String sourceResearch)
    {
        Line2D line = new Line2D();
        if (isBlank)
        {
            Vector2 blankStartPos = new Vector2(researchTiers[numberOfTiers - i - 1].Position.X + +tierSlotButtonArray[numberOfTiers - i - 1][j].Size.X, tierSlotButtonArray[i][j].Position.Y + (tierSlotButtonArray[i][j].Size.Y / 2));
            Vector2 blankEndPos = new Vector2(researchTiers[numberOfTiers - i - 1].Position.X, tierSlotButtonArray[i][j].Position.Y + (tierSlotButtonArray[i][j].Size.Y / 2));
            line.AddPoint(blankStartPos);
            line.AddPoint(blankEndPos);
        }
        float leftSide = researchTiers[numberOfTiers - i - 1].Position.X;
        float middlePos = tierSlotButtonArray[i][j].Position.Y + (tierSlotButtonArray[i][j].Size.Y / 2);
        Vector2 startPos = new Vector2(leftSide, middlePos);
        Vector2 startPos2 = new Vector2(leftSide-16.0f, middlePos);

        float rightSide = researchTiers[numberOfTiers - i].Position.X + tierSlotButtonArray[i - 1][j].Size.X;
        float middlePos2 = tierSlotButtonArray[i - 1][x].Position.Y + (tierSlotButtonArray[i - 1][x].Size.Y / 2);
        Vector2 endPos2 = new Vector2(rightSide + 16.0f, middlePos2);
        Vector2 endPos = new Vector2(rightSide, middlePos2);

        line.AddPoint(startPos);
        line.AddPoint(startPos2);
        line.AddPoint(endPos2);
        line.AddPoint(endPos);
        Random rand = new Random(sourceResearch.Sum(c => (int)c));
        line.DefaultColor = new Godot.Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
        line.EndCapMode = Line2D.LineCapMode.Round;
        line.BeginCapMode = Line2D.LineCapMode.Round;
        lineList.Add(line);
        researchHBox.AddChild(line);
        
    }

    //for testing the tree with different ordering TODO remove for any actually game usage
    private Random rng = new Random();
    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}