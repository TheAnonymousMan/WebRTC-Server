# Unity WebRTC Streaming and Signaling Server

## Overview

This project implements a fully Unity-based WebRTC solution for streaming camera video, exchanging data messages, and managing WebRTC signaling without requiring external Node.js or browser-based infrastructure. It uses Unity's built-in WebRTC API and the WebSocketSharp library for signaling.

## Features

* Unity camera stream capture using VideoStreamTrack
* Unity-only WebRTC signaling server using WebSocketSharp
* Data channel messaging with buffering support before channel is open
* ICE candidate and SDP exchange
* Compatible with STUN (stun\:stun.l.google.com:19302)
* No external servers required – everything runs inside Unity

## Project Structure

| Script                        | Purpose                                                                       |
| ----------------------------- | ----------------------------------------------------------------------------- |
| `CameraStreamer.cs`           | Captures the Unity camera as a VideoStreamTrack for WebRTC                    |
| `WebRtcServerManager.cs`      | Manages the RTCPeerConnection, handles offers/answers, ICE, and data channels |
| `WebRtcMessage.cs`            | Represents signaling messages (SDP, ICE, or custom data)                      |
| `WebSocketSignalingServer.cs` | Runs a WebSocket server and handles signaling messages inside Unity           |
| `MainThreadDispatcher.cs`     | Ensures Unity coroutines run on the main thread from WebSocketSharp callbacks |
| `SendButtonHandler.cs`        | Triggers sending messages over WebRTC from a Unity input action               |

## How It Works

1. Unity starts a WebSocket server at `ws://<device-ip>:8080/ws`
2. A remote WebRTC peer (client app) connects and sends an SDP offer
3. The Unity app:

   * Sets the remote description
   * Adds a video track from the Unity camera
   * Sends an SDP answer
4. ICE candidates are exchanged
5. A data channel is established for sending/receiving messages
6. Unity can send messages using `SendMessageBuffered()`

## Getting Started

### 1. Install Dependencies

* Unity 2022.3 or later (with Unity.WebRTC installed via Package Manager)
* WebSocketSharp (included in `Assets/Plugins`)

### 2. Unity Project Setup

* Attach `CameraStreamer.cs` to your camera GameObject
* Attach `WebRtcServerManager.cs` to an empty GameObject in your scene
* Attach `WebSocketSignalingServer.cs` to the same or another GameObject
* Optionally add `SendButtonHandler.cs` and wire it to a button or input action

### 3. Run

* Build and run the Unity scene on a device (e.g., VR headset, mobile, or desktop)
* The app will open a WebSocket signaling server at `ws://<device-ip>:8080/ws`
* Connect a WebRTC-capable client to the server and initiate an SDP offer

## Example Message Formats

**WebRTC Offer (from client)**

```json
{
  "type": "offer",
  "sdp": "v=0..."
}
```

**ICE Candidate (from client or Unity)**

```json
{
  "type": "candidate",
  "candidate": "candidate:123...",
  "sdpMid": "0",
  "sdpMLineIndex": 0
}
```

**Answer (from Unity)**

```json
{
  "type": "answer",
  "sdp": "v=0..."
}
```

## Input Handling

* Uses Unity Input System
* `SendButtonHandler.cs` binds a custom `SendMessage` action (e.g., a key or controller button)
* UI Button also calls the same method to trigger message send through the WebRTC data channel

## WebRTC Messaging Format

The signaling server uses a simple JSON format:

```json
{
  "type": "offer" | "answer" | "candidate" | "data",
  "sdp": "...",
  "candidate": "...",
  "sdpMid": "...",
  "sdpMLineIndex": 0,
  "data": "optional string data"
}
```

**WebSocket Endpoint:** `ws://<your-ip>:8080/ws`

Use `IPAddressDisplay.cs` to determine your local IP address in the Unity UI.

## Troubleshooting

* Ensure STUN server (`stun:stun.l.google.com:19302`) is accessible from client and server
* Verify port `8080` is open on your local network
* Use `IPAddressDisplay.cs` to determine server IP for clients on the same LAN
* WebSocket clients must connect to `ws://<ip>:8080/ws`

## Use Cases

* Local-only WebRTC testing
* Streaming Unity camera feed to WebRTC-capable apps (desktop, mobile, VR)
* Prototyping peer-to-peer communication for training or therapy scenarios

## License

MIT or internal usage for research and prototype purposes. Attribution not required but appreciated.

## Author

Developed by Souporno Ghosh as part of a 4-week research and development project.
