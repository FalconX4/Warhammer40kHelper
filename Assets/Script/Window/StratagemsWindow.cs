using UnityEngine;

public class StratagemsWindow : MonoBehaviour
{
	public Stratagem StratagemPrefab;
	public Transform StratagemParent;
	public RectTransform StratagemContainer;
	public RectTransform StratagemScrollRect;

	public void SetData(TeamScriptableObject team)
	{
		Utility.ClearChilds(StratagemParent);

		for (int i = 0; i < team.StratagemsTeamData.StratagemsData.Count; i++)
		{
			var stratagem = Instantiate(StratagemPrefab, StratagemParent);
			stratagem.SetData(team.StratagemsTeamData.StratagemsData[i]);
		}

		for (int i = 0; i < team.StratagemsDefaultData.StratagemsData.Count; i++)
		{
			var stratagem = Instantiate(StratagemPrefab, StratagemParent);
			stratagem.SetData(team.StratagemsDefaultData.StratagemsData[i]);
		}

		var prefabWidth = (StratagemPrefab.transform as RectTransform).sizeDelta.x;
		StratagemContainer.sizeDelta = new Vector2(StratagemParent.childCount * prefabWidth - StratagemScrollRect.rect.width, StratagemContainer.sizeDelta.y);
	}
}
