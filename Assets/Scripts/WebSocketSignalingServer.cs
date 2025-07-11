using System.Collections;
using Newtonsoft.Json;
using Unity.WebRTC;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

/// <summary>
/// Acts as an embedded WebSocket signaling server within Unity.
/// Handles client connections and message broadcasting for WebRTC negotiation.
/// </summary>
public class WebSocketSignalingServer : MonoBehaviour
{
    private WebSocketServer _webSocketServer;

    // Singleton instance for global access
    public static WebSocketSignalingServer Singleton { get; private set; }

    /// <summary>
    /// Ensures only one instance of the signaling server exists.
    /// </summary>
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    /// <summary>
    /// Initializes and starts the WebSocket server on port 8080.
    /// </summary>
    private void Start()
    {
        // Listen on all available network interfaces (0.0.0.0)
        _webSocketServer = new WebSocketServer(System.Net.IPAddress.Any, 8080);

        // Add signaling behavior under "/ws" path
        _webSocketServer.AddWebSocketService<SignalingBehavior>("/ws");

        // Start the server
        _webSocketServer.Start();

        Debug.Log($"WebSocket server started at ws://{System.Net.IPAddress.Broadcast}:8080/ws");
    }

    /// <summary>
    /// Optional raw message handler (not used directly but useful for debugging).
    /// </summary>
    public void HandleRawMessage(string rawMessage)
    {
        Debug.Log($"[WebSocketSignalingServer] Received message: {rawMessage}");
    }

    /// <summary>
    /// Broadcasts a structured WebRTC message (e.g., answer, ICE candidate) to all connected clients.
    /// </summary>
    public void Broadcast(WebRtcMessage message)
    {
        string json = JsonConvert.SerializeObject(message);
        Broadcast(json); // Reuse string version
    }

    /// <summary>
    /// Broadcasts a raw JSON string to all clients connected under "/ws".
    /// </summary>
    public void Broadcast(string message)
    {
        _webSocketServer.WebSocketServices["/ws"].Sessions.Broadcast(message);
    }

    /// <summary>
    /// Gracefully stops the WebSocket server on app quit.
    /// </summary>
    private void OnApplicationQuit()
    {
        _webSocketServer.Stop();
    }
}

/// <summary>
/// WebSocketSharp behavior that handles individual client connections and message routing.
/// </summary>
public class SignalingBehavior : WebSocketBehavior
{
    /// <summary>
    /// Called when a client successfully connects.
    /// </summary>
    protected override void OnOpen()
    {
        Debug.Log("[SignalingBehavior] Client connected.");
    }

    /// <summary>
    /// Called when a client disconnects.
    /// </summary>
    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log("[SignalingBehavior] Client disconnected.");
    }

    /// <summary>
    /// Called when a message is received from a client.
    /// Parses and dispatches it to WebRTC logic.
    /// </summary>
    protected override void OnMessage(MessageEventArgs e)
    {
        // Deserialize the incoming JSON into a WebRtcMessage object
        var msg = JsonConvert.DeserializeObject<WebRtcMessage>(e.Data);

        Debug.Log($"[SignalingBehavior] Received message: {msg.Type}");

        // Handle offer messages (i.e., client wants to initiate connection)
        if (msg.Type == "offer")
        {
            Debug.Log("[SignalingBehavior] Offer received.");
            // Schedule on main Unity thread
            MainThreadDispatcher.Enqueue(WebRtcServerManager.Singleton.OnOfferReceived(msg));
        }
        // Handle ICE candidates sent by client for NAT traversal
        else if (msg.Type == "candidate")
        {
            Debug.Log("[SignalingBehavior] ICE candidate received.");
            WebRtcServerManager.Singleton.OnIceCandidateReceived(msg);
        }
    }
}
