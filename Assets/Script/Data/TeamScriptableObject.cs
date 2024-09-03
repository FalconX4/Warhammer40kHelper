using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamData", menuName = "Data/TeamData")]
public class TeamScriptableObject : ScriptableObject
{
	[Serializable]
	public struct AbilityData
	{
		public string Name;
		public string Ability;
	}

	[Serializable]
	public enum WeaponType { Melee, Ranged }

	[Serializable]
	public struct WeaponData
	{
		public string Name;
		public string Range;
		public string A;
		public string Sk;
		public string S;
		public string AP;
		public string D;
		public WeaponType Type;
		public List<string> Keywords;
	}

	[Serializable]
	public struct ModelData
	{
		public string Name;
		public string M;
		public string T;
		public string Sv;
		public string W;
		public string Ld;
		public string OC;
	}

	[Serializable]
	public enum InvulnerableType { All, Unkown, Ranged, Melee }

	[Serializable]
	public struct InvulnerableData
	{
		public string Value;
		public InvulnerableType Type;
	}

	[Serializable]
	public struct UnitData
	{
		public ModelData Model;
		public int ModelCount;
		public int SameUnitCount;
		public InvulnerableData ISv;
		public Sprite Sprite;
		public List<string> Categories;
		public List<string> Rules;
		public List<AbilityData> Abilities;
		public List<WeaponData> Weapons;

		public string Name => Model.Name + (ModelCount > 1 ? "(" + ModelCount + ")" : "") + (SameUnitCount > 1 ? " x " + SameUnitCount : "");

		public bool Equals(UnitData data)
		{
			if (Model.Name != data.Model.Name || ModelCount != data.ModelCount || Abilities.Count != data.Abilities.Count || Weapons.Count != data.Weapons.Count)
				return false;

			for (int i = 0; i < Abilities.Count; i++)
				if (Abilities[i].Name != data.Abilities[i].Name)
					return false;

			for (int i = 0; i < Weapons.Count; i++)
				if (Weapons[i].Name != data.Weapons[i].Name)
					return false;

			return true;
		}
	}

	public string Race = string.Empty;
	public string Detachment = string.Empty;
	public List<UnitData> TeamData = new List<UnitData>();
	public StratagemsScriptableObject.StratagemDetachmentData StratagemsDefaultData;
	public StratagemsScriptableObject.StratagemDetachmentData StratagemsTeamData;

}
