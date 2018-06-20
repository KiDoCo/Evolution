using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//List of events
public enum EVENT {PlayerHit, UpdateExperience, UpdateSize,
                   Eat, Increase, Victory, Defeat, RoundBegin,
                   RoundEnd};

public static class EventManager 
{
    private static Dictionary<EVENT, Action>              actionEventTable = new Dictionary<EVENT, Action>();
    private static Dictionary<EVENT, Action<AudioSource>> soundEventTable  = new Dictionary<EVENT, Action<AudioSource>>();


    /// <summary>
    /// Adds a new action or adds new method to existing entry to dictionary and stores it to event list 
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="action"></param>
    public static void ActionAddHandler(EVENT evnt, Action action)
    {
        if (!actionEventTable.ContainsKey(evnt))
        {
            actionEventTable[evnt] = action;
        }
        else
        {
            actionEventTable[evnt] += action;
        }
    }
    /// <summary>
    /// Adds a new sound action or adds a new method to existing entry to dictionary
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="action"></param>
    public static void SoundAddHandler(EVENT evnt, Action<AudioSource> action)
    {
        if (!soundEventTable.ContainsKey(evnt))
        {
            soundEventTable[evnt] = action;
        }
        else
        {
            soundEventTable[evnt] += action;
        }
    }

    /// <summary>
    /// Calls a action event specified in the parameter
    /// </summary>
    /// <param name="evnt"></param>
    public static void Broadcast(EVENT evnt)
    {
        if(actionEventTable[evnt] != null)
        {
            actionEventTable[evnt]();
        }
    }
    /// <summary>
    /// Calls a sound action event specified in the parameter
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="source"></param>
    public static void SoundBroadcast(EVENT evnt, AudioSource source)
    {
        if(soundEventTable[evnt] != null)
        {
            soundEventTable[evnt](source);
        }
    }

    //List of events and methods in that event



}
