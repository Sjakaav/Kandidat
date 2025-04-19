using UnityEngine;
using TMPro;
using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;

public class SocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
    public TMP_InputField messageInput;
    public TMP_Text responseText;

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
            Debug.Log("✅ Connected to Python server!");
        };

        socket.On("transcribed_text", response =>
        {
            string transcribedText = response.GetValue<string>();
            Debug.Log("📝 Received transcribed text: " + transcribedText);

            mainThreadActions.Enqueue(() =>
            {
                messageInput.text = transcribedText;
                SendMessageToServer(); // Optional: auto-send after transcription
            });
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("❌ Disconnected from Python server.");
        };

        socket.Connect();
    }

    void Update()
    {
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
            Debug.Log("📤 Sent to Python: " + message);
        }
    }

    public void SendAudioToServer()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "recording.wav");

        if (!File.Exists(filePath))
        {
            Debug.LogError("❌ Audio file not found at: " + filePath);
            return;
        }

        try
        {
            byte[] audioBytes = File.ReadAllBytes(filePath);
            string base64Audio = Convert.ToBase64String(audioBytes);

            var payload = new
            {
                audio = base64Audio
            };

            socket.Emit("audio_message", payload);
            Debug.Log("🎙️ Sent audio to server for transcription");
        }
        catch (Exception ex)
        {
            Debug.LogError("🚨 Failed to send audio: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}
