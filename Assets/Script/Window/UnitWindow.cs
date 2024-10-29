using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitWindow : MonoBehaviour
{
	[Header("Parts")]
	public Abilities Abilities;
	public Keywords Keywords;
	public Model Model;
	public Weapons Weapons;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	List<TeamScriptableObject.UnitData> _unitDatas;
	int _index;
	TeamScriptableObject.UnitData _unitData;
	bool _team1;

	public void Refresh() => SetData(_unitData, _team1);
	public void SetData(List<TeamScriptableObject.UnitData> unitDatas, int index, bool team1)
	{
		_unitDatas = unitDatas;
		_index = index;
		SetData(_unitDatas[_index], team1);
	}

	public void SetData(TeamScriptableObject.UnitData unitData, bool team1)
	{
		_unitData = unitData;
		_team1 = team1;
		Abilities.SetData(unitData.Rules, unitData.Abilities, team1);
		Keywords.SetData(unitData.Categories, team1);
		Model.SetData(unitData, Resources.Load<Sprite>(unitData.SpritePath), team1);
		Weapons.SetData(unitData.Weapons, team1);

		var teamUI = Database.Instance.RacesScriptableObject.Get(team1 ? Database.Instance.Team1ScriptableObject.Race : Database.Instance.Team2ScriptableObject.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;
	}

	public void Previous()
	{
		_index = (int)Mathf.Repeat(_index - 1, _unitDatas.Count);
		SetData(_unitDatas[_index], _team1);
	}

	public void Next()
	{
		_index = (int)Mathf.Repeat(_index + 1, _unitDatas.Count);
		SetData(_unitDatas[_index], _team1);
	}
}
