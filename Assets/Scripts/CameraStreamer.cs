using UnityEngine;
using Unity.WebRTC;
using System.Collections;

/// <summary>
/// This component captures video from a Unity Camera and converts it into a WebRTC-compatible VideoStreamTrack.
/// This track can be sent over a WebRTC peer connection.
/// </summary>
[RequireComponent(typeof(Camera))]  // Ensures the GameObject has a Camera component
public class CameraStreamer : MonoBehaviour
{
    /// <summary>
    /// The WebRTC video stream track generated from the camera feed.
    /// This is used as the video source in WebRTC peer connections.
    /// </summary>
    public VideoStreamTrack VideoTrack { get; private set; }

    /// <summary>
    /// The camera whose view will be streamed.
    /// Can be assigned via Inspector or fetched automatically.
    /// </summary>
    [SerializeField]
    private Camera mainCamera;

    /// <summary>
    /// Called once when the script starts. Ensures camera reference is set and begins video capture.
    /// </summary>
    private void Start()
    {
        // Try to get the Camera component from the same GameObject if not set in Inspector
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        // Initialize and start capturing the camera feed as a WebRTC track
        CreateVideoTrack();
    }

    /// <summary>
    /// Creates a WebRTC VideoStreamTrack from the assigned camera.
    /// This function sets the resolution of the output stream.
    /// </summary>
    private void CreateVideoTrack()
    {
        // Capture the camera's output at 1280x720 resolution and create a VideoStreamTrack from it
        VideoTrack = mainCamera.CaptureStreamTrack(1280, 720);

        Debug.Log("[CameraStreamer] VideoStreamTrack created using CaptureStreamTrack.");
    }

    /// <summary>
    /// Called automatically when the GameObject is destroyed.
    /// Cleans up and releases the video track resources.
    /// </summary>
    private void OnDestroy()
    {
        // If a video track was created, dispose it to free up resources
        if (VideoTrack != null)
        {
            VideoTrack.Dispose();
            VideoTrack = null;
        }
    }
}
