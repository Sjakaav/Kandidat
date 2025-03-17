using UnityEngine;
using System.Collections;
using UnityEngine.Android;
using System.Runtime.InteropServices;

public class STTManager : MonoBehaviour
{
    private AndroidJavaObject speechRecognizer;
    private bool isListening = false;
    public AIChatbot chatbot; // Reference to AI script

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void StartSpeechRecognition();
#endif

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            speechRecognizer = new AndroidJavaObject("android.speech.SpeechRecognizer");
            RequestPermissions(); // Ask for microphone permission when the app starts
        }
    }
    
    public void RequestPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    public void StartListening()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            StartAndroidSpeechRecognition();
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            StartSpeechRecognition();
#endif
        }
        else
        {
            Debug.LogWarning("Speech-to-Text only works on Android and iOS.");
        }
    }

    private void StartAndroidSpeechRecognition()
    {
        if (!isListening)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            speechRecognizer = new AndroidJavaObject("android.speech.SpeechRecognizer", activity);
            speechRecognizer.Call("startListening");

            isListening = true;
        }
    }

    public void OnSpeechResult(string text)
    {
        Debug.Log("Recognized Speech: " + text);
        chatbot.SendToAI(text); // Send recognized text to AI chatbot
    }
}