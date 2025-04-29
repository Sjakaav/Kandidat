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

        // 🔵 Handle transcription early
        socket.On("transcription_ready", response =>
        {
            try
            {
                Debug.Log("🧪 Received transcription_ready");

                string jsonString = response.GetValue<string>();
                Debug.Log("✅ Raw JSON: " + jsonString);

                JObject json = JObject.Parse(jsonString);
                string transcription = json["transcription"]?.ToString();

                mainThreadActions.Enqueue(() =>
                {
                    transciptionText.text = transcription;
                });

                Debug.Log("📝 Updated transcription text: " + transcription);
            }
            catch (Exception ex)
            {
                Debug.LogError("🚨 Failed to parse transcription_ready: " + ex.Message);
            }
        });

        // 🤖 Handle AI response separately
        socket.On("ai_response", response =>
        {
            try
            {
                Debug.Log("🧪 Received ai_response");

                string jsonString = response.GetValue<string>();
                Debug.Log("✅ Raw JSON: " + jsonString);

                JObject json = JObject.Parse(jsonString);
                string reply = json["response"]?.ToString();

                mainThreadActions.Enqueue(() =>
                {
                    responseText.text = reply;
                });

                Debug.Log("💬 Updated AI reply: " + reply);
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
