using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static TeamScriptableObject;

public static class NewRecruitDatabaseParser
{
	public static void ParseFiles(StratagemsScriptableObject stratagems, RulesScriptableObject rules, TeamScriptableObject team1, TeamScriptableObject team2, string team1Content, string team2Content)
	{
		JsonSerializerSettings settings = new JsonSerializerSettings { MaxDepth = 100 };
		Root team1Root = JsonConvert.DeserializeObject<Root>(team1Content, settings);
		Root team2Root = JsonConvert.DeserializeObject<Root>(team2Content, settings);

		FindDetachment(team1Root, team1, stratagems);
		FindDetachment(team2Root, team2, stratagems);
		FindUnits(team1Root, team1);
		FindUnits(team2Root, team2);

		rules.Rules.Clear();
		FindRulesData(team1Root, rules);
		FindRulesData(team2Root, rules);
	}

	public static void FindDetachment(Root teamRoot, TeamScriptableObject teamScriptableObject, StratagemsScriptableObject stratagems)
	{
		teamScriptableObject.Race = teamRoot.roster.name.Remove(0, teamRoot.roster.name.IndexOf('_') + 1);
		teamScriptableObject.Detachment = teamRoot.roster.forces[0].selections[1].selections[0].name;
		teamScriptableObject.StratagemsDefaultData = stratagems.Get("DEFAULT").Get("DEFAULT");
		var stratagemsRace = stratagems.GetUsingContains(teamScriptableObject.Race);
		teamScriptableObject.StratagemsTeamData = stratagemsRace.Get(teamScriptableObject.Detachment);
		teamScriptableObject.Race = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(stratagemsRace.Name.ToLower());
	}

	public static void FindRulesData(Root teamRoot, RulesScriptableObject rules)
	{
		foreach (var force in teamRoot.roster.forces)
			FindRulesInSelections(force.selections, rules);
	}

	public static void FindRulesInSelections(List<Selection> selections, RulesScriptableObject rules)
	{
		foreach (var selection in selections)
		{
			if (selection.rules != null)
			{
				foreach (var rule in selection.rules)
				{
					if (!rules.Contains(rule.name))
						rules.Rules.Add(new RulesScriptableObject.RulesData { Name = rule.name, Rule = rule.description });
				}
			}

			if (selection.selections != null)
				FindRulesInSelections(selection.selections, rules);
		}
	}

	public static void FindUnits(Root teamRoot, TeamScriptableObject team)
	{
		var teamData = team.TeamData;
		teamData.Clear();

		var selections = teamRoot.roster.forces[0].selections;
		for (int i = 0; i < selections.Count; i++)  // Exarch are not a new entry
		{
			var selection = selections[i];
			if (selection.type == "model")
				FindUnitData(selection, team, selection, false);
			else if (selection.type == "unit")
				for (int j = 0; j < selection.selections.Count; j++)
					FindUnitData(selection, team, selection.selections[j], true);
		}
	}

	public static void FindUnitData(Selection unit, TeamScriptableObject team, Selection selection, bool isUnit)
	{
		var data = new UnitData { Model = default, SameUnitCount = 1, Abilities = new List<AbilityData>(), Categories = new List<string>(), Rules = new List<string>(), Weapons = new List<WeaponData>() };
		data.ModelCount = isUnit ? selection.number : unit.number;
		FindAbilities(unit, ref data);
		FindCategoriesData(unit, ref data);
		FindModelData(isUnit && unit.selections.Count > 1 ? selection : unit, ref data);
		FindRules(unit, ref data);
		FindWeaponsData(selection, ref data);
		data.Sprite = Resources.Load<Sprite>($"Art/UnitPicture/{team.Race}/{data.Model.Name}");

		var teamData = team.TeamData;
		var alreadyIn = false;
		int j;
		for (j = 0; j < teamData.Count && !alreadyIn; j++)
			alreadyIn = teamData[j].Equals(data);

		if (alreadyIn)
		{
			data.SameUnitCount = teamData[j - 1].SameUnitCount + 1;
			teamData.RemoveAt(j - 1);
			teamData.Add(data);
		}
		else
			teamData.Add(data);
	}

	public static void FindAbilities(Selection model, ref UnitData data)
	{
		if (model.profiles != null)
		{
			for (int i = 0; i < model.profiles.Count; i++)
			{
				if (model.profiles[i].typeName != "Abilities")
					continue;

				var characteristic = model.profiles[i].characteristics[0];
				var description = characteristic.text.ToLower();
				var match = Regex.Match(description, @" a \d+\+ invulnerable save");
				// I dont know why but the string is weird and cannot find the word against when there is one...
				if (match.Success && description.Contains("attacks"))
				{
					var matchNumber = Regex.Match(description, @"\d+");
					var matchTypeStartIndex = description.IndexOf($" a {matchNumber.Value}+ invulnerable save") + $" a {matchNumber.Value}+ invulnerable save against ".Length;
					var matchTypeEndIndex = description.IndexOf(".", matchTypeStartIndex);
					var matchType = description.Substring(matchTypeStartIndex, matchTypeEndIndex - matchTypeStartIndex);

					data.ISv.Value = matchNumber.Value + "+";
					data.ISv.Type = matchType == "melee attacks" ? InvulnerableType.Melee : matchType == "ranged attacks" ? InvulnerableType.Ranged : InvulnerableType.Unkown;
					if (data.ISv.Type == InvulnerableType.Unkown)
						Debug.LogError($"{data.Model.Name} Invulnerable type is unknown : {matchType}");
				}
				else
				{
					match = Regex.Match(description, @" a \d+\+ invulnerable save");
					if (match.Success)
					{
						var matchNumber = Regex.Match(description, @"\d+");
						data.ISv.Value = matchNumber.Value + "+";
						data.ISv.Type = InvulnerableType.All;
					}
				}

				data.Abilities.Add(new AbilityData { Name = model.profiles[i].name, Ability = model.profiles[i].characteristics[0].text });
			}
		}

		if (model.selections != null)
			for (int i = 0; i < model.selections.Count; i++)
				FindAbilities(model.selections[i], ref data);
	}

