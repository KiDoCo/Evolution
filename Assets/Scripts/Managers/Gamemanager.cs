using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class Gamemanager : NetworkBehaviour
{ 
    // Ingnore pragmas for unnecessary warnings
#pragma warning disable
    public static Gamemanager           Instance;

    // File location variables hard coded shishnet
    private string                      hPFileLocation = "/Assets/StreamingAssets/AssetBundles/herbivore.pl"; // Herbivore asset location
    private string                      cPFileLocation = "/Assets/StreamingAssets/AssetBundles/carnivore.pl"; // Carnivore asset location

    // Match variables
    private const float             startingMatchTimer = 5.0f;  // Time value in minutes
    private const float             minutesToSeconds   = 60.0f; // Converting value
    private const float             interval           = 0.1f;  // The time in seconds that spawning will happen
    private const float             deathPenaltyTime   = 2.0f;
    private const int               maxLifeCount       = 2;

    [SyncVar (hook = "changeMatchTimer")]
    private float matchTimer;
    [SyncVar (hook = "changeLifeCount")]
    private int lifeCount;
    
    // Gamemanager lists
    private List<GameObject> carnivorePrefabs               = new List<GameObject>();
    private List<GameObject> herbivorePrefabs               = new List<GameObject>();
    public  Dictionary<string, GameObject> PlayerPrefabs    = new Dictionary<string, GameObject>();
    public  List<GameObject> foodsources;
    public  List<IEatable> FoodPlaceList            = new List<IEatable>();
    private List<Transform> FoodSpawnPointList      = new List<Transform>();

    // Strings
    private string gameScene       = "DemoScene";
    private string foodSourceName  = "FoodSource";
    private string playerSpawnName = "player";
    private string menuScene       = "Menu";

    // Prefabs
    public GameObject CameraPrefab;
    public GameObject BerryPrefab;
#pragma warning restore
    
    //Properties
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



    // -- Match methods

    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
    }

    /// <summary>
    /// Starts the match between players. Must be called after loading the game scene
    /// </summary>
    [ServerCallback]
    private IEnumerator StartMatch()
    {
        lifeCount = maxLifeCount; 

        // Search every spawnpoint for foodsources
        for (int i = 0; i < GameObject.FindGameObjectsWithTag(foodSourceName).Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag(foodSourceName)[i].transform);
        }

        // Set the match timer and spawn the objects
        MatchTimer = startingMatchTimer * minutesToSeconds;
        EventManager.Broadcast(EVENT.FoodSpawn);
        EventManager.Broadcast(EVENT.AINodeSpawn);
        FoodSpawnPointList.Clear();

        // Repeaters for spawning food/populating sources
        InvokeRepeating("IncreaseFoodOnSources", interval, interval);
        yield return matchTimer;
    }

    /// <summary>
    /// Respawns the player 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator Respawn(Character player)
    {
        // Disable player (kill player)
        yield return new WaitForSeconds(deathPenaltyTime);
        // Enable player (respawn)
        // DO THIS IN PLAYER (Character) -> EventManager.SoundBroadcast(EVENT.PlayMusic, player.GetComponent<AudioSource>(), (int)MusicEvent.Ambient);
        yield return null;
    }

    /// <summary>
    /// Ends the match when time ends or one of the sides wins the game
    /// </summary>
    [ServerCallback]
    private void EndMatch()
    {
        CancelInvoke();
        // get stats and stop/end match for in game players
        foreach (Character p in NetworkGameManager.Instance.PlayerList)
        {

        }
        // show match end screen
        // return to the lobby after x secs
    }

    /// <summary>
    /// Ends the match for a single player
    /// </summary>
    public void EndMatchForPlayer(Character player)
    {
        // Kill player
        // Fixed camera in scene. Spectate others?
    }

    /// <summary>
    /// Spawns the sources to the environment
    /// </summary>
    private void SpawnFoodSources()
    {
        // SPAWN IN CLIENTS!

        for (int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
            GameObject clone = Instantiate(foodsources[0], FoodSpawnPointList[i].position, Quaternion.identity);
            clone.name = foodSourceName + i;
        }
        for( int a = 0; a <FoodSpawnPointList.Capacity; a++)
        {
            Destroy(FoodSpawnPointList[a].gameObject);
        }
    }

    /// <summary>
    /// Populates the asset dictionaries
    /// </summary>
    private void LoadAssetToDictionaries()
    {
        //Search the file with WWW class and loads them to cache
        carnivorePrefabs.AddRange(WWW.LoadFromCacheOrDownload("file:///" + (Directory.GetCurrentDirectory() + cPFileLocation).Replace("\\", "/"), 0).assetBundle.LoadAllAssets<GameObject>());
        herbivorePrefabs.AddRange(WWW.LoadFromCacheOrDownload("file:///" + (Directory.GetCurrentDirectory() + hPFileLocation).Replace("\\", "/"), 0).assetBundle.LoadAllAssets<GameObject>());

        foreach (GameObject prefab in carnivorePrefabs)
        {
            PlayerPrefabs.Add(prefab.name, prefab);
        }

        foreach (GameObject prefab in herbivorePrefabs)
        {
            PlayerPrefabs.Add(prefab.name, prefab);
        }

        Debug.Log("Carnivores loaded: " + carnivorePrefabs.Count);
        Debug.Log("Herbivores loaded: " + herbivorePrefabs.Count);
    }

    private void LoadGame() 
    {
        if (isServer)
            StartCoroutine(StartMatch());
    }

    /// <summary>
    /// Checks if the player can be spawned
    /// </summary>
    /// <param name="player"></param>
    public void RespawnPlayer(Character player)
    {
        if (lifeCount > 0)
        {
            lifeCount--;
            StartCoroutine(Respawn(player));
        }
        else
        {
            EndMatchForPlayer(player);
        }
    }

    private void changeLifeCount(int life)
    {
        lifeCount = life;
        // HUD update
        HUDController.instance.CurHealth = life;
    }

    private void changeMatchTimer(float time)
    {
        matchTimer = time;
        // HUD update
    }

    // -- Unity methods

    private void Update()
    {
        if (MatchTimer <= 0 && SceneManager.GetActiveScene().name == "Demoscene")
        {
            EventManager.Broadcast(EVENT.RoundEnd);
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
        EventManager.ActionAddHandler(EVENT.RoundEnd, EndMatch);
        EventManager.ActionAddHandler(EVENT.FoodSpawn, SpawnFoodSources);

        SceneManager.LoadSceneAsync(menuScene);
    }
}
