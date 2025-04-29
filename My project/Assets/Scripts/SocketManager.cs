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

#if UNITY_WEBGL && !UNITY_EDITOR
    private string serverIP = "ws://localhost:5000"; // WebSocket URL for WebGL
#else
    private string serverIP = "http://localhost:5000"; // HTTP URL for Editor and PC builds
#endif

    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    void Start()
    {
        socket = new SocketIOUnity(serverIP, new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("✅ Connected to Python server at " + serverIP);
        };

        // Handle transcription
        socket.On("transcription_ready", response =>
        {
            try
            {
                Debug.Log("🧪 Received transcription_ready");
                string jsonString = response.GetValue<string>();
                JObject json = JObject.Parse(jsonString);
                string transcription = json["transcription"]?.ToString();

                mainThreadActions.Enqueue(() =>
                {
                    transciptionText.text = transcription;
                });

                Debug.Log("📝 Updated transcription: " + transcription);
            }
            catch (Exception ex)
            {
                Debug.LogError("🚨 Error parsing transcription_ready: " + ex.Message);
            }
        });

        // Handle AI response
        socket.On("ai_response", response =>
        {
            try
            {
                Debug.Log("🧪 Received ai_response");
                string jsonString = response.GetValue<string>();
                JObject json = JObject.Parse(jsonString);
                string reply = json["response"]?.ToString();

                mainThreadActions.Enqueue(() =>
                {
                    responseText.text = reply;
                });

                Debug.Log("💬 Updated AI response: " + reply);
            }
            catch (Exception ex)
            {
                Debug.LogError("🚨 Error parsing ai_response: " + ex.Message);
            }
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogWarning("🔌 Disconnected from server!");
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

    public void SendBase64AudioToServer(string base64Audio)
    {
        if (socket == null || !socket.Connected)
        {
            Debug.LogWarning("⚠️ Socket not connected. Cannot send audio.");
            return;
        }

        try
        {
            socket.Emit("audio_message", base64Audio);
            Debug.Log("📤 Sent base64 audio to server.");
        }
        catch (Exception ex)
        {
            Debug.LogError("🚨 Failed to send base64 audio: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        if (socket != null)
        {
            socket.Disconnect();
        }
    }
}
