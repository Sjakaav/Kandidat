using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;

public class SocketManager : MonoBehaviour
{
    public TMP_Text responseText;
    public TMP_Text transcriptionText;

    // ─── WebGL JS Bridge ────────────────────────────────────────────────────────
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SocketIO_Connect(string url);

    [DllImport("__Internal")]
    private static extern void SocketIO_Emit(string eventName, string jsonData);
#endif
    // ────────────────────────────────────────────────────────────────────────────

    // Server URL (same for WebGL and standalone)
    private string serverIP = "http://localhost:5000";

    // Desktop fallback client
    private SocketIOUnity socket;

    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    private string pendingBase64Audio = null;
    private bool isSocketReady = false;

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: use JS bridge
        SocketIO_Connect(serverIP);
#else
        // Desktop: use SocketIOUnity C# client
        socket = new SocketIOUnity(serverIP, new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("✅ Connected to Python server at " + serverIP);
            isSocketReady = true;

            if (!string.IsNullOrEmpty(pendingBase64Audio))
            {
                Debug.Log("📤 Sending pending audio...");
                socket.Emit("audio_message", pendingBase64Audio);
                pendingBase64Audio = null;
            }
        };

        socket.OnError += (sender, e) =>
        {
            Debug.LogError("🔥 Socket error: " + e);
        };

        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log("🔁 Attempting to reconnect...");
        };

        socket.On("transcription_ready", response =>
        {
            try
            {
                string jsonString = response.GetValue<string>();
                JObject json = JObject.Parse(jsonString);
                string transcription = json["transcription"]?.ToString();
                mainThreadActions.Enqueue(() => transcriptionText.text = transcription);
                Debug.Log("📝 Transcription: " + transcription);
            }
            catch (Exception ex)
            {
                Debug.LogError("🚨 Error parsing transcription_ready: " + ex.Message);
            }
        });

        socket.On("ai_response", response =>
        {
            try
            {
                string jsonString = response.GetValue<string>();
                JObject json = JObject.Parse(jsonString);
                string reply = json["response"]?.ToString();
                mainThreadActions.Enqueue(() => responseText.text = reply);
                Debug.Log("💬 AI Response: " + reply);
            }
            catch (Exception ex)
            {
                Debug.LogError("🚨 Error parsing ai_response: " + ex.Message);
            }
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogWarning("🔌 Disconnected from server!");
            isSocketReady = false;
        };

        socket.Connect();
#endif
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
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isSocketReady)
        {
            Debug.LogWarning("⚠️ WebGL socket not ready. Queuing audio.");
            pendingBase64Audio = base64Audio;
            return; 
        }
        // Wrap the raw base64 in quotes so SocketIO_Emit → JSON.parse(js) yields a JS string
        string payload = "\"" + base64Audio + "\"";
        SocketIO_Emit("audio_message", payload);
        Debug.Log("📤 WebGL sent audio_message");
#else
        if (socket == null || !isSocketReady)
        {
            Debug.LogWarning("⚠️ Socket not ready. Queuing audio for later.");
            pendingBase64Audio = base64Audio;
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
#endif
    }

    private void OnApplicationQuit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // nothing to clean up on JS socket
#else
        if (socket != null)
        {
            socket.Disconnect();
        }
#endif
    }

    // ─── Callbacks from JS (WebGL) ────────────────────────────────────────────
#if UNITY_WEBGL && !UNITY_EDITOR
    public void onSocketIOConnect()
    {
        Debug.Log("✅ WebGL Socket.IO connected");
        isSocketReady = true;
        if (!string.IsNullOrEmpty(pendingBase64Audio))
        {
            // replay exactly the same wrapping logic
            string payload = "\"" + pendingBase64Audio + "\"";
            SocketIO_Emit("audio_message", payload);
            pendingBase64Audio = null;
            Debug.Log("📤 WebGL sent pending audio_message");
        }
    }

    public void onSocketIODisconnect()
    {
        Debug.LogWarning("🔌 WebGL Socket.IO disconnected");
        isSocketReady = false;
    }

    public void onConnectError(string message)
    {
        Debug.LogError("🔌 WebGL Socket.IO connect_error: " + message);
    }

    public void onTranscription(string transcription)
    {
        try
        {
            // transcription is already the plain text
            mainThreadActions.Enqueue(() => transcriptionText.text = transcription);
            Debug.Log("📝 Transcription: " + transcription);
        }
        catch(Exception e)
        {
            Debug.LogError("Failed to parse transcription JSON: " + e);
        }
    }

    public void onAIResponse(string response)
    {
        try
        {
            mainThreadActions.Enqueue(() => responseText.text = response);
            Debug.Log("💬 AI Response: " + response);
        }
        catch(Exception e)
        {
            Debug.LogError("Failed to parse ai_response JSON: " + e);
        }
    }
#endif
}
