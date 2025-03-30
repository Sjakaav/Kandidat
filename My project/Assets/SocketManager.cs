using UnityEngine;
using TMPro;
using SocketIOClient;

public class SocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
    public TMP_InputField messageInput;  // Input field for user messages
    public TMP_Text responseText;        // TextMeshPro UI Text to display AI responses
    private string latestResponse = "";  // Temporary variable for UI update

    void Start()
    {
        // Connect to the Python WebSocket server
        socket = new SocketIOUnity("http://localhost:5000", new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        // Event Listeners
        socket.OnConnected += (sender, e) => Debug.Log("Connected to Python server!");
        
        // Listen for 'message' event (AI response from Python)
        socket.On("message", response =>
        {
            string data = response.GetValue<string>();  // Extract response as string
            Debug.Log("Received from Python: " + data);
            
            latestResponse = data;  // Store response in a variable for Update() to handle
        });

        socket.Connect();
    }

    void Update()
    {
        // If there's a new response, update the UI
        if (!string.IsNullOrEmpty(latestResponse))
        {
            responseText.SetText(latestResponse);  // Update TextMeshPro UI text
            latestResponse = "";  // Clear to prevent unnecessary updates
        }
    }

    // Called when the button is clicked
    public void SendMessageToServer()
    {
        string message = messageInput.text;  // Get text from the input field
        if (!string.IsNullOrEmpty(message))
        {
            socket.Emit("message", message);
            Debug.Log("Sent to Python: " + message);
        }
    }

    private void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}