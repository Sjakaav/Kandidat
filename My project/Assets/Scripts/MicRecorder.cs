using UnityEngine;
using System;
using System.IO;

public class MicManager : MonoBehaviour
{
    [SerializeField] private string fileName = "recordedAudio.wav";

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void StartMicRecording();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void StopMicRecording();

    public void StartRecording()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartMicRecording();
#endif
    }

    public void StopRecording()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StopMicRecording();
#endif
    }

    // Called from JavaScript via SendMessage
    public void OnRecordingComplete(string base64Audio)
    {
        byte[] audioBytes = Convert.FromBase64String(base64Audio);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, audioBytes);
        Debug.Log("Audio saved at: " + path);
    }
}
