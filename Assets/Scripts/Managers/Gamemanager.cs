using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;


public class Gamemanager : NetworkBehaviour
{ 
    //Ingnore pragmas for unnecessary warnings
#pragma warning disable
    public static Gamemanager           Instance;
    //File location variables hard coded shishnet
    private string                      hPFileLocation = "/Assets/StreamingAssets/AssetBundles/herbivore.pl"; //herbivore asset location
    private string                      cPFileLocation = "/Assets/StreamingAssets/AssetBundles/carnivore.pl"; //carnivore asset location
    public GameObject                   CameraPrefab;
    private GameObject                  MusicPlaySource;
    //Match variables
    private const float                 startingMatchTimer = 5.0f; //time value in minutes
    private const float                 minutesToSeconds   = 60.0f;//converting value
    private const float                 interval           = 0.1f;//The time in seconds that spawning will happen
    private float                       matchTimer;
    private bool                        MatchStarting;
    private bool                        gameon; //debug variable
    //Gamemanager lists
    private Dictionary<int, GameObject> carnivorePrefabs     = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> herbivorePrefabs     = new Dictionary<int, GameObject>();
    public  List<GameObject>            foodsources          = new List<GameObject>();
    public  List<IEatable>              FoodPlaceList        = new List<IEatable>();
    private List<Transform>             FoodSpawnPointList   = new List<Transform>();
    private List<Transform>             PlayerSpawnPointList = new List<Transform>();
    public  List<move>                  PlayerList           = new List<move>();

    //Strings
    private string gameScene       = "DemoScene";
    private string foodSourceName  = "FoodSource";
    private string playerSpawnName = "player";
    private string MusicSource = "MusicSource";
#pragma warning restore

    public float MatchTimer
    {
        get
        {
            return matchTimer;
        }

        set
        {
            matchTimer = value;
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
        yield return new WaitForSeconds(2.0f);

        //search every spawnpoint for players and foodsources
        for (int i = 0; i < GameObject.FindGameObjectsWithTag(foodSourceName).Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag(foodSourceName)[i].transform);
        }

        for (int i = 0; i < GameObject.FindGameObjectsWithTag(playerSpawnName).Length; i++)
        {
            PlayerSpawnPointList.Add(GameObject.FindGameObjectsWithTag(playerSpawnName)[i].transform);
        }
        yield return new WaitForSeconds(1.0f);
        //set the match timer and spawn the objects
        MatchTimer = startingMatchTimer * minutesToSeconds;
        SpawnPlayers();
        SpawnFoodSources();
        EventManager.Broadcast(EVENT.DoAction);
        FoodSpawnPointList.Clear();
        //repeaters for spawning food/populating sources
        InvokeRepeating("IncreaseFoodOnSources", interval, interval);
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
    private void SpawnPlayers()
    {
        Debug.Log("Spawning player");
            //list of player and loop it to every individual player
        GameObject clone = Instantiate(herbivorePrefabs[0], PlayerSpawnPointList[0].position, Quaternion.identity);
            clone.name = "Player";
    }


    private void SpawnFoodSources()
    {
        for (int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
            GameObject clone = Instantiate(foodsources[0], FoodSpawnPointList[i].position, Quaternion.identity);
            clone.name = foodSourceName + i;
        }
        for( int a = 0; a <FoodSpawnPointList.Capacity; a++)
        {
            Destroy(FoodSpawnPointList[a].gameObject);
        }

        for (int i = 0; i < FoodPlaceList.ToArray().Length; i++)
        {
            Debug.Log("Object in list: " + FoodPlaceList.ToArray()[i]);
        }
    }

    /// <summary>
    /// Populates the asset dictionaries
    /// </summary>
    private void LoadAssetToDictionaries()
    {
        List<GameObject> Ctemp = new List<GameObject>();
        List<GameObject> Htemp = new List<GameObject>();
        //Search the file with WWW class and loads them to cache
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
        Ctemp.Clear();
        Htemp.Clear();
            
        Debug.Log("Carnivores loaded: " + carnivorePrefabs.Count);
        Debug.Log("Herbivores loaded: " + herbivorePrefabs.Count);
    }

    public void LoadGame()
    {
        //Load the match scene
        SceneManager.LoadSceneAsync(gameScene);
        StartCoroutine(StartMatch());
    }

    //unity methods

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != gameScene)
        {
            if(!gameon)
            {
                if (Input.GetKeyDown(KeyCode.F2))

                {
                    gameon = true;
                    EventManager.Broadcast(EVENT.RoundBegin);
                }
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
        EventManager.ActionAddHandler(EVENT.Spawn, SpawnFoodSources);
    }
}
