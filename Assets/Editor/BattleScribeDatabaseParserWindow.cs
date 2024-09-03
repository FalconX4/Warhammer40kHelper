using UnityEditor;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using static TeamScriptableObject;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

static class cGetEnvVars_WinExp
{
	[DllImport("Shell32.dll")]
	private static extern int SHGetKnownFolderPath(
		[MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken,
		out IntPtr ppszPath);

	[Flags]
	public enum KnownFolderFlags : uint
	{
		SimpleIDList = 0x00000100
		, NotParentRelative = 0x00000200, DefaultPath = 0x00000400, Init = 0x00000800
		, NoAlias = 0x00001000, DontUnexpand = 0x00002000, DontVerify = 0x00004000
		, Create = 0x00008000, NoAppcontainerRedirection = 0x00010000, AliasOnly = 0x80000000
	}
	public static string GetPath(string RegStrName, KnownFolderFlags flags, bool defaultUser)
	{
		IntPtr outPath;
		int result =
			SHGetKnownFolderPath(
				new Guid(RegStrName), (uint)flags, new IntPtr(defaultUser ? -1 : 0), out outPath
			);
		if (result >= 0)
		{
			return Marshal.PtrToStringUni(outPath);
		}
		else
		{
			throw new ExternalException("Unable to retrieve the known folder path. It may not "
				+ "be available on this system.", result);
		}
	}
}

public class BattleScribeDatabaseParserWindow : EditorWindow
{
	string _inlinePath;
	string _summaryPath;
	string _inlineOtherPath;
	string _summaryOtherPath;

	string _downloadPath;

	const string LastInlinePathPref = "DataParserWindowLastInlinePath";
	const string LastSummaryPathPref = "DataParserWindowLastSummaryPath";
	const string LastInlineOtherPathPref = "DataParserWindowLastInlineOtherPath";
	const string LastSummaryOtherPathPref = "DataParserWindowLastSummaryOtherPath";

	const string _stratagemsPath = "Assets/Data/Database/StratagemsData.asset";
	const string _stratagemsDefaultPath = "Assets/Data/Database/StratagemsDefault.txt";
	const string _stratagemsNecronsPath = "Assets/Data/Database/StratagemsNecrons.txt";
	const string _stratagemsAeldariPath = "Assets/Data/Database/StratagemsAeldari.txt";

	RulesScriptableObject _rules;
	TeamScriptableObject _team1;
	TeamScriptableObject _team2;
	StratagemsScriptableObject _stratagems;

	[MenuItem("Database/Parse BattleScribe Files")]
	static public void FindStringInAssets()
	{
		var window = GetWindow(typeof(BattleScribeDatabaseParserWindow), false, "Parse battlescribe data");
		window.minSize = new Vector2(320, 110);
		window.maxSize = new Vector2(320, 110);
	}

	void OnEnable()
	{
		_inlinePath = EditorPrefs.HasKey(LastInlinePathPref) ? EditorPrefs.GetString(LastInlinePathPref) : string.Empty;
		_summaryPath = EditorPrefs.HasKey(LastSummaryPathPref) ? EditorPrefs.GetString(LastSummaryPathPref) : string.Empty;
		_inlineOtherPath = EditorPrefs.HasKey(LastInlineOtherPathPref) ? EditorPrefs.GetString(LastInlineOtherPathPref) : string.Empty;
		_summaryOtherPath = EditorPrefs.HasKey(LastSummaryOtherPathPref) ? EditorPrefs.GetString(LastSummaryOtherPathPref) : string.Empty;
	}

