using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RacesData", menuName = "Data/RacesData")]
public class RacesScriptableObject : ScriptableObject
{
	[Serializable]
	public struct RaceUI
	{
		public string Name;
		public Color FrameColor;
	}

	public List<RaceUI> RaceUIs = new List<RaceUI>();
	public RaceUI Get(string name)
	{
		for (int i = 0; i < RaceUIs.Count; i++)
			if (RaceUIs[i].Name == name)
				return RaceUIs[i];
		return default;
	}
}
