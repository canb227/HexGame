using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;
public enum TerrainMoveType
{
	Ocean,
	Coast,
	Flat,
	Rough,
	Mountain,
	Forest,
	River,
	Road,
	Coral,
	Embark,
	Disembark
}

[Serializable]
public partial class Unit
{
	public string Type => GetType().Name; // "Unit" or "Hero"
	public String name { get; set; }
	public int id { get; set; }
	public String unitType { get; set; }
	public Dictionary<TerrainMoveType, float> movementCosts { get; set; } = new();
	public Dictionary<TerrainMoveType, float> sightCosts { get; set; } = new();
	public Hex hex { get; set; }
	public float movementSpeed { get; set; }
	public float remainingMovement { get; set; }
	public float sightRange { get; set; } = 3.0f;
	public float health { get; set; } = 100.0f;
	public float combatStrength { get; set; } = 10.0f;
	public float baseCombatStrength { get; set; }
	public float maintenanceCost { get; set; } = 1.0f;
	public bool baseZoneOfControl { get; set; }
	public bool zoneOfControl { get; set; }
	public bool baseIgnoreZoneOfControl { get; set; }
	public bool ignoreZoneOfControl { get; set; }
	public float baseMaintenanceCost { get; set; }
	public int maxAttackCount { get; set; } = 1;
	public int attacksLeft { get; set; } = 1;
	public int healingFactor { get; set; }
	//healingOverTime refers to healing applied by abilities and isnt related to afk passive healing
	public int healingOverTime { get; set; }
	public int teamNum { get; set; }
	public UnitClass unitClass { get; set; }
	public List<Hex>? currentPath { get; set; } = new();
	public List<Hex> visibleHexes { get; set; } = new();
	public List<UnitEffect> effects { get; set; } = new();
	public Dictionary<string, int> abilities { get; set; } = new();
	public Dictionary<string, UnitEffect> onKillEffects { get; set; } = new();
	public bool isTargetEnemy { get; set; }
	public bool isSleeping { get; set; }
	public bool isSkipping { get; set; }
	public bool spawnSetupFinished { get; set; }
	public bool fortifying { get; set; }
	public int fortifyStrength { get; set; }
	public string IconPath { get; set; }
	public int bleedDuration { get; set; }
	public int bleedStrength { get; set; }

	public int FailedConsecutiveMovements { get; set; } = 0;
	public Unit(String unitType, int combatModifier, int id, int teamNum)
	{
		this.id = id;
		this.name = unitType;
		this.teamNum = teamNum;
		this.unitType = unitType;
		if (UnitLoader.unitsDict.TryGetValue(unitType, out UnitInfo unitInfo))
		{
			Global.gameManager.game.unitDictionary.TryAdd(id, this);
			this.unitClass = unitInfo.Class;
			this.movementCosts = unitInfo.MovementCosts;
			this.sightCosts = unitInfo.SightCosts;
			this.movementSpeed = unitInfo.MovementSpeed;
			this.remainingMovement = unitInfo.MovementSpeed;
			this.sightRange = unitInfo.SightRange;
			this.healingFactor = unitInfo.HealingFactor;
			this.combatStrength = unitInfo.CombatPower + combatModifier;
			this.baseCombatStrength = unitInfo.CombatPower + combatModifier;
			this.maintenanceCost = unitInfo.MaintenanceCost;
			this.baseMaintenanceCost = unitInfo.MaintenanceCost;
			this.baseZoneOfControl = unitInfo.ZoneOfControl;
			this.zoneOfControl = unitInfo.ZoneOfControl;
			this.baseIgnoreZoneOfControl = unitInfo.IgnoreZoneOfControl;
			this.ignoreZoneOfControl = unitInfo.IgnoreZoneOfControl;
			this.IconPath = unitInfo.IconPath;

			foreach (String effectName in unitInfo.Effects)
			{
				AddEffect(new UnitEffect(effectName));
			}

			foreach (String abilityName in unitInfo.Abilities.Keys)
			{
				AddAbility(abilityName, unitInfo);
			}
			//generic abilities
			AddGenericAbility("Sleep", "graphics/ui/icons/sleep.png");
			AddGenericAbility("Skip", "graphics/ui/icons/skipturn.png");
			RecalculateEffects();
		}
		else
		{
			throw new ArgumentException($"Unit type '{name}' not found in unit data.");
		}
	}

	public Unit()
	{

	}


	public virtual void SpawnSetup(GameHex targetGameHex, bool isRespawn = false)
	{
		spawnSetupFinished = true;
		targetGameHex.units.Add(id);
		hex = targetGameHex.hex;
		Global.gameManager.game.playerDictionary[teamNum].unitList.Add(this.id);
		RecalculateEffects();
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("NewUnit", id);
	}



	public virtual void OnTurnStarted(int turnNumber)
	{
		//bleeding prevent passive regen
		if (remainingMovement >= movementSpeed && attacksLeft >= maxAttackCount && bleedDuration <= 0)
		{
			if(Global.gameManager.game.mainGameBoard.gameHexDict[hex].ownedBy == teamNum)
			{
				increaseHealth(healingFactor + 5);
			}
			else if(Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(Global.gameManager.game.mainGameBoard.gameHexDict[hex].ownedBy))
			{
				increaseHealth(healingFactor - 5);
			}
			else
			{
				increaseHealth(healingFactor);
			}
		}
		if (fortifying && fortifyStrength < 6)
		{
			fortifyStrength += 3;
			if (fortifyStrength > 6)
			{
				fortifyStrength = 6;
			}
		}
		//take DOT damage
		if(bleedDuration > 0)
		{
			bleedDuration--;
			decreaseHealth(bleedStrength);
		}
		foreach (string abilityName in abilities.Keys)
		{
			UnitAbility ability = Global.gameManager.game.unitAbilities[abilityName];
			if (ability.maxChargesPerTurn > -1)
			{
				abilities[abilityName] = ability.maxChargesPerTurn;
			}
		}
		isSkipping = false;
		SetRemainingMovement(movementSpeed);
		SetAttacksLeft(maxAttackCount);
	}

