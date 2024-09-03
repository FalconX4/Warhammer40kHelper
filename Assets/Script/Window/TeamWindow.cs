using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamWindow : MonoBehaviour
{
	[Header("Create")]
	public Model ModelPrefab;
	public Transform Team1Parent;
	public Transform Team2Parent;
	public RectTransform Team1Container;
	public RectTransform Team2Container;

	[Header("UI")]
	public Image Team1BackgroundFrame;
	public Image Team2BackgroundFrame;
	public Image Team1HeaderBackgroundFrame;
	public Image Team2HeaderBackgroundFrame;
	public Image TeamHeaderBackgroundFrame;
	public TMPro.TextMeshProUGUI TeamHeaderText;

	public void OnEnable()
	{
		SetData(Database.Instance.Team1ScriptableObject.TeamData, Team1Parent, Team1Container, true);
		SetData(Database.Instance.Team2ScriptableObject.TeamData, Team2Parent, Team2Container, false);

		var team1UI = Database.Instance.RacesScriptableObject.Get(Database.Instance.Team1ScriptableObject.Race);
		var team2UI = Database.Instance.RacesScriptableObject.Get(Database.Instance.Team2ScriptableObject.Race);
		Team1BackgroundFrame.color = team1UI.FrameColor;
		Team2BackgroundFrame.color = team2UI.FrameColor;
		Team1HeaderBackgroundFrame.color = team1UI.FrameColor;
		Team2HeaderBackgroundFrame.color = team2UI.FrameColor;
		TeamHeaderBackgroundFrame.color = Color.Lerp(team1UI.FrameColor, team1UI.FrameColor, 0.5f);
		TeamHeaderText.color = Color.Lerp(team1UI.FrameColor, team1UI.FrameColor, 0.5f);
	}

	void SetData(List<TeamScriptableObject.UnitData> teamData, Transform teamParent, RectTransform teamContainer, bool team1)
	{
		Utility.ClearChilds(teamParent, 1);

		for (int i = 0; i < teamData.Count; i++)
		{
			var model = Instantiate(ModelPrefab, teamParent);
			model.SetData(teamData, i, team1);
		}

		var prefabHeight = (ModelPrefab.transform as RectTransform).sizeDelta.y;
		teamContainer.sizeDelta = new Vector2(teamContainer.sizeDelta.x, teamParent.childCount * prefabHeight);
	}

	public void ShowStratagems(bool team1) => UIManager.Instance.ShowStratagemsWindow(team1 ? Database.Instance.Team1ScriptableObject : Database.Instance.Team2ScriptableObject);
}
