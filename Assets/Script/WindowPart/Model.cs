using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Model : MonoBehaviour
{
	[Header("Stats")]
	public TextMeshProUGUI Name;
	public TextMeshProUGUI M;
	public TextMeshProUGUI T;
	public TextMeshProUGUI Sv;
	public TextMeshProUGUI W;
	public TextMeshProUGUI Ld;
	public TextMeshProUGUI OC;
	public TextMeshProUGUI ISV;
	public TextMeshProUGUI ISVText;
	public GameObject ISVContainer;
	public Image Image;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	List<TeamScriptableObject.UnitData> _unitDatas;
	int _index;
	TeamScriptableObject.UnitData _unitData;
	bool _team1;

	public void SetData(List<TeamScriptableObject.UnitData> unitDatas, int index, bool team1)
	{
		_unitDatas = unitDatas;
		_index = index;
		SetData(_unitDatas[_index], null, team1);
	}

	public void SetData(TeamScriptableObject.UnitData unitData, bool team1) => SetData(unitData, null, team1);
	public void SetData(TeamScriptableObject.UnitData unitData, Sprite sprite, bool team1)
	{
		_unitData = unitData;
		_team1 = team1;
		Name.text = unitData.Name;
		M.text = unitData.Model.M;
		T.text = unitData.Model.T;
		Sv.text = unitData.Model.Sv;
		W.text = unitData.Model.W;
		Ld.text = unitData.Model.Ld;
		OC.text = unitData.Model.OC;
		if (ISVText == null)
			ISV.text = unitData.ISv.Value + (unitData.ISv.Type != TeamScriptableObject.InvulnerableType.All ? "*" : "");
		else
		{
			ISV.text = unitData.ISv.Value;
			ISVText.text = "Invulnerable Save" + (unitData.ISv.Type != TeamScriptableObject.InvulnerableType.All ? "*" : "");
		}

		if (ISVContainer)
			ISVContainer.SetActive(!string.IsNullOrEmpty(unitData.ISv.Value));
		if (Image)
		{
			Image.sprite = sprite;
			Image.gameObject.SetActive(sprite != null);
		}

		var teamUI = Database.Instance.RacesScriptableObject.Get(team1 ? Database.Instance.Team1ScriptableObject.Race : Database.Instance.Team2ScriptableObject.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;
	}

	public void ShowUnitWindow() => UIManager.Instance.ShowUnitWindow(_unitDatas, _index, _team1);
	public void ShowDefenseWindow() => UIManager.Instance.ShowDefenseWindow(_unitData, _team1);
	public void ShowISVExplainWindow() => UIManager.Instance.ShowExplanationWindow(ISV.transform.position, $"This unit has {_unitData.ISv.Value} invulnerable save against {_unitData.ISv.Type} attacks.", _team1);
}
