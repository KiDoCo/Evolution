using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class Gamemanager : NetworkBehaviour
{ 
    //Ingnore pragmas for unnecessary warnings
#pragma warning disable
    public static Gamemanager           Instance;
    //File location variables hard coded shishnet
    private string                      hPFileLocation = "/Assets/StreamingAssets/AssetBundles/herbivore.pl"; //herbivore asset location
    private string                      cPFileLocation = "/Assets/StreamingAssets/AssetBundles/carnivore.pl"; //carnivore asset location

    //Match variables
    private const float                 startingMatchTimer = 5.0f;  //time value in minutes
    private const float                 minutesToSeconds   = 60.0f; //converting value
    private const float                 interval           = 0.1f;  //The time in seconds that spawning will happen
    private const float                 deathPenaltyTime   = 2.0f;
    private float                       matchTimer;
    private int                         lifeCount;
    private const int                   maxLifeCount = 2;
    private GameObject                  deathCameraPlace;
    
    //Gamemanager lists
    private List<GameObject> carnivorePrefabs               = new List<GameObject>();
    private List<GameObject> herbivorePrefabs               = new List<GameObject>();
    public Dictionary<string, GameObject> PlayerPrefabs     = new Dictionary<string, GameObject>();
    public  List<GameObject> foodsources;
    public  List<IEatable> FoodPlaceList            = new List<IEatable>();
    private List<Transform> FoodSpawnPointList      = new List<Transform>();

    //Strings
    private string gameScene       = "DemoScene";
    private string foodSourceName  = "FoodSource";
    private string playerSpawnName = "player";
    private string menuScene       = "Menu";

    //Prefabs
    public  GameObject CameraPrefab;
    public  GameObject BerryPrefab;

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

    public GameObject DeathCameraPlace
    {
        get
        {
            return deathCameraPlace;
        }
    }

    //Methods

    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
    }

    /// <summary>
    /// Starts the match between players. Must be called after loading the game scene
    /// </summary>
    private IEnumerator StartMatch()
    {
        lifeCount = maxLifeCount; 

        //search every spawnpoint for players and foodsources
        for (int i = 0; i < GameObject.FindGameObjectsWithTag(foodSourceName).Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag(foodSourceName)[i].transform);
        }

        //set the match timer and spawn the objects
        MatchTimer = startingMatchTimer * minutesToSeconds;
        EventManager.Broadcast(EVENT.FoodSpawn);
        EventManager.Broadcast(EVENT.AINodeSpawn);
        FoodSpawnPointList.Clear();
        deathCameraPlace = new GameObject();
        //repeaters for spawning food/populating sources
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
        player.gameObject.SetActive(false);
        yield return new WaitForSeconds(deathPenaltyTime);
        player.gameObject.SetActive(true);
        EventManager.SoundBroadcast(EVENT.PlayMusic, player.GetComponent<AudioSource>(), (int)MusicEvent.Ambient);
        yield return null;
    }

    /// <summary>
    /// Ends the match when time ends or one of the sides wins the game
    /// </summary>
    private void EndMatch()
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

    public void EndMatchForPlayer(Character player)
    {
        //some client magix röh röh

        //Remove player from list and place camera to fixed point of the map
        player.CameraClone.GetComponent<CameraController>().CameraPlaceOnDeath(player);
        player.gameObject.SetActive(false);
    }


    /// Spawns the sources to the environment
    /// </summary>
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

    //unity methods

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
