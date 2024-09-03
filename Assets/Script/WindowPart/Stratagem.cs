using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stratagem : MonoBehaviour
{
	public TextMeshProUGUI Name;
	public TextMeshProUGUI When;
	public TextMeshProUGUI Target;
	public TextMeshProUGUI Effect;
	public TextMeshProUGUI Restriction;
	public StratagemIcon IconPrefab;
	public Transform IconParent;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	public void SetData(StratagemsScriptableObject.StratagemData stratagemData)
	{
		var stratagemUI = Database.Instance.StratagemsScriptableObject.Get(stratagemData.Type);
		Name.text = stratagemData.Name;
		When.text = $"<color=#{ColorUtility.ToHtmlStringRGB(stratagemUI.Color)}>WHEN:</color> {stratagemData.When}";
		Target.text = $"<color=#{ColorUtility.ToHtmlStringRGB(stratagemUI.Color)}>TARGET:</color> {stratagemData.Target}";
		Effect.text = $"<color=#{ColorUtility.ToHtmlStringRGB(stratagemUI.Color)}>EFFECT:</color> {stratagemData.Effect}";
		Restriction.text = $"<color=#{ColorUtility.ToHtmlStringRGB(stratagemUI.Color)}>RESTRICTION:</color> {stratagemData.Restrictions}";
		Restriction.gameObject.SetActive(!string.IsNullOrEmpty(stratagemData.Restrictions));

		foreach (var icon in stratagemData.Icons)
		{
			var iconInstance = Instantiate(IconPrefab, IconParent);
			iconInstance.SetData(icon, stratagemData.CP, stratagemUI.Color);
		}

		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = stratagemUI.Color;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = stratagemUI.Color;
	}
}
