using UnityEngine;
using System;

public class MicManager : MonoBehaviour
{
    [SerializeField] private string fileName = "recordedAudio.wav"; // Optional now
    private SocketManager socketManager;
    public GameObject recordButton;
    public GameObject stopButton;

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void StartMicRecording();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void StopMicRecording();

    private void Start()
    {
        // Find the SocketManager in the scene
        socketManager = FindObjectOfType<SocketManager>();
        if (socketManager == null)
        {
            Debug.LogError("❌ No SocketManager found in scene!");
        }
        stopButton.SetActive(false);
    }

    public void StartRecording()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartMicRecording();
        recordButton.SetActive(false);
        stopButton.SetActive(true);
#endif
    }

    public void StopRecording()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StopMicRecording();
        recordButton.SetActive(true);
        stopButton.SetActive(false);
#endif
    }

    // Called from JavaScript via SendMessage when recording is finished
    public void OnRecordingComplete(string base64Audio)
    {
        Debug.Log("🎤 Recording complete, received base64 audio");

        if (socketManager != null)
        {
            socketManager.SendBase64AudioToServer(base64Audio);
        }
        else
        {
            Debug.LogError("❌ Cannot send audio, SocketManager missing!");
        }
    }
}