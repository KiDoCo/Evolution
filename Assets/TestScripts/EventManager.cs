using System;
using System.Collections;
using System.Collections.Generic;


public enum EVENT { AddScore,PlayerHit, UpdateScore};
public static class EventManager 
{
    private static Dictionary<EVENT, Action> eventTable = new Dictionary<EVENT, Action>();

    /// <summary>
    /// Adds a new action to dictionary and stores it to event list 
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="action"></param>
    public static void AddHandler(EVENT evnt, Action action)
    {
        if (!eventTable.ContainsKey(evnt))
        {
            eventTable[evnt] = action;
        }
        else
        {
            eventTable[evnt] += action;
        }
    }

    /// <summary>
    /// Calls the event specified
    /// </summary>
    /// <param name="evnt"></param>
    public static void Broadcast(EVENT evnt)
    {
        if(eventTable[evnt] != null)
        {
            eventTable[evnt]();
        }
    }
}
