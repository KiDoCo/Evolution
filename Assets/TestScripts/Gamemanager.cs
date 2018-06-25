using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;


public class Gamemanager : NetworkBehaviour
{
    #pragma warning disable
    public static Gamemanager Instance;
    //File location variables hard coded shishnet
    private string hPFileLocation = "/Assets/StreamingAssets/AssetBundles/carnivore.pl"; //herbivore asset location
    private string cPFileLocation = "/Assets/StreamingAssets/AssetBundles/herbivore.pl"; //carnivore asset location
   
    //Match variables
    [SyncVar]
    private float                       matchTimer;
    private const float                 startingMatchTimer = 5.0f; //time value in minutes
    private const float                 minutesToSeconds   = 60.0f;
    private const float                 interval           = 2.0f;

    //Gamemanager lists
    private Dictionary<int,GameObject>  carnivorePrefabs     = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> herbivorePrefabs     = new Dictionary<int, GameObject>();
    private List<GameObject>            foodsources          = new List<GameObject>();
    public  List<IEatable>              FoodPlaceList        = new List<IEatable>();
    private List<Transform>             FoodSpawnPointList   = new List<Transform>();
    private List<Transform>             PlayerSpawnPointList = new List<Transform>();
    public  List<Test>                  PlayerList           = new List<Test>();
    #pragma warning restore

    public float MatchTimer
    {
        get
        {
            return matchTimer;
        }

        set
        {
            matchTimer = Mathf.Clamp(value, 0, startingMatchTimer);
        }
    }

    /// <summary>
    /// periodically increases the foodamount to every object that implements IEtable interface
    /// </summary>
    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
    }

    /// <summary>
    /// Starts the match between player
    /// </summary>
    public void StartMatch()
    {
        //check the player selection and add them to a list
        for(int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
        //playerselection blah blah 
        //playerlist.add(playerselection[i])
        }

        //Load the match scene
        SceneManager.LoadScene("Game");

        //search every spawnpoint
        for (int i = 0; i < GameObject.FindObjectsOfType<IEatable>().Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag("FoodSource")[i].transform);
        }

        for (int i = 0; i < GameObject.FindGameObjectsWithTag("PlayerSpawns").Length; i++)
        {
            PlayerSpawnPointList.Add(GameObject.FindGameObjectsWithTag("PlayerSpawns")[i].transform);
        }

        //set the match timer and spawn the objects
        MatchTimer = startingMatchTimer * minutesToSeconds;
        Debug.Log("foodspawnlist count" + FoodSpawnPointList.Count);
        Debug.Log("playerspawnlist count" + PlayerSpawnPointList.Count);
        SpawnPlayers();
        SpawnFoodSources();
        //repeaters
        InvokeRepeating("IncreaseFoodOnSources", interval, interval);
        InvokeRepeating("SpawnFish", interval, interval);
    }

    /// <summary>
    /// Ends the match when time ends or one of the sides wins the game
    /// </summary>
    public void EndMatch()
    {
        //stop players movement
        //match result for remaining players
        //insert function to kill server after x seconds 
        //and return remaining players to lobby/menu screen
        CancelInvoke();
    }

    /// <summary>
    /// Ends the match for a single player
    /// </summary>
    public void EndMatchForPlayer()
    {
        //some client magix röh röh
    }

    /// <summary>
    /// Spawns The player to the match map
    /// </summary>
    public void SpawnPlayers()
    {
        GameObject clone = Instantiate(carnivorePrefabs[0], PlayerSpawnPointList[0]);
        for(int i = 0; i < PlayerList.Capacity; i++)
        {
            //list of player and loop it to every individual player
            clone.name = "Player";
        }
    }

    /// <summary>
    /// Spawns fish to the map
    /// </summary>
    public void SpawnFish()
    {
        //Insert function to spawn a fish at a random spawnpoint
    }

    /// <summary>
    /// Populates the food sources
    /// </summary>
    private void SpawnFoodSources()
    {
        for(int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
            GameObject clone = Instantiate(foodsources[0], FoodSpawnPointList[i], false);
            clone.name = "FoodSource " + i;
        }
    }

    /// <summary>
    /// Populates the asset dictionaries
    /// </summary>
    private void LoadAssetToDictionaries()
    {
        List<GameObject> Ctemp = new List<GameObject>();
        List<GameObject> Htemp = new List<GameObject>();
        Ctemp.AddRange(WWW.LoadFromCacheOrDownload("file:///" + (Directory.GetCurrentDirectory() + cPFileLocation).Replace("\\", "/"), 0).assetBundle.LoadAllAssets<GameObject>());
        Htemp.AddRange(WWW.LoadFromCacheOrDownload("file:///" + (Directory.GetCurrentDirectory() + hPFileLocation).Replace("\\", "/"), 0).assetBundle.LoadAllAssets<GameObject>());

        for (int i = 0; i < Ctemp.Count; i++)
        {
            carnivorePrefabs.Add(i ,Ctemp[i]);
        }

        for (int i = 0; i < Htemp.Count; i++)
        {
            herbivorePrefabs.Add(i,Htemp[i]);
        }

        Debug.Log("Carnivores loaded: " + carnivorePrefabs.Count);
        Debug.Log("Herbivores loaded: " + herbivorePrefabs.Count);
    }

    //unity methods

    private void Update()
    {
        if (Input.GetKey(KeyCode.F2))
        {
            EventManager.Broadcast(EVENT.RoundBegin);
        }

        if(MatchTimer <= 0)
        {
            EndMatch();
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadAssetToDictionaries();
        EventManager.ActionAddHandler(EVENT.RoundBegin, StartMatch);
    }
}
