using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Provides a mechanism to safely execute actions on Unity's main thread by maintaining a queue of tasks
/// that need to be run in the Update loop. This is particularly useful for executing code from background threads
/// that interact with Unity objects or require main-thread execution.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    // Internal queue for storing actions to be executed on the main thread.
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    // Singleton instance of the dispatcher.
    private static UnityMainThreadDispatcher _instance = null;

    /// <summary>
    /// Returns the singleton instance of the UnityMainThreadDispatcher.
    /// If none exists, a new GameObject is created, and this component is added to it.
    /// The GameObject is marked to not be destroyed on scene load.
    /// </summary>
    /// <returns>The singleton instance of UnityMainThreadDispatcher.</returns>
    public static UnityMainThreadDispatcher Instance()
    {
        if (!_instance)
        {
            // Create a new GameObject and attach this component.
            var obj = new GameObject("UnityMainThreadDispatcher");
            _instance = obj.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(obj);
        }
        return _instance;
    }

    /// <summary>
    /// Called once per frame. Executes all actions queued for the main thread.
    /// </summary>
    void Update()
    {
        // Ensure thread-safety while accessing the execution queue.
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    /// <summary>
    /// Enqueues an IEnumerator action to be executed as a coroutine on the main thread.
    /// </summary>
    /// <param name="action">The IEnumerator action to start as a coroutine.</param>
    public void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() => { StartCoroutine(action); });
        }
    }

    /// <summary>
    /// Enqueues an Action to be executed on the main thread.
    /// This method wraps the Action into an IEnumerator before enqueuing it.
    /// </summary>
    /// <param name="action">The Action delegate to execute.</param>
    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    /// <summary>
    /// Wraps an Action inside an IEnumerator to allow its execution as a coroutine.
    /// The action is executed immediately and the coroutine yields once.
    /// </summary>
    /// <param name="a">The Action to wrap.</param>
    /// <returns>An IEnumerator that executes the action.</returns>
    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
}
