using SimpleFileBrowser;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class ImportWindow : MonoBehaviour
{
	public StratagemsScriptableObject Stratagems;
	public RulesScriptableObject Rules;
	public TeamScriptableObject Team1;
	public TeamScriptableObject Team2;

	public TMP_InputField Team1InputField;
	public TMP_InputField Team2InputField;

	bool _loadTeam1File;
	bool _loadTeam2File;
	bool _isLoadingFile;
	string _team1File;
	string _team2File;

#region Android
	public string GetDownloadsPath()
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

	public void OpenDocument()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
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
	}

	public void ReadFileContentFromUri(string uri)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				currentActivity.Call("readFileContentFromUri", uri);
			}
		}
		else
			OnFileContentRead(File.ReadAllText(uri));
	}

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
			NewRecruitDatabaseParser.ParseFiles(Stratagems, Rules, Team1, Team2, _team1File, _team2File);
			UIManager.Instance.ShowTeamWindow();
			Close();
		}
	}

	public void OnFileReadError(string error) { }
#endregion

	public void Team1Button()
	{
		if (Application.platform == RuntimePlatform.Android && !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
			Permission.RequestUserPermission(Permission.ExternalStorageRead);

		_loadTeam1File = true;
		OpenDocument();
	}

	public void Team2Button()
	{
		if (Application.platform == RuntimePlatform.Android && !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
			Permission.RequestUserPermission(Permission.ExternalStorageRead);

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
