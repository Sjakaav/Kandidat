using UnityEngine;
using TMPro;
using SocketIOClient;
using System;
using System.Collections.Concurrent;
using System.IO;

public class SocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
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
            Debug.Log("✅ Connected to Python server");
        };

        socket.On("ai_response", response =>
        {
            string reply = response.GetValue<string>();
            Debug.Log("💬 AI Response: " + reply);

            mainThreadActions.Enqueue(() =>
            {
                responseText.text = reply;
            });
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("🔌 Disconnected from server");
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

            socket.Emit("audio_message", base64Audio);
            Debug.Log("📤 Sent audio to server");
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
