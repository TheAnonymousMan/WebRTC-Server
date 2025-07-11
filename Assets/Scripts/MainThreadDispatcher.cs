using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity-based dispatcher for running coroutines and actions on the main Unity thread.
/// Useful when background threads (e.g., WebSocket callbacks or async events) need to run Unity-specific code like coroutines or GameObject manipulation.
/// </summary>
public class MainThreadDispatcher : MonoBehaviour
{
    // Thread-safe queue that stores actions to be executed on the main thread
    private static readonly Queue<Action> ExecutionQueue = new();

    /// <summary>
    /// Enqueues a coroutine to be executed on the main Unity thread.
    /// This is useful for triggering Unity coroutines from non-main threads (e.g., WebSocket or network threads).
    /// </summary>
    /// <param name="action">The IEnumerator coroutine to run.</param>
    public static void Enqueue(IEnumerator action)
    {
        lock (ExecutionQueue) // Ensure thread-safe access to the queue
        {
            ExecutionQueue.Enqueue(() => Singleton.StartCoroutine(action));
        }
    }

    /// <summary>
    /// Singleton instance of the dispatcher.
    /// Only one dispatcher should exist to ensure reliable access from other classes.
    /// </summary>
    public static MainThreadDispatcher Singleton;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the singleton and ensures the dispatcher persists across scenes.
    /// </summary>
    void Awake()
    {
        if (Singleton == null)
            Singleton = this; // Set this as the singleton instance
        else
        {
            Destroy(gameObject); // Prevent multiple instances
            return;
        }

        // Keep this GameObject alive across scene loads
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Called once per frame. Executes all queued actions on the main thread.
    /// </summary>
    void Update()
    {
        lock (ExecutionQueue) // Thread-safe access to the queue
        {
            while (ExecutionQueue.Count > 0)
            {
                var action = ExecutionQueue.Dequeue();
                action?.Invoke(); // Execute the action safely
            }
        }
    }
}
