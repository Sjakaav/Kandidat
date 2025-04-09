using UnityEngine;
//using SocketIO;  // Import the Socket.IO namespace

public class ChatbotClient : MonoBehaviour
{
    /*
    private SocketIOComponent socket;

    void Start()
    {
        // Connect to the Flask server
        socket = GetComponent<SocketIOComponent>();
        socket.Connect();

        // Listen for messages from the server
        socket.On("message", (data) =>
        {
            Debug.Log("Message from server: " + data);
        });

        // Send a message to the server
        SendMessageToServer("Hello, chatbot!");
    }

    public void SendMessageToServer(string message)
    {
        JSONObject jsonMessage = new JSONObject();
        jsonMessage.Add("message", message);
        socket.Emit("message", jsonMessage);
    }*/
}
