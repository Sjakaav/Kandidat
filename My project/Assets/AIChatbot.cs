using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AIChatbot : MonoBehaviour
{
    private string aiEndpoint = "https://YOUR_AI_SERVER_URL"; // Change to your AI API URL
    private string apiKey = "YOUR_API_KEY"; // Change if needed
    public TTSManager tts; // Reference to TTS script

    public void SendToAI(string userText)
    {
        StartCoroutine(SendRequest(userText));
    }

    IEnumerator SendRequest(string message)
    {
        string json = "{\"messages\": [{\"role\": \"user\", \"content\": \"" + message + "\"}]}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(aiEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("AI Response: " + responseText);
            tts.Speak(responseText); // Send AI response to TTS
        }
        else
        {
            Debug.LogError("AI Error: " + request.error);
        }
    }
}