using TMPro;
using UnityEngine;

public class AttackingModel : MonoBehaviour
{
	public TextMeshProUGUI Name;
	public TextMeshProUGUI HR;
	public TextMeshProUGUI WR;
	public TextMeshProUGUI Sv;
	public TextMeshProUGUI D;

	string _unitName;
	bool _team1;

	public void SetData(TeamScriptableObject.UnitData data, TeamScriptableObject.WeaponData weaponData, string weaponOwnerName, bool team1)
	{
		_unitName = weaponOwnerName;
		_team1 = team1;
		var wr = $"{Database.WoundRoll(int.Parse(weaponData.S), int.Parse(data.Model.T))}+";
		for (int i = 0; i < weaponData.Keywords.Count; i++)
		{
			var anti = "Anti-";
			if (weaponData.Keywords[i].StartsWith(anti))
			{
				var spaceIndex = weaponData.Keywords[i].IndexOf(" ");
				var type = weaponData.Keywords[i].Substring(anti.Length, spaceIndex - anti.Length);
				if (data.Categories.Contains(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(type.ToLower())))
					wr = weaponData.Keywords[i].Substring(spaceIndex + 1);
			}
		}

		Name.text = weaponData.Name;
		HR.text = weaponData.Sk;
		WR.text = wr;
		Sv.text = $"{(data.Model.Sv[0] - '0') - int.Parse(weaponData.AP)}+";
		D.text = weaponData.D;
	}

	public void OnPressed() => UIManager.Instance.ShowExplanationWindow(new Vector2(transform.position.x - ((RectTransform)transform).sizeDelta.x / 2f, transform.position.y), _unitName, _team1);
}
