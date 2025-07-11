using Newtonsoft.Json;

/// <summary>
/// Represents a WebRTC signaling message used to exchange SDP offers/answers, ICE candidates, or custom data
/// between peers (typically via a signaling server).
/// </summary>
public class WebRtcMessage
{
    /// <summary>
    /// The type of the WebRTC message.
    /// Common values: "offer", "answer", "candidate", or "data".
    /// </summary>
    [JsonProperty("type")]
    public string Type;

    /// <summary>
    /// The Session Description Protocol (SDP) string.
    /// Used in "offer" and "answer" messages for negotiating media capabilities.
    /// </summary>
    [JsonProperty("sdp")]
    public string Sdp;

    /// <summary>
    /// ICE (Interactive Connectivity Establishment) candidate string.
    /// Used when exchanging network candidates to establish a peer-to-peer connection.
    /// </summary>
    [JsonProperty("candidate")]
    public string Candidate;

    /// <summary>
    /// The media stream identification (mid) for the ICE candidate.
    /// Helps map the candidate to the appropriate media section.
    /// </summary>
    [JsonProperty("sdpMid")]
    public string SdpMid;

    /// <summary>
    /// The index of the media line in the SDP that this candidate is associated with.
    /// </summary>
    [JsonProperty("sdpMLineIndex")]
    public int SdpMLineIndex;

    /// <summary>
    /// Optional field for sending arbitrary application-level data (e.g., chat messages).
    /// Used in custom signaling extensions.
    /// </summary>
    [JsonProperty("data")]
    public string Data;
}
