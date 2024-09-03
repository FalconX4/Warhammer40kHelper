using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Keywords : MonoBehaviour
{
	[Header("Data")]
	public TextMeshProUGUI KeywordsText;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	public void SetData(List<string> keywords, bool team1)
	{
		KeywordsText.text = $"KEYWORDS: <b>{string.Join("</b>, <b>", keywords)}</b>";

		var teamUI = Database.Instance.RacesScriptableObject.Get(team1 ? Database.Instance.Team1ScriptableObject.Race : Database.Instance.Team2ScriptableObject.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;
	}
}
