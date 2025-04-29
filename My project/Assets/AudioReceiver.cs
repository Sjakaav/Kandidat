using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioReceiver : MonoBehaviour
{
    private string base64Audio;

    public void OnRecordingComplete(string base64)
    {
        base64Audio = base64;
        Debug.Log("Audio received in Base64 format");

        // Optionally send it right away
        StartCoroutine(SendToServer(base64Audio));
    }

    private IEnumerator SendToServer(string base64)
    {
        string json = JsonUtility.ToJson(new AudioData { audio = base64 });

        using UnityWebRequest www = UnityWebRequest.PostWwwForm("http://your-server-url/your-endpoint", json);
        www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError("Upload failed: " + www.error);
        else
            Debug.Log("Upload succeeded: " + www.downloadHandler.text);
    }

    [System.Serializable]
    private class AudioData
    {
        public string audio;
    }
}
