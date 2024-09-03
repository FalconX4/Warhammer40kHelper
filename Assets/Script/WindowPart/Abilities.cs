using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
	[Header("Create")]
	public TextMeshProUGUI Rules;
	public TextMeshProUGUI AbilityPrefab;
	public RectTransform AbilitiesParent;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	public void SetData(List<string> rules, List<TeamScriptableObject.AbilityData> abilities, bool team1)
	{
		Utility.ClearChilds(AbilitiesParent, 1);

		Rules.text = $"RULES: ";
		for (int i = 0; i < rules.Count; i++)
		{
			var ruleDescription = Database.Instance.FindRuleName(rules[i]);
			if (string.IsNullOrEmpty(ruleDescription))
				continue;

			if (ruleDescription.Contains("{x}"))
				ruleDescription = ruleDescription.Replace("{x}", rules[i].Substring(Database.Instance.FindRuleShortenedName(rules[i]).Length + 1));
			var text = Database.Instance.ShowRewrittenText ? ruleDescription : rules[i];
			Rules.text += $"<b>{text}</b>, ";
		}
		Rules.text = Rules.text.Remove(Rules.text.Length - 2);

		for (int i = 0; i < abilities.Count; i++)
		{
			var description = Database.Instance.FindAbilityDescription(abilities[i].Name, abilities[i].Ability);
			if (string.IsNullOrEmpty(description))
				continue;

			var abilityInstance = Instantiate(AbilityPrefab, AbilitiesParent);
			abilityInstance.text = $"<b>{abilities[i].Name}:</b> {description}";
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(AbilitiesParent);

		var teamUI = Database.Instance.RacesScriptableObject.Get(team1 ? Database.Instance.Team1ScriptableObject.Race : Database.Instance.Team2ScriptableObject.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;
	}
}
