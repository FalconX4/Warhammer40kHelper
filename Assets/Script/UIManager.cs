using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public TeamWindow TeamWindow;
	public UnitWindow UnitWindow;
	public StratagemsWindow StratagemsWindow;
	public WeaponAttackWindow WeaponAttackWindow;
	public DefenseWindow DefenseWindow;
	public ExplanationWindow ExplanationWindow;
	public ImportWindow ImportWindow;

	public static UIManager Instance;
	private void Awake()
	{
		Instance = this;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToLandscapeLeft= false;
		Screen.autorotateToLandscapeRight = false;
		Screen.orientation = ScreenOrientation.LandscapeLeft | ScreenOrientation.LandscapeRight;
	}
#if UNITY_STANDALONE_WIN
	public void ParseFiles()
	{
		string downloadsPath = "/storage/emulated/0/Download/";
	}
#endif

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			if (ExplanationWindow.gameObject.activeInHierarchy)
				ExplanationWindow.Close();
			else
				Back();
		}
	}

	public void Back()
	{
		ShowTeamWindow();
	}

	public void RewrittenDatabase()
	{
		Database.Instance.ShowRewrittenText = !Database.Instance.ShowRewrittenText;
		if (UnitWindow.gameObject.activeInHierarchy)
			UnitWindow.Refresh();
	}

	public void ShowTeamWindow()
	{
		UnitWindow.gameObject.SetActive(false);
		StratagemsWindow.gameObject.SetActive(false);
		TeamWindow.gameObject.SetActive(true);
	}

	public void ShowUnitWindow(List<TeamScriptableObject.UnitData> unitDatas, int index, bool team1)
	{
		UnitWindow.gameObject.SetActive(true);
		TeamWindow.gameObject.SetActive(false);
		UnitWindow.SetData(unitDatas, index, team1);
	}

	public void ShowStratagemsWindow(TeamScriptableObject team)
	{
		StratagemsWindow.gameObject.SetActive(true);
		TeamWindow.gameObject.SetActive(false);
		StratagemsWindow.SetData(team);
	}

	public void ShowWeaponAttackWindow(TeamScriptableObject.WeaponData weaponData, bool team1)
	{
		WeaponAttackWindow.gameObject.SetActive(true);
		WeaponAttackWindow.SetData(weaponData, team1);
	}

	public void ShowDefenseWindow(TeamScriptableObject.UnitData unitData, bool team1)
	{
		DefenseWindow.gameObject.SetActive(true);
		DefenseWindow.SetData(unitData, team1);
	}

	public void ShowExplanationWindow(Vector2 position, string explain, bool team1)
	{
		ExplanationWindow.gameObject.SetActive(true);
		ExplanationWindow.transform.position = position;
		ExplanationWindow.SetData(explain, team1);
	}

	public void ShowImportWindow()
	{
		UnitWindow.gameObject.SetActive(false);
		StratagemsWindow.gameObject.SetActive(false);
		TeamWindow.gameObject.SetActive(false);
		ImportWindow.gameObject.SetActive(true);
	}
}
