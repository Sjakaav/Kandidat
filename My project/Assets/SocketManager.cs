using UnityEngine;
using TMPro;
using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System;
using System.IO;

public class SocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
    public TMP_InputField messageInput;
    public TMP_Text responseText;
    public AudioSource audioSource;

    private string serverIP = "http://127.0.0.1:5000";

    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    void Start()
    {
        socket = new SocketIOUnity(serverIP, new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected to Python server!");
        };

        // Listen for AI response with audio
        socket.On("audio_response", response =>
        {
            try
            {
                string json = response.ToString();
                Debug.Log("Raw response: " + json);

                string text = response.GetValue().GetProperty("text").GetString();
                string base64Audio = response.GetValue().GetProperty("audio").GetString();

                // Queue the Unity work to run on main thread
                mainThreadActions.Enqueue(() =>
                {
                    responseText.SetText(text);
                    StartCoroutine(PlayAudioFromBase64(base64Audio));
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse response: " + ex);
            }
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from Python server.");
        };

        socket.Connect();
    }

    void Update()
    {
        // Handle any queued main thread work
        while (mainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    public void SendMessageToServer()
    {
        string message = messageInput.text;
        if (!string.IsNullOrEmpty(message))
        {
            socket.Emit("message", message);
            Debug.Log("Sent to Python: " + message);
        }
    }

    private IEnumerator PlayAudioFromBase64(string base64Audio)
    {
        byte[] audioBytes = Convert.FromBase64String(base64Audio);

        // Load WAV audio clip from bytes
        using (var memoryStream = new MemoryStream(audioBytes))
        {
            var www = new WWW("data:audio/wav;base64," + Convert.ToBase64String(audioBytes));
            yield return www;

            AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}
