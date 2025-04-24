using UnityEngine;
using TMPro;
using SocketIOClient;
using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json.Linq;


public class SocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
    public TMP_Text responseText;
    public TMP_Text transciptionText;

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
            try
            {
                Debug.Log("🧪 Received ai_response");
                string jsonString = response.GetValue<string>();
                Debug.Log("✅ Parsed JSON string: " + jsonString);

                JObject json = JObject.Parse(jsonString);

                string reply = json["response"]?.ToString();
                string transcription = json["transcription"]?.ToString();

                Debug.Log("🗣️ Transcription: " + transcription);
                Debug.Log("💬 AI Response: " + reply);

                mainThreadActions.Enqueue(() =>
                {
                    transciptionText.text = transcription;
                    responseText.text = reply;
                });
            }
            catch (Exception ex)
            {
                Debug.LogError("🚨 Failed to parse ai_response: " + ex.Message);
            }
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