	public void OnTurnEnded(int turnNumber)
	{
		((Player)Global.gameManager.game.playerDictionary[teamNum]).AddGold(-maintenanceCost);
		if(remainingMovement > 0.0f && currentPath.Any() && !isTargetEnemy)
		{
			MoveTowards(Global.gameManager.game.mainGameBoard.gameHexDict[currentPath.Last()], Global.gameManager.game.teamManager, false);
		}
	}

	public void SetAttacksLeft(int attacksLeft)
	{
		this.attacksLeft = attacksLeft;
		foreach (string abilityName in abilities.Keys)
		{
			UnitAbility ability = Global.gameManager.game.unitAbilities[abilityName];
			if (ability.name.EndsWith("Attack"))
			{
				abilities[abilityName] = attacksLeft;
			}
		}
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
		{
			manager.CallDeferred("Update2DUI", (int)UIElement.unitDisplay);
			manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
		}
	}

	public void SetRemainingMovement(float remainingMovement)
	{
		this.remainingMovement = remainingMovement;
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
		{
			manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
			manager.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
		}
	}

	public void RecalculateEffects()
	{

		//must reset all to base and recalculate
		if (UnitLoader.unitsDict.TryGetValue(unitType, out UnitInfo unitInfo))
		{
			movementCosts = unitInfo.MovementCosts;
			sightCosts = unitInfo.SightCosts;
			movementSpeed = unitInfo.MovementSpeed;
			sightRange = unitInfo.SightRange;
			combatStrength = baseCombatStrength;
			maintenanceCost = baseMaintenanceCost;
		}
		//also order all effects, multiply/divide after add/subtract priority
		//0 means it is applied first 100 means it is applied "last" (highest number last)
		//so multiply/divide effects should be 20 and add/subtract will be 10 to give wiggle room
		PriorityQueue<UnitEffect, int> orderedEffects = new();
		foreach (UnitPlayerEffect effectTuple in Global.gameManager.game.playerDictionary[teamNum].unitPlayerEffects)
		{
			if (unitClass.HasFlag(effectTuple.effectedClass))
			{
				orderedEffects.Enqueue(effectTuple.effect, effectTuple.effect.priority);
			}
		}
		foreach (UnitEffect effect1 in effects)
		{
			orderedEffects.Enqueue(effect1, effect1.priority);
		}
		UnitEffect effect;
		int priority;
		while(orderedEffects.TryDequeue(out effect, out priority))
		{
			effect.Apply(id);
		}
		//difficulty mod
		combatStrength += Global.gameManager.game.playerDictionary[teamNum].combatPowerDifficultyModifier;

		//fortification mod
		combatStrength += fortifyStrength;
		if (spawnSetupFinished)
		{
			UpdateVision();
		}
	}

	public void AddEffect(UnitEffect effect)
	{
		effects.Add(effect);
		RecalculateEffects();
	}

	public void RemoveEffect(UnitEffect effect)
	{
		effects.Remove(effect);
		RecalculateEffects();
	}

	public void AddAbility(string abilityName, UnitInfo unitInfo)
	{
		if(Global.gameManager.game.unitAbilities.ContainsKey(abilityName))
		{
			abilities.Add(abilityName, Global.gameManager.game.unitAbilities[abilityName].maxChargesPerTurn);
		}
		else
		{
			Global.gameManager.game.unitAbilities.Add(abilityName, new UnitAbility(abilityName, unitInfo.Abilities[abilityName].Item1, unitInfo.Abilities[abilityName].Item2, unitInfo.Abilities[abilityName].Item3, unitInfo.Abilities[abilityName].Item4, unitInfo.Abilities[abilityName].Item5, unitInfo.Abilities[abilityName].Item6, unitInfo.Abilities[abilityName].Item7));
			abilities.Add(abilityName, Global.gameManager.game.unitAbilities[abilityName].maxChargesPerTurn);
		}
	}

	public void AddGenericAbility(string abilityName, string abilityIconPath)
	{
		TargetSpecification validTargetTypes = new TargetSpecification();
		validTargetTypes.TargetSelf = true; validTargetTypes.AllowsAnyUnit = true; validTargetTypes.AllowsAnyBuilding = true;
		validTargetTypes.AllowsAnyTerrain = true; validTargetTypes.AllowsAnyResource = true; validTargetTypes.AllowsAlly = true;
		validTargetTypes.AllowsAnyFeature = true;
		if (Global.gameManager.game.unitAbilities.ContainsKey(abilityName))
		{
			abilities.Add(abilityName, Global.gameManager.game.unitAbilities[abilityName].maxChargesPerTurn);
		}
		else
		{
			Global.gameManager.game.unitAbilities.Add(abilityName, new UnitAbility(abilityName, validTargetTypes: validTargetTypes, iconPath: abilityIconPath)  );
			abilities.Add(abilityName, Global.gameManager.game.unitAbilities[abilityName].maxChargesPerTurn);
		}
	}

