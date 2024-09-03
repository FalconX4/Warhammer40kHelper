using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
	public TextMeshProUGUI Name;
	public TextMeshProUGUI Range;
	public TextMeshProUGUI A;
	public TextMeshProUGUI Sk;
	public TextMeshProUGUI S;
	public TextMeshProUGUI AP;
	public TextMeshProUGUI D;
	public WeaponKeyword KeywordPrefab;
	public RectTransform KeywordsParent;
	public HorizontalLayoutGroup KeywordsLayout;

	bool _team1;
	TeamScriptableObject.WeaponData _data;

	public void SetData(TeamScriptableObject.WeaponData data, bool team1)
	{
		Utility.ClearChilds(KeywordsParent);

		_data = data;
		_team1 = team1;
		Name.text = data.Name;
		Range.text = data.Range == "Melee" ? string.Empty : data.Range;
		A.text = data.A;
		Sk.text = data.Sk;
		S.text = data.S;
		AP.text = data.AP;
		D.text = data.D;

		for (int i = 0; i < data.Keywords.Count; i++)
		{
			var keywordDescription = Database.Instance.FindWeaponRuleName(data.Keywords[i]);
			var keyword = Instantiate(KeywordPrefab, KeywordsParent);
			var text = Database.Instance.ShowRewrittenText ? keywordDescription : data.Keywords[i];
			if (string.IsNullOrEmpty(keywordDescription))
				keyword.SetData("-", team1);
			else
				keyword.SetData(text, team1);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(KeywordsParent);
		UIManager.Instance.StartCoroutine(EnableLayout());
	}

	IEnumerator EnableLayout()
	{
		KeywordsLayout.enabled = false;
		yield return null;
		KeywordsLayout.enabled = true;
	}

	public void OnNamePressed()
	{
		UIManager.Instance.ShowWeaponAttackWindow(_data, _team1);
	}
}
