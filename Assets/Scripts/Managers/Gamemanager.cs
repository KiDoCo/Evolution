using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;


public class Gamemanager : NetworkBehaviour
{ //Ingnore pragmas for unnecessary warnings
#pragma warning disable
    public static Gamemanager           Instance;
    //File location variables hard coded shishnet
    private string                      hPFileLocation = "/Assets/StreamingAssets/AssetBundles/carnivore.pl"; //herbivore asset location
    private string                      cPFileLocation = "/Assets/StreamingAssets/AssetBundles/herbivore.pl"; //carnivore asset location

    //Match variables
    private const float                 startingMatchTimer = 5.0f; //time value in minutes
    private const float                 minutesToSeconds = 60.0f;  //converting value
    private const float                 interval = 2.0f;           //The time in seconds that spawning will happen
    [SyncVar]
    private float                       matchTimer;
    private bool                        MatchStarting;

    //Gamemanager lists
    private Dictionary<int, GameObject> carnivorePrefabs     = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> herbivorePrefabs     = new Dictionary<int, GameObject>();
    public List<GameObject>             foodsources          = new List<GameObject>();
    public List<IEatable>               FoodPlaceList        = new List<IEatable>();
    private List<Transform>             FoodSpawnPointList   = new List<Transform>();
    private List<Transform>             PlayerSpawnPointList = new List<Transform>();
    public List<Test>                   PlayerList           = new List<Test>();
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
    /// periodically increases the foodamount 
    /// </summary>
    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
    }

    /// <summary>
    /// Starts the match between players. Must be called after loading the game scene
    /// </summary>
    public IEnumerator StartMatch()
    {
        if (MatchStarting) yield break;
        MatchStarting = true;

        //check the player selection and add them to a list
        for (int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
            //playerselection blah blah 
            //playerlist.add(playerselection[i])
        }
        //Stop for a moment to scene to load
        yield return new WaitForSeconds(1.0f);

        //search every spawnpoint for players and foodsources
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("FoodSource").Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag("FoodSource")[i].transform);
        }

        for (int i = 0; i < GameObject.FindGameObjectsWithTag("PlayerSpawns").Length; i++)
        {
            PlayerSpawnPointList.Add(GameObject.FindGameObjectsWithTag("PlayerSpawns")[i].transform);
        }

        //set the match timer and spawn the objects
        MatchTimer = startingMatchTimer * minutesToSeconds;
        SpawnPlayers();
        SpawnFoodSources();

        //repeaters for spawning food/populating sources
        InvokeRepeating("IncreaseFoodOnSources", interval, interval);
        InvokeRepeating("SpawnFish", interval, interval);
        MatchStarting = false;
        yield return matchTimer;
    }

    /// <summary>
    /// Ends the match when time ends or one of the sides wins the game
    /// </summary>
    public void EndMatch()
    {
        /*
        stop players movement
        match result for remaining players
        insert function to kill server after x seconds 
        and return remaining players to lobby/menu screen
        */
        CancelInvoke();
        //killserver
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
        for (int i = 0; i < PlayerList.Capacity; i++)
        {
        GameObject clone = Instantiate(carnivorePrefabs[0], PlayerSpawnPointList[0].position, Quaternion.identity);
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

    private void SpawnFoodSources()
    {
        for (int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
            GameObject clone = Instantiate(foodsources[0], FoodSpawnPointList[i].position, Quaternion.identity);
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
            carnivorePrefabs.Add(i, Ctemp[i]);
        }

        for (int i = 0; i < Htemp.Count; i++)
        {
            herbivorePrefabs.Add(i, Htemp[i]);
        }

        Debug.Log("Carnivores loaded: " + carnivorePrefabs.Count);
        Debug.Log("Herbivores loaded: " + herbivorePrefabs.Count);
    }

    public void LoadGame()
    {
        //Load the match scene
        SceneManager.LoadSceneAsync("Game");
        StartCoroutine(StartMatch());
    }

    //unity methods

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            if (Input.GetKey(KeyCode.F2))
            {
                EventManager.Broadcast(EVENT.RoundBegin);
            }
        }

        if (MatchTimer <= 0)
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
        EventManager.ActionAddHandler(EVENT.RoundBegin, LoadGame);
    }
}
