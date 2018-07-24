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
    public static Gamemanager Instance;


    // Match variables
    private const float startingMatchTimer = 5.0f;  // Time value in minutes
    private const float minutesToSeconds   = 60.0f; // Converting value
    private const float interval           = 0.1f;  // The time in seconds that spawning will happen
    private const float deathPenaltyTime   = 2.0f;
    private const float experiencePenalty  = 25.0f;

    [SyncVar (hook = "changeMatchTimer")]
    private float matchTimer;
    [SyncVar (hook = "changeLifeCount")]
    private int lifeCount;
    private const int maxLifeCount = 2;
    private GameObject deathCameraPlace;

    // Gamemanager lists
    private List<GameObject> carnivorePrefabs = new List<GameObject>();
    private List<GameObject> herbivorePrefabs = new List<GameObject>();
    public  Dictionary<string, GameObject> PlayerPrefabs = new Dictionary<string, GameObject>();
    public List<GameObject> foodsources;
    public List<GameObject> FoodPlaceList = new List<GameObject>();
    private List<Transform> FoodSpawnPointList = new List<Transform>();

    // Strings
    private string gameScene = "DemoScene";
    private string foodSourceName = "FoodSource";
    private string playerSpawnName = "player";
    private string menuScene = "Menu";

    // Prefabs
    public GameObject BerryPrefab;

#pragma warning restore
    

    #region getters&setters
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


    public List<GameObject> HerbivorePrefabs
    {
        get
        {
            return herbivorePrefabs;
        }

        set
        {
            herbivorePrefabs = value;
        }
    }

    #endregion

    //Methods


    #region match Methods

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
        if (!isServer) yield return null;
        if (SceneManager.GetActiveScene().name != gameScene) yield return null;

        lifeCount = maxLifeCount;

        MatchTimer = startingMatchTimer * minutesToSeconds;

        SpawnFoodSources();

        EventManager.Broadcast(EVENT.AINodeSpawn);
        FoodSpawnPointList.Clear();
        deathCameraPlace = new GameObject();
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
        //search every spawnpoint for foodsources
        for (int i = 0; i < GameObject.FindGameObjectsWithTag(foodSourceName).Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag(foodSourceName)[i].transform);
        }
        for (int i = 0; i < FoodSpawnPointList.Capacity; i++)
        {
            GameObject clone = Instantiate(foodsources[0], FoodSpawnPointList[i].position, Quaternion.identity);
            for (int a = 0; a < clone.transform.GetChild(0).transform.childCount; i++)
            {
                NetworkServer.Spawn(clone.transform.GetChild(0).transform.GetChild(a).gameObject);
            }
            clone.name = foodSourceName + i;
        }
        for (int a = 0; a < FoodSpawnPointList.Capacity; a++)
        {
            Destroy(FoodSpawnPointList[a].gameObject);
        }

    }

    /// <summary>
    /// Checks if the player can be spawned
    /// </summary>
    /// <param name="player"></param>
    public void RespawnPlayer(Herbivore player)
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

    #endregion

    /// <summary>
    /// Populates the asset dictionaries
    /// </summary>
    private void LoadAssetToDictionaries()
    {
        //Search the file with WWW class and loads them to cache
        carnivorePrefabs.AddRange(Resources.LoadAll<GameObject>("Character/Carnivore"));
        herbivorePrefabs.AddRange(Resources.LoadAll<GameObject>("Character/Herbivore"));

        foreach (GameObject prefab in carnivorePrefabs)
        {
            PlayerPrefabs.Add(prefab.name, prefab);
        }

        foreach (GameObject prefab in herbivorePrefabs)
        {
            PlayerPrefabs.Add(prefab.name, prefab);
        }

        Debug.Log("Carnivores loaded: " + carnivorePrefabs.Count);
        Debug.Log("Herbivores loaded: " + HerbivorePrefabs.Count);
    }

    public void LoadGame()
    {
        if (isServer)
            StartCoroutine(StartMatch());
    }

    #region Unity Methods
    private void changeLifeCount(int life)
    {
        lifeCount = life;
        // HUD update
        HUDController.Instance.CurHealth = life;
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
    }
    
    public void Blood()
    {
        GameObject.Find("bloodParticle").GetComponent<ParticleSystem>().Play();
    }

    #endregion
}
