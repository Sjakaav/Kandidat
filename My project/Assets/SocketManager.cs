using UnityEngine;
using TMPro;
using SocketIOClient;

public class SocketManager : MonoBehaviour
{
    private SocketIOUnity socket;
    public TMP_InputField messageInput;  // Input field for user messages
    public TMP_Text responseText;        // TextMeshPro UI Text to display AI responses

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
            string data = response.GetValue<string>();  // Extract string response
            Debug.Log("Received from Python: " + data);
            
            // Force update TextMeshPro UI
            responseText.SetText(data);
        });

        socket.Connect();
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