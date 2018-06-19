using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Gamemanager : NetworkBehaviour
{
    public static Gamemanager Instance;

    //Match variables
    [SyncVar]
    private float          matchTimer;
    private const float    StartingMatchTimer = 5.0f; //time value in minutes
    private const float    MinutesToSeconds = 60.0f;  
    //gamemanager lists
    public List<IEatable>  FoodPlaceDictionary = new List<IEatable>();
    public List<Transform> SpawnPointList;
    public List<Transform> PlayerSpawnPointList;
    public List<Test>      PlayerList          = new List<Test>();

    public float MatchTimer
    {
        get
        {
            return matchTimer;
        }

        set
        {
            matchTimer = Mathf.Clamp(value, 0, StartingMatchTimer);
        }
    }

    /// <summary>
    /// periodically increases the foodamount to every object that implements IEtable interface
    /// </summary>
    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
    }

    public void StartMatch()
    {
        MatchTimer = StartingMatchTimer * MinutesToSeconds;
        //event call to start a match
    }

    public void EndMatch()
    {
        //insert function to kill server after x seconds 
        //and return remaining players to lobby/menu screen
    }

    public void SpawnPlayers()
    {
        //list of player and loop it to every individual player
    }

    public void SpawnFish()
    {
        //Insert function to spawn a fish at a random spawnpoint
    }

    private void Update()
    {
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InvokeRepeating("IncreaseFoodOnSources", 2.0f, 2.0f);
    }
}
