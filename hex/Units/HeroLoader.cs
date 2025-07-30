using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public struct HeroInfo
{
    public UnitInfo unitInfo;
    public int mana;
    public int manaRegeneration;
    public int maxLevel;
    public List<HeroAbility> heroAbilities;
}

public static class HeroLoader
{
    public static Dictionary<string, HeroInfo> heroDict;

    static HeroLoader()
    {
        string xmlPath = "hex/Heroes.xml";
        heroDict = LoadHeroData(xmlPath);
    }

    public static Dictionary<string, HeroInfo> LoadHeroData(string xmlPath)
    {
        XDocument xmlDoc = XDocument.Load(xmlPath);
        return xmlDoc.Descendants("Hero")
            .ToDictionary(
                h => h.Attribute("Name")?.Value ?? throw new Exception("Missing Hero Name"),
                h => new HeroInfo
                {
                    unitInfo = ParseUnitInfo(h.Element("Unit")),
                    mana = int.TryParse(h.Attribute("Mana")?.Value, out var mana) ? mana : 0,
                    manaRegeneration = int.TryParse(h.Attribute("ManaRegeneration")?.Value, out var regen) ? regen : 0,
                    maxLevel = int.TryParse(h.Attribute("MaxLevel")?.Value, out var maxLevel) ? maxLevel : 1,
                    heroAbilities = h.Element("HeroAbilities")?.Elements("Ability")?.Select(ParseHeroAbility).ToList() ?? new List<HeroAbility>()
                }
            );
    }
    private static UnitInfo ParseUnitInfo(XElement r)
    {
        return new UnitInfo
        {
            Class = Enum.TryParse(r.Attribute("Class")?.Value, out UnitClass unitClass) ? unitClass : UnitClass.None,
            Faction = Enum.TryParse(r.Attribute("Faction")?.Value, out FactionType factionType) ? factionType : FactionType.All,
            ProductionCost = int.TryParse(r.Attribute("ProductionCost")?.Value, out var productionCost) ? productionCost : 0,
            GoldCost = int.TryParse(r.Attribute("GoldCost")?.Value, out var goldCost) ? goldCost : 0,
            MovementSpeed = float.TryParse(r.Attribute("MovementSpeed")?.Value, out var movementSpeed) ? movementSpeed : 0.0f,
            SightRange = float.TryParse(r.Attribute("SightRange")?.Value, out var sightRange) ? sightRange : 0.0f,
            CombatPower = float.TryParse(r.Attribute("CombatPower")?.Value, out var combatPower) ? combatPower : 0.0f,
            HealingFactor = int.TryParse(r.Attribute("HealingFactor")?.Value, out var healingFactor) ? healingFactor : 0,
            MaintenanceCost = int.TryParse(r.Attribute("MaintenanceCost")?.Value, out var maintenanceCost) ? maintenanceCost : 0,
            ZoneOfControl = bool.TryParse(r.Attribute("ZoneOfControl")?.Value, out var zoneOfControl) && zoneOfControl,
            IgnoreZoneOfControl = bool.TryParse(r.Attribute("IgnoreZoneOfControl")?.Value, out var ignoreZoc) && ignoreZoc,
            IconPath = r.Attribute("IconPath")?.Value ?? "",
            ModelPath = r.Attribute("ModelPath")?.Value ?? "",
            MovementCosts = r.Element("MovementCosts")?.Elements("TerrainMoveType").ToDictionary(
                m => Enum.Parse<TerrainMoveType>(m.Attribute("Name").Value),
                m => float.TryParse(m.Attribute("Value")?.Value, out var value) ? value : 0.0f
            ) ?? new Dictionary<TerrainMoveType, float>(),
            SightCosts = r.Element("SightCosts")?.Elements("TerrainMoveType").ToDictionary(
                s => Enum.Parse<TerrainMoveType>(s.Attribute("Name").Value),
                s => float.TryParse(s.Attribute("Value")?.Value, out var value) ? value : 0.0f
            ) ?? new Dictionary<TerrainMoveType, float>(),
            Effects = r.Element("Effects")?.Elements("Effect").Select(e => e.Value).ToList() ?? new List<string>(),
            Abilities = r.Element("Abilities")?.Elements("Ability").ToDictionary(
                a => a.Attribute("Name").Value,
                a => (
                    float.TryParse(a.Attribute("CombatPower")?.Value, out var cp) ? cp : 0,
                    int.TryParse(a.Attribute("UsageCount")?.Value, out var uc) ? uc : 1,
                    int.TryParse(a.Attribute("Range")?.Value, out var range) ? range : 0,
                    ParseTargetSpecification(a.Element("TargetSpecification")),
                    a.Attribute("IconPath")?.Value ?? ""
                )
            ) ?? new Dictionary<string, (float, int, int, TargetSpecification?, string)>()
        };
    }
    private static HeroAbility ParseHeroAbility(XElement abilityElement)
    {
        return new HeroAbility
        {
            ability = ParseUnitAbility(abilityElement.Element("UnitAbility")),
            manaCost = abilityElement.Element("ManaCost")?.Value
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var val) ? val : 0)
                        .ToArray(),
            cooldown = abilityElement.Element("Cooldown")?.Value
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s.Trim(), out var val) ? val : 0)
                        .ToArray(),
            level = int.TryParse(abilityElement.Element("Level")?.Value, out var level) ? level : 1,
            maxLevel = int.TryParse(abilityElement.Element("MaxLevel")?.Value, out var maxLevel) ? maxLevel : 1,
            minLevelToLearn = int.TryParse(abilityElement.Element("MinLevelToLearn")?.Value, out var minLevel) ? minLevel : 1
        };
    }

    private static UnitAbility ParseUnitAbility(XElement element)
    {
        if (element == null)
            throw new Exception("Missing UnitAbility element");

        return new UnitAbility
        {
            name = element.Attribute("Name")?.Value ?? "Unnamed Ability",
            combatPower = float.TryParse(element.Attribute("CombatPower")?.Value, out var cp) ? cp : 0f,
            maxChargesPerTurn = int.TryParse(element.Attribute("UsageCount")?.Value, out var uc) ? uc : 1,
            range = int.TryParse(element.Attribute("Range")?.Value, out var range) ? range : 0,
            validTargetTypes = ParseTargetSpecification(element.Element("TargetSpecification")),
            iconPath = element.Attribute("IconPath")?.Value ?? ""
        };
    }
    static TargetSpecification ParseTargetSpecification(XElement targetSpecElement)
    {
        if (targetSpecElement == null) return null;
    
        var targetSpecification = new TargetSpecification
        {
            TargetUnits = bool.TryParse(targetSpecElement.Attribute("TargetUnits")?.Value, out var targetUnits) && targetUnits,
            TargetRuralBuildings = bool.TryParse(targetSpecElement.Attribute("TargetRuralBuildings")?.Value, out var targetRuralBuildings) && targetRuralBuildings,
            TargetUrbanBuildings = bool.TryParse(targetSpecElement.Attribute("TargetUrbanBuildings")?.Value, out var targetUrbanBuildings) && targetUrbanBuildings,
            TargetTiles = bool.TryParse(targetSpecElement.Attribute("TargetTiles")?.Value, out var targetTiles) && targetTiles,
            TargetSelf = bool.TryParse(targetSpecElement.Attribute("TargetSelf")?.Value, out var targetSelf) && targetSelf,

            AllowsAnyUnit = bool.TryParse(targetSpecElement.Attribute("AllowsAnyUnit")?.Value, out var allowsAnyUnit) && allowsAnyUnit,
            AllowsAnyBuilding = bool.TryParse(targetSpecElement.Attribute("AllowsAnyBuilding")?.Value, out var allowsAnyBuilding) && allowsAnyBuilding,
            AllowsAnyTerrain = bool.TryParse(targetSpecElement.Attribute("AllowsAnyTerrain")?.Value, out var allowsAnyTerrain) && allowsAnyTerrain,
            AllowsAnyResource = bool.TryParse(targetSpecElement.Attribute("AllowsAnyResource")?.Value, out var allowsAnyResource) && allowsAnyResource,
            AllowsAnyFeature = bool.TryParse(targetSpecElement.Attribute("AllowsAnyFeature")?.Value, out var allowsAnyFeature) && allowsAnyFeature,

            RequiresAResource = bool.TryParse(targetSpecElement.Attribute("RequiresAResource")?.Value, out var requiresAResource) && requiresAResource,
            RequiresAFeature = bool.TryParse(targetSpecElement.Attribute("RequiresAFeature")?.Value, out var requiresAFeature) && requiresAFeature,

            AllowsAlly = bool.TryParse(targetSpecElement.Attribute("AllowsAlly")?.Value, out var allowsAlly) && allowsAlly,
            AllowsEnemy = bool.TryParse(targetSpecElement.Attribute("AllowsEnemy")?.Value, out var allowsEnemy) && allowsEnemy,
            AllowsNeutral = bool.TryParse(targetSpecElement.Attribute("AllowsNeutral")?.Value, out var allowsNeutral) && allowsNeutral,
            RequiresAncientRuins = bool.TryParse(targetSpecElement.Attribute("RequiresAncientRuins")?.Value, out var requiresAncientRuins) && requiresAncientRuins,
        };
    
        targetSpecification.ValidUnitTypes = targetSpecElement.Element("ValidUnitTypes")?.Elements("UnitType")
            .Select(b => b.Attribute("Name")?.Value ?? throw new Exception("Invalid String"))
            .ToHashSet() ?? new HashSet<String>();

        targetSpecification.AllowedUnitClasses = targetSpecElement.Element("AllowedUnitClasses")?.Value
            .Split(", ", StringSplitOptions.RemoveEmptyEntries)
            .Aggregate(UnitClass.None, (current, className) =>
                Enum.TryParse<UnitClass>(className, out var unitClass) ? current | unitClass : throw new Exception("Invalid UnitClass"))
            ?? UnitClass.None;

        targetSpecification.ValidBuildingTypes = targetSpecElement.Element("ValidBuildingTypes")?.Elements("BuildingType")
            .Select(b => b.Attribute("Name")?.Value ?? throw new Exception("Invalid String"))
            .ToHashSet() ?? new HashSet<String>();

        targetSpecification.ValidTerrainTypes = targetSpecElement.Element("ValidTerrainTypes")?.Elements("TerrainType")
            .Select(t => Enum.TryParse<TerrainType>(t.Attribute("Name")?.Value, out var terrainType) ? terrainType : throw new Exception("Invalid TerrainType :)" + targetSpecElement.Value + "GUH"))
            .ToHashSet() ?? new HashSet<TerrainType>();

        targetSpecification.ValidResourceTypes = targetSpecElement.Element("ValidResourceTypes")?.Elements("ResourceType")
            .Select(t => Enum.TryParse<ResourceType>(t.Attribute("Name")?.Value, out var resourceType) ? resourceType : throw new Exception("Invalid ResourceType"))
            .ToHashSet() ?? new HashSet<ResourceType>();

        targetSpecification.ValidFeatureTypes = targetSpecElement.Element("ValidFeatureTypes")?.Elements("FeatureType")
            .Select(t => Enum.TryParse<FeatureType>(t.Attribute("Name")?.Value, out var featureType) ? featureType : throw new Exception("Invalid FeatureType"))
            .ToHashSet() ?? new HashSet<FeatureType>();

        return targetSpecification;
    }
}
