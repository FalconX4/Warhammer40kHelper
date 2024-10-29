using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class ImportWindow : MonoBehaviour
{
	public TMP_InputField Team1InputField;
	public TMP_InputField Team2InputField;

	bool _loadTeam1File;
	bool _loadTeam2File;
	string _team1File;
	string _team2File;

	public void OpenDocument()
	{
		if (Application.platform == RuntimePlatform.Android)
			OpenAndroidDocument();
		else
			OpenWindowsDocument();
	}

	public void ReadFileContentFromUri(string uri)
	{
		if (Application.platform == RuntimePlatform.Android)
			ReadFileContentFromAndroidUri(uri);
		else
			ReadFileContentFromWindowsUri(uri);
	}

#region Android
	/*public string GetDownloadsPath()
	{
		string downloadsPath = string.Empty;

		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
		{
			downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\Downloads";
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
			{
				AndroidJavaObject downloadsDir = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", environment.GetStatic<string>("DIRECTORY_DOWNLOADS"));
				downloadsPath = downloadsDir.Call<string>("getAbsolutePath");
			}
		}

		return downloadsPath;
	}
	*/
	public void OpenAndroidDocument()
	{
		if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
			Permission.RequestUserPermission(Permission.ExternalStorageRead);

		using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
		using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
		{
			string actionOpenDocument = intentClass.GetStatic<string>("ACTION_OPEN_DOCUMENT");
			intentObject.Call<AndroidJavaObject>("setAction", actionOpenDocument);
			intentObject.Call<AndroidJavaObject>("addCategory", intentClass.GetStatic<string>("CATEGORY_OPENABLE"));
			intentObject.Call<AndroidJavaObject>("setType", "*/*");

			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			currentActivity.Call("startActivityForResult", intentObject, 1);
		}
	}

	public void ReadFileContentFromAndroidUri(string uri)
	{
		using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			currentActivity.Call("readFileContentFromUri", uri);
		}
	}
#endregion

#region Windows
	public void OpenWindowsDocument()
	{
#if UNITY_EDITOR
		OnFileSelected(UnityEditor.EditorUtility.OpenFilePanel($"Team NewRecruit file", "", "json"));
#else
		Debug.LogError("ImportWindow: Not Implemented");
#endif
	}

	public void ReadFileContentFromWindowsUri(string uri) => OnFileContentRead(File.ReadAllText(uri));
#endregion

	public void OnFileSelected(string uri)
	{
		if (_loadTeam1File)
			Team1InputField.text = uri;
		else if (_loadTeam2File)
			Team2InputField.text = uri;
		_loadTeam1File = false;
		_loadTeam2File = false;
	}

	public void OnFileContentRead(string fileContent)
	{
		if (_loadTeam1File)
		{
			_team1File = fileContent.Replace("$text", "text");
			_loadTeam1File = false;
			_loadTeam2File = true;
			ReadFileContentFromUri(Team2InputField.text);
		}
		else if (_loadTeam2File)
		{
			_team2File = fileContent.Replace("$text", "text");
			_loadTeam2File = false;
			NewRecruitDatabaseParser.ParseFiles(Database.Instance.StratagemsScriptableObject, Database.Instance.RulesScriptableObject, Database.Instance.Team1ScriptableObject, Database.Instance.Team2ScriptableObject, _team1File, _team2File);
			DatabaseSaveLoad.SaveToFile(Database.Instance.RulesScriptableObject, Database.Instance.Team1ScriptableObject, Database.Instance.Team2ScriptableObject);
			UIManager.Instance.ShowTeamWindow();
			Close();
		}
	}

	public void OnFileReadError(string error) { Debug.Log("FileError: " + error); }

	public void Team1Button()
	{
		_loadTeam1File = true;
		OpenDocument();
	}

	public void Team2Button()
	{
		_loadTeam2File = true;
		OpenDocument();
	}

	public void Import()
	{
		_loadTeam1File = true;
		ReadFileContentFromUri(Team1InputField.text);
	}

	public void Close()
	{
		gameObject.SetActive(false);
		UIManager.Instance.ShowTeamWindow();
	}
}
