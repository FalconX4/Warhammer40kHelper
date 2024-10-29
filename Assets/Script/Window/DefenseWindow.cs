using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenseWindow : MonoBehaviour
{
	public AttackingModel ModelPrefab;
	public Transform ModelParent;
	public RectTransform Container;
	public float MaxSize;

	[Header("UI")]
	public List<Image> ImagesUI;
	public List<TextMeshProUGUI> TextsUI;

	public void SetData(TeamScriptableObject.UnitData unitData, bool team1)
	{
		Utility.ClearChilds(ModelParent);

		List<AttackingModel> children = new List<AttackingModel>();
		List<string> weaponsName = new List<string>();
		var teamData = team1 ? Database.Instance.Team2ScriptableObject : Database.Instance.Team1ScriptableObject;
		foreach (var data in teamData.UnitsData)
		{
			foreach (var weapon in data.Weapons)
			{
				if (weaponsName.Contains(weapon.Name))
					continue;

				var model = Instantiate(ModelPrefab, ModelParent);
				model.SetData(unitData, weapon, data.Name, team1);
				children.Add(model);
				weaponsName.Add(weapon.Name);
			}
		}

		// Sort child by WR
		children = children.OrderBy(t => t.WR.text).ToList();
		foreach (var child in children)
			child.transform.SetAsLastSibling();

		// Reset container size
		var prefabHeight = (ModelPrefab.transform as RectTransform).sizeDelta.y;
		Container.sizeDelta = new Vector2(Container.sizeDelta.x, Mathf.Min((ModelParent.childCount + 1) * prefabHeight, MaxSize));

		// Set the correct color scheme
		var teamUI = Database.Instance.RacesScriptableObject.Get(teamData.Race);
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
