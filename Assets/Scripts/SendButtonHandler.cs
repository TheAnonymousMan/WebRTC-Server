using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the "Send Message" input action, either via button press or keyboard input,
/// and triggers a method that sends a WebRTC message.
/// </summary>
public class SendButtonHandler : MonoBehaviour
{
    // Reference to the InputActionAsset created in Unity's Input System (usually set via Inspector)
    [SerializeField] private InputActionAsset inputActions;

    // The specific input action for sending messages (e.g., button press, key)
    private InputAction sendMessageAction;

    /// <summary>
    /// Called when the script is enabled.
    /// Sets up the input action and subscribes to its "performed" event.
    /// </summary>
    private void OnEnable()
    {
        // Find the "Messenger" action map and get the "SendMessage" action from it
        var actionMap = inputActions.FindActionMap("Messenger", true);
        sendMessageAction = actionMap.FindAction("SendMessage", true);

        // Subscribe to the input event (button/key press)
        sendMessageAction.performed += OnSendMessage;

        // Enable the action so it starts listening
        sendMessageAction.Enable();
    }
    
    /// <summary>
    /// Called when the script is disabled.
    /// Unsubscribes from the input action and disables it to avoid leaks.
    /// </summary>
    private void OnDisable()
    {
        if (sendMessageAction != null)
        {
            // Unsubscribe from the event and disable the action
            sendMessageAction.performed -= OnSendMessage;
            sendMessageAction.Disable();
        }
    }

    /// <summary>
    /// Callback that gets invoked when the input action is triggered.
    /// </summary>
    private void OnSendMessage(InputAction.CallbackContext context)
    {
        SendMessageButtonClicked(); // Call the actual logic
    }

    /// <summary>
    /// This method contains the logic for sending the message.
    /// It can also be called directly from a UI Button.
    /// </summary>
    public void SendMessageButtonClicked()
    {
        Debug.Log("Send message button clicked");

        // Send a test message using the WebRTC server manager
        WebRtcServerManager.Singleton.SendMessageBuffered("Test Message Sent from Server!");
    }
}
