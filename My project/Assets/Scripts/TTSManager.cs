using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class TTSManager : MonoBehaviour
{
    private AndroidJavaObject tts;
    private bool isReady = false;

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void SpeakText(string text);
#endif

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            StartCoroutine(InitializeAndroidTTS());
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("iOS TTS is ready!");
        }
    }

    private IEnumerator InitializeAndroidTTS()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TTSInitListener(this));
            yield return new WaitForSeconds(1f);
        }
    }

    public void Speak(string text)
    {
        if (Application.platform == RuntimePlatform.Android && isReady)
        {
            tts.Call<int>("speak", text, 0, null, null);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            SpeakText(text);
#endif
        }
    }

    public void SetTTSReady(bool ready)
    {
        isReady = ready;
    }

    private class TTSInitListener : AndroidJavaProxy
    {
        private TTSManager manager;
        public TTSInitListener(TTSManager manager) : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            this.manager = manager;
        }

        public void onInit(int status)
        {
            if (status == 0)
            {
                manager.SetTTSReady(true);
            }
        }
    }
}