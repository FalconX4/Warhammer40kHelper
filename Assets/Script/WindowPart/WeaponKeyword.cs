using TMPro;
using UnityEngine;

public class WeaponKeyword : MonoBehaviour
{
	string _text;
	bool _team1;

	public void SetData(string text, bool team1)
	{
		_text = text;
		_team1 = team1;
		GetComponent<TextMeshProUGUI>().text = $"[{text}]";
	}

	public void OnPressed()
	{
		var description = Database.Instance.ShowRewrittenText ? Database.Instance.FindWeaponRuleUsingDescription(_text) : Database.Instance.FindWeaponRuleDescription(_text);
		if (string.IsNullOrEmpty(description))
			UIManager.Instance.ShowExplanationWindow(transform.position, $"Does not contain definition of {_text}", _team1);
		else
			UIManager.Instance.ShowExplanationWindow(transform.position, description, _team1);
	}
}
