using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAttackWindow : MonoBehaviour
{
	public AttackedModel ModelPrefab;
	public Transform ModelParent;
	public RectTransform Container;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	public void SetData(TeamScriptableObject.WeaponData weaponData, bool team1)
	{
		Utility.ClearChilds(ModelParent, 1);

		List<AttackedModel> children = new List<AttackedModel>();
		var teamData = team1 ? Database.Instance.Team2ScriptableObject : Database.Instance.Team1ScriptableObject;
		foreach (var data in teamData.TeamData)
		{
			var model = Instantiate(ModelPrefab, ModelParent);
			if ((data.ISv.Type == TeamScriptableObject.InvulnerableType.All) ||
				(data.ISv.Type == TeamScriptableObject.InvulnerableType.Ranged && weaponData.Type == TeamScriptableObject.WeaponType.Ranged) ||
				(data.ISv.Type == TeamScriptableObject.InvulnerableType.Melee && weaponData.Type == TeamScriptableObject.WeaponType.Melee))
				model.SetData(weaponData, data, data.ISv.Value);
			else
				model.SetData(weaponData, data, string.Empty);
			children.Add(model);
		}

		// Sort child by WR
		children.Sort(Sort);
		foreach (var child in children)
			child.transform.SetAsLastSibling();

		// Reset container size
		var prefabHeight = (ModelPrefab.transform as RectTransform).sizeDelta.y;
		Container.sizeDelta = new Vector2(Container.sizeDelta.x, ModelParent.childCount * prefabHeight);

		// Set the correct color scheme
		var teamUI = Database.Instance.RacesScriptableObject.Get(teamData.Race);
		for (int i = 0; i < ImagesUI.Count; i++)
			ImagesUI[i].color = teamUI.FrameColor;
		for (int i = 0; i < TextsUI.Count; i++)
			TextsUI[i].color = teamUI.FrameColor;
	}

	int Sort(AttackedModel model1, AttackedModel model2)
	{
		var model1WR = int.Parse(model1.WR.text.Remove(model1.WR.text.Length - 1, 1));
		var model2WR = int.Parse(model2.WR.text.Remove(model2.WR.text.Length - 1, 1));
		if (model1WR != model2WR)
			return model1WR - model2WR;

		var model1ISv = model1.ISV.text.Length == 0 ? 999 : int.Parse(model1.ISV.text.Remove(model1.ISV.text.Length - 1, 1));
		var model2ISv = model2.ISV.text.Length == 0 ? 999 : int.Parse(model2.ISV.text.Remove(model2.ISV.text.Length - 1, 1));
		if (model1ISv != model2ISv)
			return model2ISv - model1ISv;

		var model1Sv = int.Parse(model1.Sv.text.Remove(model1.Sv.text.Length - 1, 1));
		var model2Sv = int.Parse(model2.Sv.text.Remove(model2.Sv.text.Length - 1, 1));
		if (model1Sv != model2Sv)
			return model2Sv - model1Sv;

		var hasModel1HR = int.TryParse(model1.HR.text.Remove(model1.HR.text.Length - 1, 1), out var model1HR);
		var hasModel2HR = int.TryParse(model2.HR.text.Remove(model2.HR.text.Length - 1, 1), out var model2HR);
		if (!hasModel1HR)
			return -1;
		else if (!hasModel2HR)
			return 1;
		else
			return model1HR - model2HR;
	}

	public void Close()
	{
		gameObject.SetActive(false);
	}
}
