using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StratagemsData", menuName = "Data/StratagemsData")]
public class StratagemsScriptableObject : ScriptableObject
{
	public enum StratagemType
	{
		Either,
		Self,
		Opponent
	}

	public enum StratagemIcon
	{
		Any,
		Charge,
		Command,
		CP,
		Fight,
		Movement,
		Shooting
	}

	[Serializable]
	public struct StratagemIconData
	{
		public StratagemIcon Icon;
		public Sprite Sprite;
	}

	[Serializable]
	public struct StratagemUIData
	{
		public StratagemType StratagemType;
		public Color Color;
	}

	[Serializable]
	public struct StratagemData
	{
		public string Name;
		public StratagemType Type;
		public List<StratagemIcon> Icons;
		public int CP;
		public string When;
		public string Target;
		public string Effect;
		public string Restrictions;
	}

	[Serializable]
	public struct StratagemDetachmentData
	{
		public string Name;
		public List<StratagemData> StratagemsData;
	}

	[Serializable]
	public struct StratagemRaceData
	{
		public string Name;
		public List<StratagemDetachmentData> StratagemsDetachmentData;
		public StratagemDetachmentData Get(string detachment)
		{
			for (int i = 0; i < StratagemsDetachmentData.Count; i++)
				if (StratagemsDetachmentData[i].Name == detachment.ToUpper())
					return StratagemsDetachmentData[i];
			return default;
		}
		public StratagemDetachmentData Get(string detachment, out bool contains)
		{
			contains = false;
			for (int i = 0; i < StratagemsDetachmentData.Count; i++)
			{
				if (StratagemsDetachmentData[i].Name == detachment.ToUpper())
				{
					contains = true;
					return StratagemsDetachmentData[i];
				}
			}
			return default;
		}
	}

	public List<StratagemRaceData> Stratagems = new List<StratagemRaceData>();
	public List<StratagemUIData> StratagemsUI = new List<StratagemUIData>();
	public List<StratagemIconData> StratagemIcons = new List<StratagemIconData>();
	public StratagemRaceData GetUsingContains(string race)
	{
		for (int i = 0; i < Stratagems.Count; i++)
			if (race.ToUpper().Contains(Stratagems[i].Name))
				return Stratagems[i];
		return default;
	}
	public StratagemRaceData Get(string race)
	{
		for (int i = 0; i < Stratagems.Count; i++)
			if (Stratagems[i].Name == race.ToUpper())
				return Stratagems[i];
		return default;
	}
	public StratagemRaceData Get(string race, out bool contains)
	{
		contains = false;
		for (int i = 0; i < Stratagems.Count; i++)
		{
			if (Stratagems[i].Name == race.ToUpper())
			{
				contains = true;
				return Stratagems[i];
			}
		}
		return default;
	}
	public StratagemUIData Get(StratagemType type)
	{
		for (int i = 0; i < StratagemsUI.Count; i++)
			if (StratagemsUI[i].StratagemType == type)
				return StratagemsUI[i];
		return default;
	}
	public StratagemIconData Get(StratagemIcon icon)
	{
		for (int i = 0; i < StratagemIcons.Count; i++)
			if (StratagemIcons[i].Icon == icon)
				return StratagemIcons[i];
		return default;
	}
}