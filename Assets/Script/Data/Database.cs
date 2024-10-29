using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
	[Header("Matchup")]
	public TeamScriptableObject Team1ScriptableObject;
	public TeamScriptableObject Team2ScriptableObject;
	public RulesScriptableObject RulesScriptableObject;

	[Header("Skippable")]
	public ListStringScriptableObject SkippableAbilitiesScriptableObject;
	public ListStringScriptableObject SkippableRulesScriptableObject;

	[Header("Rewritten")]
	public NameDescriptionScriptableObject RewrittenAbilitiesScriptableObject;
	public NameDescriptionScriptableObject RewrittenRulesScriptableObject;
	public NameDescriptionScriptableObject RewrittenWeaponsRulesScriptableObject;

	[Header("Database")]
	public StratagemsScriptableObject StratagemsScriptableObject;
	public RacesScriptableObject RacesScriptableObject;

	public bool ShowRewrittenText { get; set; }
	Dictionary<string, string> _rules = new();
	Dictionary<string, string> _rewrittenAbilities = new();
	Dictionary<string, string> _rewrittenRules = new();
	Dictionary<string, string> _rewrittenWeaponsRules = new();

	public static Database Instance;
	private void Awake()
	{
		Instance = this;
		for (int i = 0; i < RulesScriptableObject.Rules.Count; i++)
			_rules[RulesScriptableObject.Rules[i].Name] = RulesScriptableObject.Rules[i].Rule;
		for (int i = 0; i < RewrittenAbilitiesScriptableObject.NameDescription.Count; i++)
			_rewrittenAbilities[RewrittenAbilitiesScriptableObject.NameDescription[i].Name] = RewrittenAbilitiesScriptableObject.NameDescription[i].Description;
		for (int i = 0; i < RewrittenRulesScriptableObject.NameDescription.Count; i++)
			_rewrittenRules[RewrittenRulesScriptableObject.NameDescription[i].Name] = RewrittenRulesScriptableObject.NameDescription[i].Description;
		for (int i = 0; i < RewrittenWeaponsRulesScriptableObject.NameDescription.Count; i++)
			_rewrittenWeaponsRules[RewrittenWeaponsRulesScriptableObject.NameDescription[i].Name] = RewrittenWeaponsRulesScriptableObject.NameDescription[i].Description;
		DatabaseSaveLoad.LoadFromFile(RulesScriptableObject, Team1ScriptableObject, Team2ScriptableObject);
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public string FindAbilityName(string abilityName) => SkippableAbilitiesScriptableObject.List.Contains(abilityName) ? string.Empty : ShowRewrittenText && _rewrittenAbilities.ContainsKey(abilityName) ? _rewrittenAbilities[abilityName] : abilityName;
	public string FindRuleName(string ruleName) => Contains(SkippableRulesScriptableObject.List, ruleName) ? string.Empty : ShowRewrittenText && ContainsKey(_rewrittenRules, ruleName) ? GetValue(_rewrittenRules, ruleName) : ruleName;
	public string FindRuleShortenedName(string ruleName) => Contains(SkippableRulesScriptableObject.List, ruleName) ? string.Empty : ShowRewrittenText && ContainsKey(_rewrittenRules, ruleName) ? GetKeyWithKey(_rewrittenRules, ruleName) : ruleName;
	public string FindWeaponRuleName(string weaponRuleName) => ShowRewrittenText && ContainsKey(_rewrittenWeaponsRules, weaponRuleName) ? GetValue(_rewrittenWeaponsRules, weaponRuleName) : weaponRuleName;
	public string FindAbilityDescription(string abilityName, string abilityDescription) => SkippableAbilitiesScriptableObject.List.Contains(abilityName) ? string.Empty : ShowRewrittenText && _rewrittenAbilities.ContainsKey(abilityName) ? GetValue(_rewrittenAbilities, abilityName) : abilityDescription;
	public string FindRuleDescription(string ruleName) => Contains(SkippableRulesScriptableObject.List, ruleName) ? string.Empty : ShowRewrittenText && ContainsKey(_rewrittenRules, ruleName) ? ruleName : GetValue(_rules, ruleName);
	public string FindWeaponRuleDescription(string weaponRuleName) => ShowRewrittenText && ContainsKey(_rewrittenWeaponsRules, weaponRuleName) ? weaponRuleName : GetValue(_rules, weaponRuleName);
	public string FindWeaponRuleUsingDescription(string weaponRuleDescription) => GetKeyWithValue(_rewrittenWeaponsRules, weaponRuleDescription);

	bool Contains(List<string> list, string check)
	{
		for (int i = 0; i < list.Count; i++)
			if (check.Contains(list[i]))
				return true;
		return false;
	}

	bool ContainsKey(Dictionary<string, string> dictionary, string check)
	{
		foreach (var data in dictionary)
			if (check.Contains(data.Key))
				return true;
		return false;
	}

	string GetKeyWithKey(Dictionary<string, string> dictionary, string check)
	{
		foreach (var data in dictionary)
			if (check.Contains(data.Key))
				return data.Key;
		return check;
	}

	string GetKeyWithValue(Dictionary<string, string> dictionary, string check)
	{
		foreach (var data in dictionary)
			if (check.Contains(data.Value))
				return data.Key;
		return check;
	}

	string GetValue(Dictionary<string, string> dictionary, string check)
	{
		foreach (var data in dictionary)
			if (check.ToUpper().Contains(data.Key.ToUpper()))
				return data.Value;
		return check;
	}

	public static int WoundRoll(int S, int T)
	{
		if (S >= T * 2) return 2;
		else if (S > T) return 3;
		else if (S == T) return 4;
		else if (S < T / 2) return 6;
		else return 5;
	}
}