	//public void UseAbilities()
	//{
	//    for (int i = 0; i < abilities.Count; i++)
	//    {
	//        var ability = abilities[i];
	//        if (ability.currentCharges >= 1)
	//        {
	//            ability.GetUnitEffect().Apply(id);
	//            ability.currentCharges -= 1; // Update the tuple directly
	//            abilities[i] = ability; // Write the modified tuple back to the list
	//        }
	//    }
	//}

	public bool CanSettleHere(Hex hex, int range, List<TerrainType> allowedTerrainTypes, bool logging = false)
	{
		if (logging) { Global.Log("[CanSettleHere Check] Checking: " + hex); }
		if (logging) { Global.Log("    Iterating over all hexes within the given range: " + range); }

		if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isCityCenter)
		{
			if (logging) { Global.Log("    Hex contains a district. We cannot settle here."); }
			return false;
		}

		if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].withinCityRange > 0)
		{
			if (logging) { Global.Log("    Hex is labeled as within City Range. We cannot settle here."); }
			return false;
		}

		if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestCity <= range && Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestCity > 0)
		{
			if (logging) { Global.Log("    Hex is in range of a city. We cannot settle here."); }
			return false;
		}
		
		//allow settling on resources now
/*        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
		{
			if (logging) { Global.Log("    Target hex itself contains a resource. We cannot settle here."); }
			return false;
		}*/
		if (!allowedTerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType))
		{
			if (logging) { Global.Log("    Target hex itself Is not an allowed terrain type. We cannot settle here."); }
			return false;
		}
		if (logging) { Global.Log("    Check passed, we can settle here."); }
		return true;
	}

	//civ 6 formula
	public float CalculateDamage(float friendlyCombatStrength, float enemyCombatStrength, float randomFactor)
	{
		float strengthDifference = (enemyCombatStrength - friendlyCombatStrength) / 25;
		float x = strengthDifference * randomFactor;

		return 30 * (float)Math.Exp(x);
	}

	private bool DistrictCombat(GameHex targetGameHex, int bonusCombatPower=0)
	{
		//we use our hex q,r and turn number to generate a random seed that is the same on all machines
		float randomFactor = (float)new Random(hex.q + hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble() * 0.4f + 0.8f; 
		if(this is Hero hero)
		{
			hero.IncreaseExperience((int)Math.Round(CalculateDamage(combatStrength+bonusCombatPower, targetGameHex.district.GetCombatStrength(), randomFactor)));
		}
		return !decreaseHealth(CalculateDamage(combatStrength+bonusCombatPower, targetGameHex.district.GetCombatStrength(), randomFactor)) & targetGameHex.district.decreaseHealth(CalculateDamage(targetGameHex.district.GetCombatStrength(), combatStrength, randomFactor));
	}

	public float CalculateCombatStrength(float bonusCombatPower, Unit targetUnit, int targetTeamNum)
	{
		float calculatedCombatStrength = combatStrength + bonusCombatPower;
		if(targetUnit != null)
		{
			//anti-cavalry check
			if ((unitClass & UnitClass.AntiCavalry) != 0 && (targetUnit.unitClass & UnitClass.Cavalry) != 0)
			{
				calculatedCombatStrength += 7;
			}
		}

		//anti-encampment bonus check
		if (Global.gameManager.game.playerDictionary[targetTeamNum].isEncampment)
		{
			calculatedCombatStrength += Global.gameManager.game.playerDictionary[teamNum].bonusAgainstEncampments;
		}
		return calculatedCombatStrength;
	}

	private bool UnitCombat(GameHex targetGameHex, Unit unit, int bonusCombatPower=0)
	{
		float modCombatStrength = CalculateCombatStrength(bonusCombatPower, unit, unit.teamNum);
		float unitModCombatStrength = unit.CalculateCombatStrength(0, this, teamNum);

		//we use our hex q,r and turn number to generate a random seed that is the same on all machines
		float randomFactor = (float)new Random(hex.q + hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble() * 0.4f + 0.8f;
		if (this is Hero hero)
		{
			hero.IncreaseExperience((int)Math.Round(CalculateDamage(modCombatStrength, unitModCombatStrength, randomFactor)));
		}
		if(unit is Hero targetHero)
		{
			targetHero.IncreaseExperience((int)Math.Round(CalculateDamage(unitModCombatStrength, modCombatStrength, randomFactor)));
		}
		bool targetDiedWeLived = !decreaseHealth(CalculateDamage(modCombatStrength, unitModCombatStrength, randomFactor)) & unit.decreaseHealth(CalculateDamage(unitModCombatStrength, modCombatStrength, randomFactor));
		if(targetDiedWeLived)
		{
			//trigger on kill passives?
			foreach (UnitEffect unitEffect in unit.onKillEffects.Values)
			{
				unitEffect.Apply(this.id, 0, 0, Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex]);
			}
		}
		return targetDiedWeLived;
	}

	public bool AttackTarget(GameHex targetGameHex, float moveCost, TeamManager teamManager, int bonusCombatPower = 0)
	{
		fortifying = false;
		fortifyStrength = 0;
		Global.gameManager.graphicManager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
		RecalculateEffects();

		isSleeping = false;
		isSkipping = false;
		SetRemainingMovement(remainingMovement - moveCost); 
		if (targetGameHex.district != null && teamManager.GetEnemies(teamNum).Contains(Global.gameManager.game.cityDictionary[targetGameHex.district.cityID].teamNum) && targetGameHex.district.health > 0.0f)
		{
			SetAttacksLeft(attacksLeft - 1);
			return DistrictCombat(targetGameHex, bonusCombatPower);;
		}
		if (targetGameHex.units.Any())
		{
			Unit unit = Global.gameManager.game.unitDictionary[targetGameHex.units[0]];
			if (teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
			{
				//combat math TODO
				//if we didn't die and the enemy has died we can move in otherwise atleast one of us should poof
				SetAttacksLeft(attacksLeft - 1);
				return UnitCombat(targetGameHex, unit, bonusCombatPower);
			}
			return false;
		}
		else
		{
			return true;
		}
	}

	private bool RangedDistrictCombat(GameHex targetGameHex, float rangedPower)
	{
		//we use our hex q,r and turn number to generate a random seed that is the same on all machines
		float randomFactor = (float)new Random(hex.q + hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble() * 0.4f + 0.8f;
		if (this is Hero hero)
		{
			hero.IncreaseExperience((int)Math.Round(CalculateDamage(rangedPower, targetGameHex.district.GetCombatStrength(), randomFactor)));
		}
		return targetGameHex.district.decreaseHealth(CalculateDamage(rangedPower, targetGameHex.district.GetCombatStrength(), randomFactor));
	}

	private bool RangedUnitCombat(GameHex targetGameHex, Unit unit, float rangedPower)
	{
		//we use our hex q,r and turn number to generate a random seed that is the same on all machines
		float randomFactor = (float)new Random(hex.q + hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble() * 0.4f + 0.8f;
		if (this is Hero hero)
		{
			hero.IncreaseExperience((int)Math.Round(CalculateDamage(rangedPower, unit.combatStrength, randomFactor)));
		}
		bool targetDiedWeLived = !unit.decreaseHealth(CalculateDamage(rangedPower, unit.combatStrength, randomFactor));
		if (targetDiedWeLived)
		{
			//trigger on kill passives
			foreach (UnitEffect unitEffect in unit.onKillEffects.Values)
			{
				unitEffect.Apply(this.id, 0, 0, Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex]);
			}
		}
		return targetDiedWeLived;
	}

	public bool RangedAttackTarget(GameHex targetGameHex, float rangedPower, TeamManager teamManager)
	{
		fortifying = false;
		fortifyStrength = 0;
		Global.gameManager.graphicManager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
		RecalculateEffects();
		isSleeping = false;
		isSkipping = false;
		//remainingMovement -= moveCost;
		if (targetGameHex.district != null && teamManager.GetEnemies(teamNum).Contains(Global.gameManager.game.cityDictionary[targetGameHex.district.cityID].teamNum) && targetGameHex.district.health > 0.0f)
		{
			SetAttacksLeft(attacksLeft - 1);
			return RangedDistrictCombat(targetGameHex, rangedPower);
		}
		if (targetGameHex.units.Any())
		{
			
			Unit unit = Global.gameManager.game.unitDictionary[targetGameHex.units[0]];
			if (teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
			{
				//combat math TODO
				//if we didn't die and the enemy has died we can move in otherwise atleast one of us should poof
				SetAttacksLeft(attacksLeft - 1);
				return RangedUnitCombat(targetGameHex, unit, rangedPower);
			}
			return false;
		}
		else
		{
			return false;
		}
	}

	public bool BombardAttackTarget(GameHex targetGameHex, float rangedPower, TeamManager teamManager)
	{
		fortifying = false;
		fortifyStrength = 0;
		Global.gameManager.graphicManager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
		RecalculateEffects();
		isSleeping = false;
		isSkipping = false;
		//remainingMovement -= moveCost;
		if (targetGameHex.district != null && teamManager.GetEnemies(teamNum).Contains(Global.gameManager.game.cityDictionary[targetGameHex.district.cityID].teamNum) && targetGameHex.district.health > 0.0f)
		{
			SetAttacksLeft(attacksLeft - 1);
			return RangedDistrictCombat(targetGameHex, rangedPower+10); //bombard gets +10 strength against districts
		}
		if (targetGameHex.units.Any())
		{
			Unit unit = Global.gameManager.game.unitDictionary[targetGameHex.units[0]];
			if (teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
			{
				//combat math TODO
				//if we didn't die and the enemy has died we can move in otherwise atleast one of us should poof
				SetAttacksLeft(attacksLeft - 1);
				return RangedUnitCombat(targetGameHex, unit, rangedPower);
			}
			return false;
		}
		else
		{
			return false;
		}
	}

	public void increaseHealth(float amount)
	{
		health += amount;
		health = Math.Min(health, 100.0f);
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
		{
			manager.CallDeferred("Update2DUI", (int)UIElement.unitDisplay);
			manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
		}
	}

	public bool decreaseHealth(float amount)
	{
		isSleeping = false;
		isSkipping = false;
		health -= amount;
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
		{
			manager.CallDeferred("Update2DUI", (int)UIElement.unitDisplay);
		}
		if (health <= 0.0f)
		{
			onDeathEffects();
			return true;
		}
		else
		{
			if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
			return false;
		}
	}

	public virtual void onDeathEffects()
	{
		if (unitType == "Settler")
		{
			((Player)Global.gameManager.game.playerDictionary[teamNum]).DecreaseAllSettlerCost();
		}
		Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(this.id);
		Global.gameManager.game.playerDictionary[teamNum].unitList.Remove(this.id);
		RemoveVision(true);
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
		{
			manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Remove);
			Global.gameManager.graphicManager.uiManager.CallDeferred("Update", (int)UIElement.endTurnButton);
		}
	}

	public void UpdateVision()
	{
		RemoveVision(false);
		AddVision(true);
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
	}

	public void RemoveVision(bool updateGraphic)
	{
		foreach (int teamNum in Global.gameManager.game.teamManager.GetAllies(teamNum))
		{
			foreach (Hex hex in visibleHexes)
			{
				int count;
				if (Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.TryGetValue(hex, out count))
				{
					if (count <= 1)
					{
						Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.Remove(hex);
						Global.gameManager.game.playerDictionary[teamNum].visibilityChangedList.Add(hex);
					}
					else
					{
						Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict[hex] = count - 1;
					}
				}
			}
		}
		foreach (Hex hex in visibleHexes)
		{
			int count;
			if (Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict.TryGetValue(hex, out count))
			{
				if (count <= 1)
				{
					Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict.Remove(hex);
				}
				else
				{
					Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict[hex] = count - 1;
				}
			}
		}
		visibleHexes.Clear();
		if (updateGraphic &&Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
	}

	public void AddVision(bool updateGraphic)
	{
		visibleHexes = CalculateVision().Keys.ToList();

		foreach (int teamNum in Global.gameManager.game.teamManager.GetAllies(teamNum))
		{
			foreach (Hex hex in visibleHexes)
			{
				Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict.TryAdd(hex, true); //add to the seen dict no matter what since duplicates are thrown out
				int count;
				if (Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.TryGetValue(hex, out count))
				{
					Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict[hex] = count + 1;
				}
				else
				{
					Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.TryAdd(hex, 1);
					Global.gameManager.game.playerDictionary[teamNum].visibilityChangedList.Add(hex);
				}
			}
		}
		//once more to add to our team's personal list used for removing alliances
		foreach (Hex hex in visibleHexes)
		{
			int count;
			if (Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict.TryGetValue(hex, out count))
			{
				Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict[hex] = count + 1;
			}
			else
			{
				Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict.TryAdd(hex, 1);
			}
		}
		if (updateGraphic &&Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
	}

	public Dictionary<Hex, float> CalculateVision()
	{
		Queue<Hex> frontier = new();
		frontier.Enqueue(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex);
		Dictionary<Hex, float> reached = new();
		reached.Add(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex, 0.0f);

		while (frontier.Count > 0)
		{
			Hex current = frontier.Dequeue();

			foreach (Hex next in current.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
			{
				float sightLeft = sightRange - reached[current];
				float visionCost = VisionCost(Global.gameManager.game.mainGameBoard.gameHexDict[next.WrapHex()], sightLeft); //vision cost is at most the cost of our remaining sight if we have atleast 1
				if (visionCost <= sightLeft)
				{
					if (!reached.Keys.Contains(next))
					{
						if(reached[current]+visionCost < sightRange)
						{
							frontier.Enqueue(next);
						}
						reached.Add(next, reached[current]+visionCost);
					}
					else if(reached[next] > reached[current]+visionCost)
					{
						reached[next] = reached[current]+visionCost;
					}
				}
			}
		}
		return reached;
	}
	

	public float VisionCost(GameHex targetGameHex, float sightLeft)
	{
		float visionCost = 0.0f;
		float cost = 0.0f;
		foreach (FeatureType feature in targetGameHex.featureSet)
		{
			if (feature == FeatureType.Forest)
			{
				visionCost += sightCosts[TerrainMoveType.Forest];
			}
			if (feature == FeatureType.Coral)
			{
				visionCost += sightCosts[TerrainMoveType.Coral];
			}
		}
		if(sightCosts.TryGetValue((TerrainMoveType)targetGameHex.terrainType, out cost))
		{
			visionCost += cost;
		}
		if(sightLeft >= 1)
		{
			return Math.Min(visionCost, sightLeft);
		}
		else
		{
			return visionCost;
		}
	}

	public Dictionary<Hex, float> MovementRange()
	{
		//breadth first using movement speed and move costs
		Queue<Hex> frontier = new();
		frontier.Enqueue(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex);
		Dictionary<Hex, float> reached = new();
		reached.Add(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex, 0.0f);
		Dictionary<Hex, GameHex> gameHexDict = Global.gameManager.game.mainGameBoard.gameHexDict;

		Dictionary<Hex, float> tempreached = new();

		while (frontier.Count > 0)
		{
			Hex current = frontier.Dequeue();

			foreach (Hex next in current.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
			{
				float movementLeft = remainingMovement - reached[current];
				float moveCost = TravelCost(current, next,Global.gameManager.game.teamManager, gameHexDict, true, movementCosts, movementSpeed, reached[current], false); 
				if (moveCost <= movementLeft)
				{
					if (!reached.Keys.Contains(next))
					{
						if(reached[current]+moveCost < remainingMovement)
						{
							frontier.Enqueue(next);
						}
						reached.Add(next, reached[current]+moveCost);
					}
					else if(reached[next] > reached[current]+moveCost)
					{
						reached[next] = reached[current]+moveCost;
					}
				}
			}
		}
		foreach(Hex tempHex in  tempreached.Keys)
		{
			reached.Remove(tempHex);
		}
		
		return reached;
	}
	

	public bool SetGameHex(GameHex newGameHex)
	{
		hex = newGameHex.hex;
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Move);
		return true;
	}

	public bool SafeSetHex(Hex newHex)
	{
		if (Global.gameManager.game.mainGameBoard.gameHexDict[newHex].units.Any())
		{
			GD.Print("Invalid SetHex for unit");
			return false;
		}

		Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(id);
		Hex previousHex = hex;
		hex = newHex;
		Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Add(id);
		UpdateVision();
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
		{
			manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Move);
			var previousHexData = new Godot.Collections.Dictionary
							{
								{ "q", previousHex.q },
								{ "r", previousHex.r },
								{ "s", previousHex.s }
							};
			var hexData = new Godot.Collections.Dictionary
							{
								{ "q", hex.q },
								{ "r", hex.r },
								{ "s", hex.s }
							};
			Global.gameManager.graphicManager.CallDeferred("UpdateHexObjectDictionary", previousHexData, id, hexData);
		}

		return true;
	}

	public void ApplyBleed(int duration, int strength)
	{
		if(strength >= bleedStrength)
		{
			bleedDuration = duration;
			bleedStrength = strength;
		}
	}

	public bool TrySetGameHex(GameHex targetGameHex, bool flexible = false)
	{
		if (targetGameHex.units.Any() || movementCosts[(TerrainMoveType)targetGameHex.terrainType] > 100 || movementCosts[(TerrainMoveType)targetGameHex.terrainType] < 0 
			|| (targetGameHex.district != null
				&& targetGameHex.district.health > 0
				&& !Global.gameManager.game.teamManager.GetAllies(teamNum).Contains(Global.gameManager.game.cityDictionary[targetGameHex.district.cityID].teamNum)))
		{
			if (flexible)
			{
				foreach (Hex rangeHex in hex.WrappingRange(3, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom).OrderBy(h => hex.Distance(h)))
				{
					if (TrySetGameHex(targetGameHex, flexible))
					{
						return true;
					}
				}
			}
			return false;
		}
		else if (movementCosts[(TerrainMoveType)targetGameHex.terrainType] < 100 && movementCosts[(TerrainMoveType)targetGameHex.terrainType] >= 0)
		{
			Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(this.id);
			Hex previousHex = hex;
			hex = targetGameHex.hex;
			Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Add(this.id);
			UpdateVision();
			if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
			{
				manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Move);
				var previousHexData = new Godot.Collections.Dictionary
							{
								{ "q", previousHex.q },
								{ "r", previousHex.r },
								{ "s", previousHex.s }
							};
				var hexData = new Godot.Collections.Dictionary
							{
								{ "q", hex.q },
								{ "r", hex.r },
								{ "s", hex.s }
							};
				Global.gameManager.graphicManager.CallDeferred("UpdateHexObjectDictionary", previousHexData, id, hexData);
			}
			return true;
		}
		return false;
	}

	public bool TryMoveToGameHex(GameHex targetGameHex, TeamManager teamManager)
	{
		if(targetGameHex.units.Any() & !isTargetEnemy)
		{
			return false;
		}
		float moveCost = TravelCost(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex, targetGameHex.hex, teamManager, isTargetEnemy, movementCosts, movementSpeed, movementSpeed-remainingMovement, false);
		if(moveCost <= remainingMovement)
		{
			if(isTargetEnemy & targetGameHex.hex.Equals(currentPath.Last()))
			{
				if (attacksLeft > 0)
				{
					if (AttackTarget(targetGameHex, moveCost, teamManager))
					{
						Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(this.id);
						Hex previousHex = hex;
						hex = targetGameHex.hex;
						Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Add(this.id);
						if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null
							&& Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isCityCenter
							&& Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].teamNum))
						{
							Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].DistrictFell();
						}
						UpdateVision();
						if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
						{
							manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Move);
							var previousHexData = new Godot.Collections.Dictionary
							{
								{ "q", previousHex.q },
								{ "r", previousHex.r },
								{ "s", previousHex.s }
							};
							var hexData = new Godot.Collections.Dictionary
							{
								{ "q", hex.q },
								{ "r", hex.r },
								{ "s", hex.s }
							};
							Global.gameManager.graphicManager.CallDeferred("UpdateHexObjectDictionary", previousHexData, id, hexData);
						}
						return true;
					}
				}
			}
			else if(!targetGameHex.units.Any() && (targetGameHex.district==null || targetGameHex.district.health == 0 || Global.gameManager.game.teamManager.GetAllies(teamNum).Contains(Global.gameManager.game.cityDictionary[targetGameHex.district.cityID].teamNum)))
			{
				SetRemainingMovement(remainingMovement - moveCost);
				Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(this.id);
				Hex previousHex = hex;
				hex = targetGameHex.hex;
				Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Add(this.id);

				if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null 
					&& Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isCityCenter 
					&& Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].teamNum))
				{
					Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].DistrictFell();
				}

				UpdateVision();
				if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
				{
					manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Move);
					var previousHexData = new Godot.Collections.Dictionary
							{
								{ "q", previousHex.q },
								{ "r", previousHex.r },
								{ "s", previousHex.s }
							};
					var hexData = new Godot.Collections.Dictionary
							{
								{ "q", hex.q },
								{ "r", hex.r },
								{ "s", hex.s }
							};
					Global.gameManager.graphicManager.CallDeferred("UpdateHexObjectDictionary", previousHexData, id, hexData);
					Global.gameManager.graphicManager.CallDeferred("PlayersCheckVisibility", hexData);

				}
				return true;
			}
		}
		return false;
	}

