using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static TeamScriptableObject;

public class NewRecruitDatabaseParserWindow : EditorWindow
{
	const string LastTeam1PathPref = "NewRecruitDataParserWindowLastTeam1Path";
	const string LastTeam2PathPref = "NewRecruitDataParserWindowLastTeam2Path";

	const string _stratagemsPath = "Assets/Data/Database/StratagemsData.asset";
	const string _stratagemsDefaultPath = "Assets/Data/Database/StratagemsDefault.txt";
	const string _stratagemsNecronsPath = "Assets/Data/Database/StratagemsNecrons.txt";
	const string _stratagemsAeldariPath = "Assets/Data/Database/StratagemsAeldari.txt";

	string _downloadPath;
	string _team1Path;
	string _team2Path;

	RulesScriptableObject _rules;
	TeamScriptableObject _team1;
	TeamScriptableObject _team2;
	StratagemsScriptableObject _stratagems;

	[MenuItem("Database/Parse NewRecuit Files")]
	static public void FindStringInAssets()
	{
		var window = GetWindow(typeof(NewRecruitDatabaseParserWindow), false, "Parse NewRecruit data");
		window.minSize = new Vector2(320, 110);
		window.maxSize = new Vector2(320, 110);
	}

	void OnEnable()
	{
		_team1Path = EditorPrefs.HasKey(LastTeam1PathPref) ? EditorPrefs.GetString(LastTeam1PathPref) : string.Empty;
		_team2Path = EditorPrefs.HasKey(LastTeam2PathPref) ? EditorPrefs.GetString(LastTeam2PathPref) : string.Empty;
	}

	void OnGUI()
	{
		_downloadPath = cGetEnvVars_WinExp.GetPath("{374DE290-123F-4565-9164-39C4925E467B}", cGetEnvVars_WinExp.KnownFolderFlags.DontVerify, false);

		_team1Path = CreatePathUI("Team1", _team1Path);
		_team2Path = CreatePathUI("Team2", _team2Path);
		if (GUILayout.Button("Parse files"))
		{
			SaveLastFileUsed();
			LoadAll();
			NewRecruitDatabaseParser.ParseFiles(_stratagems, _rules, _team1, _team2, File.ReadAllText(_team1Path).Replace("$text", "text"), File.ReadAllText(_team2Path).Replace("$text", "text"));
		}
	}

	string CreatePathUI(string uiText, string currentPath)
	{
		EditorGUILayout.BeginHorizontal();
		currentPath = EditorGUILayout.TextField(currentPath);
		if (GUILayout.Button(uiText))
			currentPath = EditorUtility.OpenFilePanel($"{uiText} battlescribe file", _downloadPath, "json");
		EditorGUILayout.EndHorizontal();
		return currentPath;
	}

	void LoadStratagems()
	{
		_stratagems = AssetDatabase.LoadAssetAtPath(_stratagemsPath, typeof(StratagemsScriptableObject)) as StratagemsScriptableObject;
		_stratagems.Stratagems.Clear();
		LoadStratagems(_stratagemsDefaultPath, "DEFAULT", 11);
		LoadStratagems(_stratagemsNecronsPath, "NECRONS", 6);
		LoadStratagems(_stratagemsAeldariPath, "AELDARI", 6);
	}

	void LoadStratagems(string path, string race, int maxCount)
	{
		var lines = File.ReadLines(path);
		var isInDetachment = false;
		var isInStratagem = false;
		var detachment = "";
		StratagemsScriptableObject.StratagemData[] stratagems = default;
		var count = 0;
		foreach (var line in lines)
		{
			if (line == "{")
			{
				isInDetachment = true;
				stratagems = new StratagemsScriptableObject.StratagemData[maxCount];
			}
			else if (line == "}")
			{
				isInDetachment = false;
				count = 0;
				var stratagemsRace = _stratagems.Get(race, out var containsRace);
				if (containsRace)
				{
					var stratagemsDetachment = stratagemsRace.Get(detachment, out var containsDetachment);
					if (containsDetachment)
					{
						stratagemsDetachment.StratagemsData.Clear();
						stratagemsDetachment.StratagemsData.AddRange(stratagems);
					}
					else
					{
						var stratagemsDetachmentData = new StratagemsScriptableObject.StratagemDetachmentData { Name = detachment, StratagemsData = new(stratagems) };
						stratagemsRace.StratagemsDetachmentData.Add(stratagemsDetachmentData);
					}
				}
				else
				{
					var stratagemsDetachmentData = new List<StratagemsScriptableObject.StratagemDetachmentData>();
					stratagemsDetachmentData.Add(new StratagemsScriptableObject.StratagemDetachmentData { Name = detachment, StratagemsData = new(stratagems) });
					_stratagems.Stratagems.Add(new StratagemsScriptableObject.StratagemRaceData { Name = race, StratagemsDetachmentData = stratagemsDetachmentData });
				}
			}
			else if (line == "<")
				isInStratagem = true;
			else if (line == ">")
			{
				isInStratagem = false;
				count++;
			}
			else if (!isInDetachment && !isInStratagem)
				detachment = line;
			else if (isInDetachment && !isInStratagem)
				stratagems[count].Name = line;
			else if (line.StartsWith("TYPE: "))
				stratagems[count].Type = (StratagemsScriptableObject.StratagemType)int.Parse(line.Remove(0, "TYPE: ".Length));
			else if (line.StartsWith("ICONS: "))
				stratagems[count].Icons = new List<StratagemsScriptableObject.StratagemIcon>(Array.ConvertAll(line.Remove(0, "ICONS: ".Length).Replace(" ", string.Empty).Split(','), (str) => (StratagemsScriptableObject.StratagemIcon)int.Parse(str)));
			else if (line.StartsWith("CP: "))
				stratagems[count].CP = int.Parse(line.Remove(0, "CP: ".Length));
			else if (line.StartsWith("WHEN: "))
				stratagems[count].When = line.Remove(0, "WHEN: ".Length);
			else if (line.StartsWith("TARGET: "))
				stratagems[count].Target = line.Remove(0, "TARGET: ".Length);
			else if (line.StartsWith("EFFECT: "))
				stratagems[count].Effect = line.Remove(0, "EFFECT: ".Length);
			else if (line.StartsWith("RESTRICTIONS: "))
				stratagems[count].Restrictions = line.Remove(0, "RESTRICTIONS: ".Length);
		}
	}

	void SaveLastFileUsed() { EditorPrefs.SetString(LastTeam1PathPref, _team1Path); EditorPrefs.SetString(LastTeam2PathPref, _team2Path); }
	void LoadAll() { LoadStratagems(); LoadRules(); LoadTeam1(); LoadTeam2(); }
	void LoadRules() => _rules = AssetDatabase.LoadAssetAtPath("Assets/Data/Matchup/RulesData.asset", typeof(RulesScriptableObject)) as RulesScriptableObject;
	void LoadTeam1() => _team1 = AssetDatabase.LoadAssetAtPath("Assets/Data/Matchup/Team1Data.asset", typeof(TeamScriptableObject)) as TeamScriptableObject;
	void LoadTeam2() => _team2 = AssetDatabase.LoadAssetAtPath("Assets/Data/Matchup/Team2Data.asset", typeof(TeamScriptableObject)) as TeamScriptableObject;
}