	void OnGUI()
	{
		_downloadPath = cGetEnvVars_WinExp.GetPath("{374DE290-123F-4565-9164-39C4925E467B}", cGetEnvVars_WinExp.KnownFolderFlags.DontVerify, false);

		_inlinePath = CreatePathUI("Inline ", _inlinePath);
		_summaryPath = CreatePathUI("Summary ", _summaryPath);
		_inlineOtherPath = CreatePathUI("Inline  other", _inlineOtherPath);
		_summaryOtherPath = CreatePathUI("Summary other ", _summaryOtherPath);
		if (GUILayout.Button("Parse files"))
		{
			LoadStratagems();
			ParseFiles();
		}
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

	string CreatePathUI(string uiText, string currentPath)
	{
		EditorGUILayout.BeginHorizontal();
		currentPath = EditorGUILayout.TextField(currentPath);
		if (GUILayout.Button(uiText))
			currentPath = EditorUtility.OpenFilePanel($"{uiText} battlescribe file", _downloadPath, "html");
		EditorGUILayout.EndHorizontal();
		return currentPath;
	}

	void ParseFiles()
	{
		SaveLastFileUsed();
		LoadAll();
		var inlineText = File.ReadAllText(_inlinePath).Replace("&#160;", " ").Replace("&#10148;", "\u00BB").Replace("&#9632;", "\u25A0").Replace("&#8216;", "'").Replace("&#8217;", "'");
		var summaryText = File.ReadAllText(_summaryPath).Replace("&#160;", " ").Replace("&#10148;", "\u00BB").Replace("&#9632;", "\u25A0").Replace("&#8216;", "'").Replace("&#8217;", "'");
		var inlineTextOther = File.ReadAllText(_inlineOtherPath).Replace("&#160;", " ").Replace("&#10148;", "\u00BB").Replace("&#9632;", "\u25A0").Replace("&#8216;", "'").Replace("&#8217;", "'");
		var summaryTextOther = File.ReadAllText(_summaryOtherPath).Replace("&#160;", " ").Replace("&#10148;", "\u00BB").Replace("&#9632;", "\u25A0").Replace("&#8216;", "'").Replace("&#8217;", "'");

		var unitSummary = FindText(summaryText, "<th>M</th><th>T</th><th>SV</th><th>W</th><th>LD</th><th>OC</th>", "<div class=\"summary\">", "</div>");
		var unitSummaryOther = FindText(summaryTextOther, "<th>M</th><th>T</th><th>SV</th><th>W</th><th>LD</th><th>OC</th>", "<div class=\"summary\">", "</div>");

		FindDetachment(inlineText, _team1);
		FindDetachment(inlineTextOther, _team2);
		FindUnits(unitSummary, inlineText, _team1, false);
		FindUnits(unitSummaryOther, inlineTextOther, _team2, true);

		_rules.Rules.Clear();
		FindRulesData(summaryText);
		FindRulesData(summaryTextOther);
		EditorUtility.SetDirty(_rules);
		EditorUtility.SetDirty(_team1);
		EditorUtility.SetDirty(_team2);
	}

	void LoadAll() { LoadRules(); LoadTeam1(); LoadTeam2(); }
	void LoadRules() => _rules = AssetDatabase.LoadAssetAtPath("Assets/Data/Matchup/RulesData.asset", typeof(RulesScriptableObject)) as RulesScriptableObject;
	void LoadTeam1() => _team1 = AssetDatabase.LoadAssetAtPath("Assets/Data/Matchup/Team1Data.asset", typeof(TeamScriptableObject)) as TeamScriptableObject;
	void LoadTeam2() => _team2 = AssetDatabase.LoadAssetAtPath("Assets/Data/Matchup/Team2Data.asset", typeof(TeamScriptableObject)) as TeamScriptableObject;

	void SaveLastFileUsed()
	{
		EditorPrefs.SetString(LastInlinePathPref, _inlinePath);
		EditorPrefs.SetString(LastSummaryPathPref, _summaryPath);
		EditorPrefs.SetString(LastInlineOtherPathPref, _inlineOtherPath);
		EditorPrefs.SetString(LastSummaryOtherPathPref, _summaryOtherPath);
	}

	string FindText(string text, string search, string start, string end)
	{
		var index = text.IndexOf(search);
		if (index == -1)
			return string.Empty;

		var firstIndex = text.LastIndexOf(start, index);
		var endIndex = text.IndexOf(end, index + search.Length);
		return text.Substring(firstIndex, endIndex + end.Length - firstIndex);
	}

	string FindText(string text, string search, string start, string end, out int firstIndex, out int endIndex)
	{
		var index = text.IndexOf(search);
		firstIndex = -1;
		endIndex = -1;
		if (index == -1)
			return string.Empty;

		firstIndex = text.LastIndexOf(start, index);
		endIndex = text.IndexOf(end, index + search.Length) + end.Length;
		return text.Substring(firstIndex, endIndex - firstIndex);
	}

	void FindUnits(string unitSummary, string inlineText, TeamScriptableObject team, bool other)
	{
		var teamData = team.TeamData;
		teamData.Clear();

		var unitList = new List<string>();
		var unitListCount = new List<int>();
		var profileNameCheck = "<td class=\"profile-name\">";
		var indexSummary = unitSummary.IndexOf(profileNameCheck);
		while (indexSummary != -1)
		{
			var lastIndex = unitSummary.IndexOf("</td>", indexSummary);
			var unitName = unitSummary.Substring(indexSummary + profileNameCheck.Length, lastIndex - indexSummary - profileNameCheck.Length);
			if (Regex.IsMatch(unitName, @"\d*\.\S*") || Regex.IsMatch(unitName, @"\S*\[\d*\]\S*"))
			{
				var containsBracket = unitName.Contains("[1]");
				var containsDot = unitName.Contains("1.");
				if (containsBracket || containsDot)
				{
					if (containsBracket)
						unitName = unitName.Remove(unitName.IndexOf(" [1]"));
					if (containsDot)
						unitName = unitName.Remove(0, unitName.IndexOf(". ") + ". ".Length);
				}
				else
				{
					indexSummary = unitSummary.IndexOf(profileNameCheck, indexSummary + 1);
					continue;
				}
			}
			if (unitName.Contains(" ("))
				unitName = unitName.Remove(unitName.IndexOf(" ("));

			var indexInline = inlineText.IndexOf("<h4>" + unitName);
			while (indexInline != -1)
			{
				var firstIndex = inlineText.LastIndexOf("<li class=\"rootselection\">", indexInline);
				var endIndex = inlineText.IndexOf("</li>", indexInline);
				var ulIndex = inlineText.IndexOf("<ul>", firstIndex, endIndex - firstIndex);
				while (ulIndex != -1)
				{
					endIndex = inlineText.IndexOf("</li>", inlineText.IndexOf("</ul>", endIndex));
					ulIndex = inlineText.IndexOf("<ul>", ulIndex + 1, endIndex - ulIndex - 1);
				}

				var unitInfo = inlineText.Substring(firstIndex, endIndex + "</li>".Length - firstIndex).Replace("<li", "<div").Replace("</li>", "</div>");
				var data = new UnitData { Model = default, Abilities = new List<AbilityData>(), Categories = new List<string>(), Rules = new List<string>(), Weapons = new List<WeaponData>() };
				var unitTypeCount = FindModelData(unitInfo, unitName, ref data);
				var unitListIndex = unitList.IndexOf(unitInfo);
				if (unitListIndex == -1 || unitListCount[unitListIndex] < unitTypeCount)
				{
					FindAbilities(unitInfo, ref unitSummary, lastIndex, ref data);
					FindCategoriesData(unitInfo, ref data);
					FindRules(unitInfo, ref data);
					FindWeaponsData(unitInfo, unitName, ref data);
					data.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/UnitPicture/{team.Race}/{data.Model.Name}.png");

					teamData.Add(data);
					if (unitListIndex == -1)
					{
						unitList.Add(unitInfo);
						unitListCount.Add(1);
					}
					else
						unitListCount[unitListIndex]++;
				}

				indexInline = inlineText.IndexOf("<h4>" + unitName, endIndex + "</li>".Length);
			}
			indexSummary = unitSummary.IndexOf(profileNameCheck, indexSummary + 1);
		}
	}

	void FindDetachment(string inline, TeamScriptableObject teamScriptableObject)
	{
		var indexRaceStart = inline.IndexOf("<h2>Army Roster ") + "<h2>Army Roster ".Length;
		var indexRaceEnd = inline.IndexOf("</h2>", indexRaceStart);
		teamScriptableObject.Race = inline.Substring(indexRaceStart, indexRaceEnd - indexRaceStart);
		var indexDetachmentStart = inline.IndexOf("<h4>Detachment Choice</h4>", indexRaceEnd);
		if (indexDetachmentStart == -1)
			indexDetachmentStart = inline.IndexOf("<h4>Detachment</h4>", indexRaceEnd);
		indexDetachmentStart = inline.IndexOf("<span class=\"bold\">Selections:</span> ", indexDetachmentStart) + "<span class=\"bold\">Selections:</span> ".Length;
		var indexDetachmentEnd = inline.IndexOf("\n", indexDetachmentStart);
		teamScriptableObject.Detachment = inline.Substring(indexDetachmentStart, indexDetachmentEnd - indexDetachmentStart);
		teamScriptableObject.StratagemsDefaultData = _stratagems.Get("DEFAULT").Get("DEFAULT");
		var stratagemsRace = _stratagems.GetUsingContains(teamScriptableObject.Race);
		teamScriptableObject.StratagemsTeamData = stratagemsRace.Get(teamScriptableObject.Detachment);
		teamScriptableObject.Race = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(stratagemsRace.Name.ToLower());
	}

	void FindAbilities(string unitInfo, ref string unitSummary, int unitSummaryIndex, ref UnitData data)
	{
		var unitInfoLower = unitInfo.ToLower();
		var match = Regex.Match(unitInfoLower, @" a \d+\+ invulnerable save");
		if (match.Success)
		{
			var matchNumber = Regex.Match(unitInfo, @"\d+");
			var saveIndex = unitSummary.IndexOf('+', unitSummaryIndex);
			unitSummary = unitSummary.Insert(saveIndex, "(" + matchNumber.Value + ")");

			data.ISv.Value = matchNumber.Value + "+";
			data.ISv.Type = InvulnerableType.All;
		}

		match = Regex.Match(unitInfoLower, @"have a \d+\+ invulnerable save against ");
		if (match.Success)
		{
			var matchNumberStartIndex = unitInfoLower.IndexOf(" invulnerable save against ") - 9;
			var matchNumber = Regex.Match(unitInfoLower.Substring(matchNumberStartIndex, 9), @"\d+");
			var matchTypeStartIndex = unitInfoLower.IndexOf($"have a {matchNumber.Value}+ invulnerable save against ") + $"have a {matchNumber.Value}+ invulnerable save against ".Length;
			var matchTypeEndIndex = unitInfoLower.IndexOf(".", matchTypeStartIndex);
			var matchType = unitInfoLower.Substring(matchTypeStartIndex, matchTypeEndIndex - matchTypeStartIndex);

			data.ISv.Value = matchNumber.Value + "+";
			data.ISv.Type = matchType == "melee attacks" ? InvulnerableType.Melee : matchType == "ranged attacks" ? InvulnerableType.Ranged : InvulnerableType.Unkown;
			if (data.ISv.Type == InvulnerableType.Unkown)
				Debug.LogError($"{data.Model.Name} Invulnerable type is unknown : {matchType}");

			var isv = matchNumber.Value + (data.ISv.Type != InvulnerableType.All ? "*" : "");
			var saveIndex = unitSummary.IndexOf('+', unitSummaryIndex);
			unitSummary = unitSummary.Insert(saveIndex, "(" + isv + ")");
		}

		var abilitySearch = "<td class=\"profile-name\">";
		var abilitiesInfo = FindText(unitInfo, "<th>Abilities</th>", "<table cellspacing=\"-1\">", "</table>", out int firstAbilitiesInfoIndex, out int endAbilitiesInfoIndex);
		var currentAbilityIndex = abilitiesInfo.IndexOf(abilitySearch);
		while (currentAbilityIndex != -1)
		{
			var endAbilityNameIndex = abilitiesInfo.IndexOf("</td>", currentAbilityIndex);
			var abilityName = abilitiesInfo.Substring(currentAbilityIndex + abilitySearch.Length, endAbilityNameIndex - currentAbilityIndex - abilitySearch.Length);
			var descriptionStartIndex = abilitiesInfo.IndexOf("<td>", endAbilityNameIndex) + "<td>".Length;
			var descriptionEndIndex = abilitiesInfo.IndexOf("</td>", descriptionStartIndex);
			var abilityDescription = abilitiesInfo.Substring(descriptionStartIndex, descriptionEndIndex - descriptionStartIndex);
			abilityDescription = abilityDescription.Replace("<br/>", "\r");
			abilityDescription = abilityDescription.Replace("                                            ", "");
			data.Abilities.Add(new AbilityData { Name = abilityName, Ability = abilityDescription });
			currentAbilityIndex = abilitiesInfo.IndexOf(abilitySearch, endAbilityNameIndex);
		}
	}

	void FindCategoriesData(string unitInfo, ref UnitData data)
	{
		var search = "<span class=\"bold\">Categories:</span> <span class=\"caps\">";
		var startInfoIndex = unitInfo.IndexOf(search) + search.Length;
		var endInfoIndex = unitInfo.IndexOf("</span>", startInfoIndex);
		var subText = unitInfo.Substring(startInfoIndex, endInfoIndex - startInfoIndex);
		data.Categories = new List<string>(subText.Split(',').Select(p => p.Trim()).ToArray());
	}

	int FindModelData(string unitInfo, string unitName, ref UnitData data)
	{
		var startInfoIndex = unitInfo.IndexOf("<th>Unit</th>");
		var unitTypeCount = 0;
		var unitTypeCountIndex = unitInfo.IndexOf("<tr>", startInfoIndex);
		while (unitTypeCountIndex != -1)
		{
			unitTypeCount++;
			unitTypeCountIndex = unitInfo.IndexOf("<tr>", unitTypeCountIndex + "<tr>".Length);
		}

		var index = unitInfo.LastIndexOf("<tr>", unitInfo.IndexOf(unitName, startInfoIndex)) + "<tr>".Length;
		var endIndex = unitInfo.IndexOf("</tr>", index);
		var dataPart = unitInfo.Substring(index, endIndex - index);
		dataPart = dataPart.Replace("\r\n", "");
		dataPart = dataPart.Replace("\t", "");
		dataPart = dataPart.Replace("</td>", ",");
		var splitData = dataPart.Split(',');
		var modelData = new ModelData();
		for (int i = 0; i < splitData.Length; i++)
		{
			var startIndex = splitData[i].IndexOf(">") + 1;
			var value = splitData[i].Remove(0, startIndex);
			if (i == 0)
				modelData.Name = value;
			switch (i)
			{
				case 0: modelData.Name = value; break;
				case 1: modelData.M = value; break;
				case 2: modelData.T = value; break;
				case 3: modelData.Sv = value; break;
				case 4: modelData.W = value; break;
				case 5: modelData.Ld = value; break;
				case 6: modelData.OC = value; break;
				default: break;
			}
		}

		data.Model = modelData;
		return unitTypeCount;
	}

	void FindRules(string unitInfo, ref UnitData data)
	{
		var search = "<span class=\"bold\">Rules:</span> <span class=\"italic\">";
		var startInfoIndex = unitInfo.IndexOf(search) + search.Length;
		var endInfoIndex = unitInfo.IndexOf("</span>", startInfoIndex);
		var rules = unitInfo.Substring(startInfoIndex, endInfoIndex - startInfoIndex);
		data.Rules = new List<string>(rules.Split(',').Select(p => p.Trim()).ToArray());
	}

	void FindRulesData(string summary)
	{
		var rulesStartIndex = summary.IndexOf("<h2>Selection Rules</h2>") + "<h2>Selection Rules</h2>".Length;
		var rulesEndIndex = summary.IndexOf("</div>", rulesStartIndex);
		var rulesText = summary.Substring(rulesStartIndex, rulesEndIndex - rulesStartIndex).Trim();
		var ruleStartIndex = rulesText.IndexOf("<span class=\"bold\">");
		while (ruleStartIndex != -1)
		{
			ruleStartIndex += "<span class=\"bold\">".Length;
			var ruleNameEndIndex = rulesText.IndexOf("</span>", ruleStartIndex);
			var ruleName = rulesText.Substring(ruleStartIndex, ruleNameEndIndex - ruleStartIndex - 1).Trim();
			var ruleDescriptionStartIndex = ruleNameEndIndex + "</span>".Length;
			var ruleDescriptionEndIndex = rulesText.IndexOf("</p>", ruleNameEndIndex);
			var ruleDescription = rulesText.Substring(ruleDescriptionStartIndex, ruleDescriptionEndIndex - ruleDescriptionStartIndex).Trim();
			if (!_rules.Contains(ruleName))
				_rules.Rules.Add(new RulesScriptableObject.RulesData { Name = ruleName, Rule = ruleDescription });
			ruleStartIndex = rulesText.IndexOf("<span class=\"bold\">", ruleDescriptionEndIndex);
		}
	}

	void FindWeaponsData(string unitInfo, string unitName, ref UnitData data)
	{
		data.Weapons.Clear();
		var meleeIndex = unitInfo.IndexOf("<th>Melee Weapons</th>");
		var rangeIndex = unitInfo.IndexOf("<th>Ranged Weapons</th>");
		var hasMelee = meleeIndex != -1;
		var hasRange = rangeIndex != -1;
		var startIndex = hasMelee ? meleeIndex : rangeIndex;
		var endTableIndex = unitInfo.IndexOf("</table>", startIndex);
		FindWeaponsData(unitInfo, startIndex, endTableIndex, unitName, ref data);
		if (hasMelee && hasRange)
		{
			endTableIndex = unitInfo.IndexOf("</table>", rangeIndex);
			FindWeaponsData(unitInfo, rangeIndex, endTableIndex, unitName, ref data);
		}
	}

	void FindWeaponsData(string unitInfo, int index, int endTableIndex, string unitName, ref UnitData data)
	{
		index = unitInfo.IndexOf("<tr>", index);
		var abilitiesIndex = unitInfo.IndexOf("Abilities:");
		var nameIndex = unitInfo.IndexOf(unitName, abilitiesIndex);
		var selectionStartIndex = unitInfo.IndexOf("<span class=\"bold\">Selections:</span> ", nameIndex);
		if (selectionStartIndex == -1)
			selectionStartIndex = unitInfo.IndexOf("<span class=\"bold\">Selections:</span> ");
		selectionStartIndex += "<span class=\"bold\">Selections:</span> ".Length;
		var selectionEndIndex = unitInfo.IndexOf("</p>", selectionStartIndex);
		var selections = unitInfo.Substring(selectionStartIndex, selectionEndIndex - selectionStartIndex).Trim().Split(',');
		var countStartIndex = unitInfo.LastIndexOf("<h4>", selectionStartIndex) + "<h4>".Length;
		var countEndIndex = unitInfo.IndexOf(" ", countStartIndex) - "x".Length;
		var countString = unitInfo.Substring(countStartIndex, countEndIndex - countStartIndex).Trim();
		var hasCount = int.TryParse(countString, out var count);
		if (hasCount)
			data.ModelCount = count;
		else
			data.ModelCount = 1;
		while (index < endTableIndex && index >= 0)
		{
			index += "<tr>".Length;
			var endIndex = unitInfo.IndexOf("</tr>", index);
			var dataPart = unitInfo.Substring(index, endIndex - index);
			dataPart = dataPart.Replace("\r\n", "");
			dataPart = dataPart.Replace("\t", "");
			dataPart = dataPart.Replace("</td>", "?");
			var splitData = dataPart.Split('?');
			var weaponData = new WeaponData();
			for (int i = 0; i < splitData.Length; i++)
			{
				var startIndex = splitData[i].IndexOf(">") + 1;
				var value = splitData[i].Remove(0, startIndex);
				switch (i)
				{
					case 0:
						{
							var aModifier = 0;
							for (int j = 0; j < selections.Length; j++)
							{
								var selection = selections[j].Trim();
								Match match = Regex.Match(selection, $"\\d+x {value}");
								if (match.Success)
								{
									Match matchNumber = Regex.Match(selection, @"\d+");
									aModifier = int.Parse(matchNumber.Value) / (hasCount ? count : 1);
								}
								else
								{
									match = Regex.Match(selection, $"\\d+x ");
									if (match.Success)
										selection = selection.Remove(0, match.Value.Length);
									if (value.Contains(selection))
										aModifier = 1;
								}
							}

							if (aModifier == 0)
								i = splitData.Length;
							else
								weaponData.Name = aModifier > 1 ? $"{aModifier}x {value}" : value;
						}
						break;
					case 1: weaponData.Range = value; break;
					case 2: weaponData.A = value; break;
					case 3: weaponData.Sk = value; break;
					case 4: weaponData.S = value; break;
					case 5: weaponData.AP = value; break;
					case 6: weaponData.D = value; break;
					case 7: value = value.Replace(", ", ","); weaponData.Keywords = new List<string>(value.Split(',')); break;
					default: break;
				}
			}

			if (!string.IsNullOrEmpty(weaponData.Name))
			{
				weaponData.Type = weaponData.Range == "Melee" ? WeaponType.Melee : WeaponType.Ranged;
				data.Weapons.Add(weaponData);
			}
			index = unitInfo.IndexOf("<tr>", index);
		}
	}
}