/*    public bool MoveToGameHex(GameHex targetGameHex)
	{
		Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(this.id);
		hex = targetGameHex.hex;
		Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Add(this.id);
		UpdateVision();
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Move);
		return true;
	}*/

	public bool MoveTowards(GameHex targetGameHex, TeamManager teamManager, bool isTargetEnemy)
	{
		fortifying = false;
		fortifyStrength = 0;
		Global.gameManager.graphicManager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
		RecalculateEffects();
		isSleeping = false;
		isSkipping = true;
		this.isTargetEnemy = isTargetEnemy;
		
		currentPath = PathFind(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex, targetGameHex.hex,Global.gameManager.game.teamManager, movementCosts, movementSpeed, out float temp);
		currentPath.Remove(Global.gameManager.game.mainGameBoard.gameHexDict[hex].hex);
		while (currentPath.Count > 0)
		{
			GameHex nextHex = Global.gameManager.game.mainGameBoard.gameHexDict[currentPath[0]];
			if (!TryMoveToGameHex(nextHex, teamManager))
			{
				FailedConsecutiveMovements++;
				if (FailedConsecutiveMovements > 3)
				{
					Global.Log($"Unit movement failed {FailedConsecutiveMovements} times in a row!",true,Global.LogLevel.Error);
				}
				return false;
			}
			FailedConsecutiveMovements = 0;
			currentPath.Remove(nextHex.hex);
		}
		return true;
	}

	public void CancelMovement()
	{
		currentPath.Clear();
		isTargetEnemy = false;
		if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
	}

	public float TravelCost(Hex first, Hex second, TeamManager teamManager, bool isTargetEnemy, Dictionary<TerrainMoveType, float> movementCosts, float unitMovementSpeed, float costSoFar, bool ignoreUnits, bool calculateZoneOfControl = true)
	{
		Dictionary<Hex, GameHex> gameHexDict = Global.gameManager.game.mainGameBoard.gameHexDict;
		return TravelCost(first, second, Global.gameManager.game.teamManager, gameHexDict, isTargetEnemy, movementCosts, movementSpeed, costSoFar, ignoreUnits, calculateZoneOfControl);
	}

	public float TravelCost(Hex first, Hex second, TeamManager teamManager, Dictionary<Hex, GameHex> gameHexDict, bool isTargetEnemy, Dictionary<TerrainMoveType, float> movementCosts, float unitMovementSpeed, float costSoFar, bool ignoreUnits, bool calculateZoneOfControl = true)
	{
		//cost for river, embark, disembark are custom (0 = end turn to enter, 1/2/3/4 = normal cost)\\
		GameHex firstHex;
		GameHex secondHex;
		if (!Global.gameManager.game.mainGameBoard.gameHexDict.TryGetValue(first, out firstHex)) //if firstHex is somehow off the table return max
		{
			return 111111;
		}
		if (!Global.gameManager.game.mainGameBoard.gameHexDict.TryGetValue(second, out secondHex)) //if secondHex is off the table return max
		{
			return 333333;
		}
		float moveCost = 222222; //default value should be set
		if (firstHex.terrainType == TerrainType.Coast || firstHex.terrainType == TerrainType.Ocean) //first hex is on water
		{
			if (secondHex.terrainType == TerrainType.Coast || secondHex.terrainType == TerrainType.Ocean) //second hex is on coast so we pay the normal cost
			{
				moveCost = movementCosts[(TerrainMoveType)secondHex.terrainType];
				foreach (FeatureType feature in secondHex.featureSet)
				{
					if (feature == FeatureType.Coral)
					{
						moveCost += movementCosts[TerrainMoveType.Coral];
					}
				}
			}
			else //second hex is on land so we are disembarking
			{
				if(movementCosts[TerrainMoveType.Disembark] < 0) //we CANT disembark
				{
					moveCost = 777777;
				}
				else if(movementCosts[TerrainMoveType.Disembark] == 0) //we must use all remaining movement to disembark
				{
					moveCost = (costSoFar % unitMovementSpeed == 0) ? unitMovementSpeed : costSoFar % unitMovementSpeed;
				}
				else //otherwise treat it like a normal land move
				{
					moveCost = movementCosts[(TerrainMoveType)secondHex.terrainType];
					foreach (FeatureType feature in secondHex.featureSet)
					{
						if (feature == FeatureType.Road)
						{
							moveCost = movementCosts[TerrainMoveType.Road];
							break;
						}
						if (feature == FeatureType.River && movementCosts[TerrainMoveType.River] == 0) //if river apply river penalty
						{
							moveCost = (costSoFar % unitMovementSpeed == 0) ? unitMovementSpeed : costSoFar % unitMovementSpeed;
						}
						if (feature == FeatureType.Forest) //if there is a forest add movement penalty
						{
							moveCost += movementCosts[TerrainMoveType.Forest];
						}
					}
				}
			}
		}
		else //first hex is on land
		{
			if (secondHex.terrainType == TerrainType.Coast || secondHex.terrainType == TerrainType.Ocean) //second hex is on water
			{
				//embark costs all remaining movement and requires at least 1 so costSoFar % unitMovementSpeed = cost or if == 0 then = unitMovementSpeed
				if (movementCosts[TerrainMoveType.Embark] < 0) //we CANT embark
				{
					moveCost = 666666;
				}
				else if (movementCosts[TerrainMoveType.Embark] == 0)
				{
					moveCost = (costSoFar % unitMovementSpeed == 0) ? unitMovementSpeed : costSoFar % unitMovementSpeed;
				}
				else//if we have a non-0 embark speed work like normal water
				{
					moveCost = movementCosts[(TerrainMoveType)secondHex.terrainType];
					foreach (FeatureType feature in secondHex.featureSet)
					{
						if (feature == FeatureType.Coral)
						{
							moveCost += movementCosts[TerrainMoveType.Coral];
						}
					}
				}
			}
			else //second hex is on land
			{
				moveCost = movementCosts[(TerrainMoveType)secondHex.terrainType];
				foreach (FeatureType feature in secondHex.featureSet)
				{
					if (feature == FeatureType.Road)
					{
						moveCost = movementCosts[TerrainMoveType.Road];
						break;
					}
					if (feature == FeatureType.River && movementCosts[TerrainMoveType.River] == 0) //if river apply river penalty
					{
						moveCost = (costSoFar % unitMovementSpeed == 0) ? unitMovementSpeed : costSoFar % unitMovementSpeed;
					}
					if (feature == FeatureType.Forest) //if there is a forest add movement penalty
					{
						moveCost += movementCosts[TerrainMoveType.Forest];
					}
				}
			}
		}
		//check for units
		if(!ignoreUnits)
		{
			foreach (int unitID in secondHex.units)
			{
				Unit unit = Global.gameManager.game.unitDictionary[unitID];
				if (isTargetEnemy && teamManager.GetEnemies(teamNum).Contains(unit.teamNum) && attacksLeft > 0)
				{
					break;
				}
				else
				{
					moveCost = 555555;
				}
			}
		}
		//check for districts, your districts OK, all others are a no no, unless attacking enemy OR its dead
		if (secondHex.district != null)
		{
			var districtTeam = Global.gameManager.game.cityDictionary[secondHex.district.cityID].teamNum;
			bool isDistrictEnemy = teamManager.GetEnemies(teamNum).Contains(districtTeam);
			bool isDistrictMine = districtTeam == teamNum;

			// Add cost unless target is enemy AND district is enemy AND health > 0
			if ((isTargetEnemy && isDistrictEnemy && secondHex.district.health <= 0) || isDistrictMine)
			{
				GD.Print("Enemy District or Your District" + isTargetEnemy + " " + isDistrictEnemy + " " + secondHex.district.health + " " + isDistrictMine);
			}
			else
			{
				GD.Print("Non-Enemy District" + isTargetEnemy + " " + isDistrictEnemy + " " + secondHex.district.health);
				moveCost += 12121212;
			}
		}
		if (moveCost < 9999)
		{
			moveCost = Math.Min(moveCost, unitMovementSpeed);
		}
		if(moveCost < 0)
		{
			moveCost = 888888;
		}

		//zone of control check, if a neighbor has an enemy unit that applies zone of control increase movecost by 5
		if(calculateZoneOfControl && !ignoreZoneOfControl)
		{
			foreach(Hex hex in firstHex.hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
			{
				if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Any())
				{
					Unit unit = Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].units[0]];
					if (unit.zoneOfControl && Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
					{
						float newMoveCost = (costSoFar % unitMovementSpeed == 0) ? unitMovementSpeed : costSoFar % unitMovementSpeed;
						if (moveCost < newMoveCost)
						{
							moveCost = newMoveCost;
						}
						break;
					}
				}
			}
		}
		return moveCost;
	}



	private int AstarHeuristic(Hex start, Hex end)
	{
		return start.WrapDistance(end);
	}

	public List<Hex> PathFind(Hex start, Hex end, TeamManager teamManager, Dictionary<TerrainMoveType, float> movementCosts, float unitMovementSpeed, out float totalCost)
	{
		PriorityQueue<Hex, float> frontier = new();
		frontier.Enqueue(start, 0);
		Dictionary<Hex, float> cost_so_far = new();
		Dictionary<Hex, Hex> came_from = new();
		came_from[start] = start;
		cost_so_far[start] = 0;
		if (start.Equals(end))
		{
			totalCost = 0;
			return new List<Hex>();
		}

		while (frontier.TryDequeue(out Hex current, out float priority))
		{
			if (current.Equals(end))
			{
				if (cost_so_far[current] > 100)
				{
					totalCost = cost_so_far[current];
					//GD.Print("Pathfind fail1");
					return new List<Hex>();
				}
				List<Hex> path = new List<Hex>();
				while (!current.Equals(start))
				{
					path.Add(current);
					current = came_from[current];
				}
				path.Add(start);
				path.Reverse();
				totalCost = cost_so_far[current];
				return path;
			}

			foreach (Hex next in current.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
			{
				float new_cost = cost_so_far[current] + TravelCost(current, next, teamManager, isTargetEnemy, movementCosts, unitMovementSpeed, cost_so_far[current], false, false);
				//if cost_so_far doesn't have next as a key yet or the new cost is lower than the lowest cost of this node previously
				if (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
				{
					cost_so_far[next] = new_cost;
					float new_priority = new_cost + AstarHeuristic(end, next);
					frontier.Enqueue(next, new_priority);
					came_from[next] = current;
				}
			}
		}
		//if the end is unreachable return an empty path
		GD.Print("Pathfind fail2");
		totalCost = 999999;
		return new List<Hex>();
	}
}
