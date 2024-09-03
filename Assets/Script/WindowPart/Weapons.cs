using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weapons : MonoBehaviour
{
	[Header("Create")]
	public Weapon WeaponPrefab;
	public VerticalLayoutGroup Layout;
	public Transform RangeParent;
	public Transform MeleeParent;
	public RectTransform RangeContainer;
	public RectTransform MeleeContainer;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	public void SetData(List<TeamScriptableObject.WeaponData> unitData, bool team1)
	{
		Utility.ClearChilds(RangeParent);
		Utility.ClearChilds(MeleeParent);

		var hadRange = false;
		var hadMelee = false;
		foreach (var data in unitData)
		{
			var isMelee = data.Range == "Melee";
			var weapon = Instantiate(WeaponPrefab, isMelee ? MeleeParent : RangeParent);
			weapon.SetData(data, team1);
			hadMelee |= isMelee;
			hadRange |= !isMelee;
		}

		var prefabHeight = (WeaponPrefab.transform as RectTransform).sizeDelta.y;
		RangeContainer.gameObject.SetActive(hadRange);
		MeleeContainer.gameObject.SetActive(hadMelee);
		RangeContainer.sizeDelta = new Vector2(RangeContainer.sizeDelta.x, (RangeParent.childCount + 1) * prefabHeight);
		MeleeContainer.sizeDelta = new Vector2(MeleeContainer.sizeDelta.x, (MeleeParent.childCount + 1) * prefabHeight);

		var teamUI = Database.Instance.RacesScriptableObject.Get(team1 ? Database.Instance.Team1ScriptableObject.Race : Database.Instance.Team2ScriptableObject.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;

		Layout.SetLayoutVertical();
	}
}
