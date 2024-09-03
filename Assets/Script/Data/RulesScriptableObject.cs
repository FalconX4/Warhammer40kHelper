using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RulesData", menuName = "Data/RulesData")]
public class RulesScriptableObject : ScriptableObject
{
	[Serializable]
	public struct RulesData
	{
		public string Name;
		public string Rule;
	}
	public List<RulesData> Rules = new List<RulesData>();

	public bool Contains(string name)
	{
		for (int i = 0; i < Rules.Count; i++)
			if (Rules[i].Name == name)
				return true;
		return false;
	}
}
