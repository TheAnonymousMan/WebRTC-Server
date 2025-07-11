using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// Manages WebRTC server-side signaling and connection handling for Unity-based video and data communication.
/// Designed to handle incoming offers, send answers, and stream video via CameraStreamer.
/// </summary>
public class WebRtcServerManager : MonoBehaviour
{
    public static WebRtcServerManager Singleton;

    private RTCPeerConnection _remoteConnection;     // Peer connection with the remote client
    private RTCDataChannel _receiveChannel;          // Bi-directional data channel

    private bool _isReceiveChannelReady;             // Indicates whether data channel is ready for sending
    private readonly Queue<string> _queuedMessages = new(); // Messages to send when channel becomes available

    private readonly List<RTCIceCandidate> _pendingCandidates = new(); // ICE candidates received before remote description is set
    private bool _remoteDescSet = false;             // Flag to track if SetRemoteDescription was called

    [SerializeField] private CameraStreamer cameraStreamer; // Reference to the camera streamer component

    private void Awake()
    {
        // Enforce singleton instance
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject); // Keep alive between scenes
    }

    private void Start()
    {
        Connect(); // Initialize WebRTC connection

        // Start coroutine to update WebRTC context every frame
        StartCoroutine(WebRTC.Update());
    }

    /// <summary>
    /// Sets up the peer connection and ICE candidate handling.
    /// </summary>
    private void Connect()
    {
        // Define ICE servers (STUN server for NAT traversal)
        RTCConfiguration configuration = new RTCConfiguration
        {
            iceServers = new[]
            {
                new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
            }
        };

        // Create peer connection and register callbacks
        _remoteConnection = new RTCPeerConnection(ref configuration);
        _remoteConnection.OnDataChannel = ReceiveChannelCallback;

        _remoteConnection.OnIceCandidate = candidate =>
        {
            if (candidate != null)
            {
                // Send ICE candidate to the client
                string json = JsonConvert.SerializeObject(new WebRtcMessage
                {
                    Type = "candidate",
                    Candidate = candidate.Candidate,
                    SdpMid = candidate.SdpMid,
                    SdpMLineIndex = candidate.SdpMLineIndex ?? 0
                });

                WebSocketSignalingServer.Singleton.Broadcast(json);
            }
        };

        _remoteConnection.OnIceConnectionChange = state =>
        {
            Debug.Log("[WebRtcServerManager] ICE State: " + state);
        };
    }

    /// <summary>
    /// Called when an ICE candidate is received from the client.
    /// </summary>
    public void OnIceCandidateReceived(WebRtcMessage msg)
    {
        var candidate = new RTCIceCandidate(new RTCIceCandidateInit
        {
            candidate = msg.Candidate,
            sdpMid = msg.SdpMid,
            sdpMLineIndex = msg.SdpMLineIndex
        });

        // Apply or buffer depending on remote description status
        if (_remoteDescSet)
        {
            _remoteConnection.AddIceCandidate(candidate);
        }
        else
        {
            _pendingCandidates.Add(candidate);
            Debug.Log("[WebRtcServerManager] Candidate buffered.");
        }
    }

    /// <summary>
    /// Called when a WebRTC offer is received from the client.
    /// Begins SDP negotiation.
    /// </summary>
    public IEnumerator OnOfferReceived(WebRtcMessage msg)
    {
        Debug.Log("[WebRtcServerManager] Offer received.");
        var desc = new RTCSessionDescription
        {
            type = RTCSdpType.Offer,
            sdp = msg.Sdp
        };

        yield return HandleOffer(desc);
    }

    /// <summary>
    /// Handles the remote offer, adds local video track, and responds with an answer.
    /// </summary>
    private IEnumerator HandleOffer(RTCSessionDescription desc)
    {
        Debug.Log("[WebRtcServerManager] Setting remote description...");
        var op = _remoteConnection.SetRemoteDescription(ref desc);
        yield return op;

        _remoteDescSet = true;

        // Add any candidates that were buffered before the remote description was set
        foreach (var candidate in _pendingCandidates)
        {
            _remoteConnection.AddIceCandidate(candidate);
        }

        _pendingCandidates.Clear();

        // Inform the peer that we're going to send video
        _remoteConnection.AddTransceiver(TrackKind.Video);

        // Add local video stream from the camera
        if (cameraStreamer != null && cameraStreamer.VideoTrack != null)
        {
            Debug.Log($"[WebRtcServerManager] cameraStreamer: {cameraStreamer != null}, VideoTrack: {cameraStreamer?.VideoTrack}");
            _remoteConnection.AddTrack(cameraStreamer.VideoTrack);
        }
        else
        {
            Debug.LogWarning("[WebRtcServerManager] Video track is null or cameraStreamer not assigned.");
        }

        // Create and set local SDP answer
        var answerOp = _remoteConnection.CreateAnswer();
        yield return answerOp;
        var answer = answerOp.Desc;
        yield return _remoteConnection.SetLocalDescription(ref answer);

        Debug.Log("[WebRtcServerManager] Answer created.");
        Debug.Log($"[WebRtcServerManager] Answer SDP:\n{answer.sdp}");

        // Send the answer back to the client
        WebRtcMessage answerMsg = new WebRtcMessage
        {
            Type = "answer",
            Sdp = answer.sdp
        };

        string json = JsonConvert.SerializeObject(answerMsg);
        WebSocketSignalingServer.Singleton.Broadcast(json);
    }

    /// <summary>
    /// Called when the data channel is opened or closed.
    /// Flushes queued messages if ready.
    /// </summary>
    private void HandleReceiveChannelStatusChange()
    {
        Debug.Log($"[WebRtcServerManager] DataChannel state: {_receiveChannel.ReadyState}");
        _isReceiveChannelReady = _receiveChannel.ReadyState == RTCDataChannelState.Open;

        if (_isReceiveChannelReady)
        {
            // Send all queued messages
            while (_queuedMessages.Count > 0)
            {
                string msg = _queuedMessages.Dequeue();
                _receiveChannel.Send(msg);
                Debug.Log($"[WebRtcServerManager] Flushed message: {msg}");
            }
        }
        else
        {
            Debug.Log("[WebRtcServerManager] Send channel closed or not ready.");
            _isReceiveChannelReady = false;
        }
    }

    /// <summary>
    /// Called when a data channel is received from the client.
    /// Sets up handlers for message, open, and close events.
    /// </summary>
    private void ReceiveChannelCallback(RTCDataChannel channel)
    {
        Debug.Log($"[WebRtcServerManager] Receive channel created: {channel.Label}");
        _receiveChannel = channel;
        _receiveChannel.OnOpen = HandleReceiveChannelStatusChange;
        _receiveChannel.OnClose = HandleReceiveChannelStatusChange;
        _receiveChannel.OnMessage = HandleReceiveMessage;
    }

    /// <summary>
    /// Sends a message through the data channel, or queues it if not yet open.
    /// </summary>
    public void SendMessageBuffered(string message)
    {
        _isReceiveChannelReady = _receiveChannel.ReadyState == RTCDataChannelState.Open;

        if (_isReceiveChannelReady)
        {
            _receiveChannel.Send(message);
            Debug.Log($"[WebRtcServerManager] Sent message: {message}");
        }
        else
        {
            Debug.Log("[WebRtcServerManager] Channel not ready. Queuing message.");
            _queuedMessages.Enqueue(message);
        }
    }

    /// <summary>
    /// Handles receiving a message from the client via the data channel.
    /// </summary>
    private void HandleReceiveMessage(byte[] bytes)
    {
        var message = Encoding.UTF8.GetString(bytes);
        Debug.Log($"[WebRtcServerManager] Message received: {message}");
    }

    /// <summary>
    /// Placeholder for adding local tracks (currently not used).
    /// </summary>
    private void AddLocalVideoTrack(VideoStreamTrack track)
    {
        if (_remoteConnection != null)
        {
            Debug.Log("[WebRtcServerManager] Local video track added to Remote Connection.");
        }
        else
        {
            Debug.LogWarning("[WebRtcServerManager] Remote Connection is null. Cannot add video track.");
        }
    }

    /// <summary>
    /// Cleans up peer connection and data channel on destruction.
    /// </summary>
    private void OnDestroy()
    {
        _receiveChannel?.Close();
        _remoteConnection?.Close();
    }
}