	public static void FindCategoriesData(Selection model, ref UnitData data)
	{
		for (int i = 0; i < model.categories.Count; i++)
			data.Categories.Add(model.categories[i].name);
	}

	public static void FindModelData(Selection model, ref UnitData data)
	{
		var foundData = false;
		for (int i = 0; i < model.profiles.Count; i++)
		{
			var modelData = model.profiles[i];
			if (modelData.typeName != "Unit")
				continue;

			foundData = true;
			data.Model.Name = modelData.name;
			data.Model.M = modelData.characteristics[0].text;
			data.Model.T = modelData.characteristics[1].text;
			data.Model.Sv = modelData.characteristics[2].text;
			data.Model.W = modelData.characteristics[3].text;
			data.Model.Ld = modelData.characteristics[4].text;
			data.Model.OC = modelData.characteristics[5].text;
		}

		if (!foundData)
			for (int i = 0; i < model.selections.Count; i++)
				FindModelData(model.selections[i], ref data);
	}

	public static void FindRules(Selection model, ref UnitData data)
	{
		for (int i = 0; i < model.rules.Count; i++)
			data.Rules.Add(model.rules[i].name);
	}

	public static void FindWeaponsData(Selection model, ref UnitData data)
	{
		for (int i = 0; i < model.selections.Count; i++)
		{
			if (model.selections[i].type != "upgrade" || model.selections[i].profiles == null || model.selections[i].profiles.Count == 0)
				continue;

			for (int j = 0; j < model.selections[i].profiles.Count; j++)
			{
				var weapon = model.selections[i].profiles[j];
				if (!weapon.typeName.Contains("Weapons"))
					continue;

				var weaponData = new WeaponData();
				var weaponCount = model.selections[i].number / data.ModelCount;
				weaponData.Name = weaponCount > 1 ? $"{weaponCount}x {weapon.name}" : weapon.name;
				weaponData.Range = weapon.characteristics[0].text;
				weaponData.A = weapon.characteristics[1].text;
				weaponData.Sk = weapon.characteristics[2].text;
				weaponData.S = weapon.characteristics[3].text;
				weaponData.AP = weapon.characteristics[4].text;
				weaponData.D = weapon.characteristics[5].text;
				weaponData.Keywords = new List<string>(weapon.characteristics[6].text.Split(", "));
				weaponData.Type = weaponData.Range == "Melee" ? WeaponType.Melee : WeaponType.Ranged;
				data.Weapons.Add(weaponData);
			}
		}
	}

	#region JSON
	[Serializable]
	public class Category
	{
		public string id;
		public string name;
		public string entryId;
		public bool primary;
	}

	[Serializable]
	public class Characteristic
	{
		public string text;
		public string name;
		public string typeId;
	}

	[Serializable]
	public class Cost
	{
		public string name;
		public string typeId;
		public int value;
	}

	[Serializable]
	public class CostLimit
	{
		public string name;
		public string typeId;
		public int value;
	}

	[Serializable]
	public class Force
	{
		public List<Selection> selections;
		public List<Category> categories;
		public string id;
		public string name;
		public string entryId;
		public string catalogueId;
		public int catalogueRevision;
		public string catalogueName;
	}

	[Serializable]
	public class Profile
	{
		public List<Characteristic> characteristics;
		public string id;
		public string name;
		public bool hidden;
		public string typeId;
		public string typeName;
		public string from;
	}

	[Serializable]
	public class Root
	{
		public Roster roster;
	}

	[Serializable]
	public class Roster
	{
		public List<Cost> costs;
		public List<CostLimit> costLimits;
		public List<Force> forces;
		public string id;
		public string name;
		public double battleScribeVersion;
		public string generatedBy;
		public string gameSystemId;
		public string gameSystemName;
		public int gameSystemRevision;
		public string xmlns;
	}

	[Serializable]
	public class Rule
	{
		public string description;
		public string id;
		public string name;
		public bool hidden;
		public int? page;
	}

	[Serializable]
	public class Selection
	{
		public List<Selection> selections;
		public List<Category> categories;
		public string id;
		public string name;
		public string entryId;
		public int number;
		public string type;
		public string from;
		public List<Rule> rules;
		public List<Profile> profiles;
		public List<Cost> costs;
		public string entryGroupId;
		public string group;
	}
	#endregion
}
