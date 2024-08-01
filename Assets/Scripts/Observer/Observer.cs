using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Observer : MonoBehaviour
{
    public static Observer Instance;
    private readonly Dictionary<string, Action<object>> eventList = new();

    void Awake()
    {
        //implement singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Each observer object will need to have 1 parameter as object
    public void AddObserver(string eventName, Action<object> eventAction)
    {
        if (eventList.ContainsKey(eventName))
        {
            eventList[eventName] += eventAction;
            return;
        }

        eventList[eventName] = eventAction;
    }

    public void RemoveObserver(string eventName, Action<object> eventAction)
    {
        if (!eventList.ContainsKey(eventName))
        {
            Debug.Log($"Event {eventName} does not subscribed.");
            return;
        }

        eventList[eventName] -= eventAction;
    }

    public void RemoveAllObserver(string eventName)
    {
        if (!eventList.ContainsKey(eventName))
        {
            //Debug.Log($"Event {eventName} does not subscribed.");
            return;
        }

        eventList.Remove(eventName);
    }

    public void RemoveAllObservers()
    {
        eventList.Clear();
    }

    public void TriggerEvent(string eventName, object? eventParams = null)
    {
        if (!eventList.ContainsKey(eventName))
        {
            //Debug.Log($"Event {eventName} does not subscribed.");
            return;
        }
        eventList[eventName]?.Invoke(eventParams);
    }

    public bool IsAddToObserver(string eventName)
    {
        if (eventList.ContainsKey(eventName))
        {
            return true;
        }
        return false;
    }
}

