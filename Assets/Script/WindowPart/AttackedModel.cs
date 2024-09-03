using TMPro;
using UnityEngine;

public class AttackedModel : MonoBehaviour
{
	public TextMeshProUGUI Name;
	public TextMeshProUGUI HR;
	public TextMeshProUGUI WR;
	public TextMeshProUGUI Sv;
	public TextMeshProUGUI ISV;
	public TextMeshProUGUI W;
	public TextMeshProUGUI D;

	public void SetData(TeamScriptableObject.WeaponData weaponData, TeamScriptableObject.UnitData data, string ISVData)
	{
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

		Name.text = data.Name;
		HR.text = weaponData.Sk;
		WR.text = wr;
		Sv.text = $"{(data.Model.Sv[0] - '0') - int.Parse(weaponData.AP)}+";
		if (ISV != null)
		{
			ISV.text = ISVData;
			ISV.gameObject.SetActive(true);
		}
		if (W != null)
		{
			W.text = data.Model.W;
			W.gameObject.SetActive(true);
		}
		if (D != null)
		{
			D.text = weaponData.D;
			D.gameObject.SetActive(true);
		}
	}
}
