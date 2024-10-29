using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DatabaseSaveLoad
{
	[Serializable]
	public class DatabaseJSON
	{
		public struct TeamData
		{
			public string Race;
			public string Detachment;
			public List<TeamScriptableObject.UnitData> UnitsData;
			public StratagemsScriptableObject.StratagemDetachmentData StratagemsDefaultData;
			public StratagemsScriptableObject.StratagemDetachmentData StratagemsTeamData;

			public void CopyToScriptableObject(TeamScriptableObject teamScriptableObject)
			{
				teamScriptableObject.Race = Race;
				teamScriptableObject.Detachment = Detachment;
				teamScriptableObject.UnitsData = UnitsData;
				teamScriptableObject.StratagemsDefaultData = StratagemsDefaultData;
				teamScriptableObject.StratagemsTeamData = StratagemsTeamData;
			}

			public void CopyFromScriptableObject(TeamScriptableObject teamScriptableObject)
			{
				Race = teamScriptableObject.Race;
				Detachment = teamScriptableObject.Detachment;
				UnitsData = teamScriptableObject.UnitsData;
				StratagemsDefaultData = teamScriptableObject.StratagemsDefaultData;
				StratagemsTeamData = teamScriptableObject.StratagemsTeamData;
			}
		}

		public TeamData Team1Data;
		public TeamData Team2Data;
		public List<RulesScriptableObject.RulesData> Rules;
	}

	public static void SaveToFile(RulesScriptableObject rules, TeamScriptableObject team1, TeamScriptableObject team2)
	{
		var data = new DatabaseJSON();
		data.Team1Data.CopyFromScriptableObject(team1);
		data.Team2Data.CopyFromScriptableObject(team2);
		data.Rules = rules.Rules;
		string json = JsonConvert.SerializeObject(data, Formatting.Indented);
		string path = Path.Combine(Application.persistentDataPath, "Warhammer40kHelperDatabase.json");
		File.WriteAllText(path, json);
	}

	public static void LoadFromFile(RulesScriptableObject rules, TeamScriptableObject team1, TeamScriptableObject team2)
	{
		string path = Path.Combine(Application.persistentDataPath, "Warhammer40kHelperDatabase.json");
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			var data = JsonConvert.DeserializeObject<DatabaseJSON>(json);
			data.Team1Data.CopyToScriptableObject(team1);
			data.Team2Data.CopyToScriptableObject(team2);
			rules.Rules = data.Rules;
		}
	}
}