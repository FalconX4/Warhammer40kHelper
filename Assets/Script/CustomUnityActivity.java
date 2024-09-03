package com.DefaultCompany.Warhammer40kHelper;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;

import java.io.InputStream;
import java.io.BufferedReader;
import java.io.InputStreamReader;

public class CustomUnityActivity extends UnityPlayerActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Any additional setup if needed
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == 1 && resultCode == RESULT_OK && data != null) {
            Uri uri = data.getData();
            // Persist URI permission
            final int takeFlags = data.getFlags()
                    & (Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
            getContentResolver().takePersistableUriPermission(uri, takeFlags);

            // Convert the URI to a string for sending to Unity
            String uriString = uri.toString();
            UnityPlayer.UnitySendMessage("ImportWindow", "OnFileSelected", uriString);
        }
    }

    // Method to convert URI to file path, if necessary
    private String getPathFromURI(Uri uri) {
        // Example implementation to convert URI to a file path, if needed
        // This implementation may vary based on your specific needs
        // and how you handle the file access
        return uri.getPath();
    }
	
	public void readFileContentFromUri(String uriString) {
    try {
        Uri uri = Uri.parse(uriString);
        InputStream inputStream = getContentResolver().openInputStream(uri);
        BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream));
        StringBuilder stringBuilder = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) {
            stringBuilder.append(line).append("\n");
        }
        inputStream.close();

        // Send the content back to Unity
        UnityPlayer.UnitySendMessage("ImportWindow", "OnFileContentRead", stringBuilder.toString());
    } catch (Exception e) {
        e.printStackTrace();
        UnityPlayer.UnitySendMessage("ImportWindow", "OnFileReadError", e.getMessage());
    }
}
}