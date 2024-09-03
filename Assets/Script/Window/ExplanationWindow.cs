using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExplanationWindow : MonoBehaviour
{
	public RectTransform Transform;
	public ContentSizeFitter ExplanationContentSizeFitter;
	public TextMeshProUGUI Explanation;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	float width = 0;
	private void Awake()
	{
		width = Transform.sizeDelta.x;
	}

	public void SetData(string explain, bool team1)
	{
		ExplanationContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		Explanation.rectTransform.sizeDelta = new Vector2(width, Explanation.rectTransform.sizeDelta.y);
		Explanation.text = explain;
		LayoutRebuilder.ForceRebuildLayoutImmediate(Explanation.rectTransform);
		if (Explanation.rectTransform.sizeDelta.y < Explanation.textInfo.lineInfo[0].lineHeight + Explanation.margin.y * 2f + 1f)
		{
			ExplanationContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			LayoutRebuilder.ForceRebuildLayoutImmediate(Explanation.rectTransform);
		}
		Transform.sizeDelta = Explanation.rectTransform.sizeDelta;

		var teamUI = Database.Instance.RacesScriptableObject.Get(team1 ? Database.Instance.Team1ScriptableObject.Race : Database.Instance.Team2ScriptableObject.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;
	}

	public void Close()
	{
		gameObject.SetActive(false);
	}
}
