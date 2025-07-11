# Unity WebRTC Server App

This Unity project serves as the WebRTC signaling and media server for the VR therapy application. It listens for incoming WebSocket connections from clients and handles peer connection setup.

## Features
- Embedded WebSocket signaling server
- Unity WebRTC peer connection setup
- Streaming camera feed or other media to clients
- No external signaling server required

## Running
1. Open the project in Unity.
2. Press Play in the Unity Editor (or build and run).
3. The server automatically starts and listens for WebSocket client connections.

## Dependencies
- Unity WebRTC package
- WebSocketSharp (included)
- Newtonsoft.Json (included)

## Notes
- Ensure the firewall or network settings allow WebSocket connections to the listening port.
