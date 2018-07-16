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

    //Match variables
    private const float startingMatchTimer = 5.0f; //time value in minutes
    private const float minutesToSeconds = 60.0f;//converting value
    private const float interval = 0.1f; //The time in seconds that spawning will happen
    private const float deathPenaltyTime = 2.0f;
    private const float experiencePenalty = 25.0f;
    private float       matchTimer;
    private int         lifeCount;
    private const int   maxLifeCount = 2;
    private GameObject  deathCameraPlace;

    //Gamemanager lists
    private List<GameObject> carnivorePrefabs = new List<GameObject>();
    private List<GameObject> herbivorePrefabs = new List<GameObject>();
    public List<GameObject>  foodsources;
    public List<IEatable>    FoodPlaceList = new List<IEatable>();
    private List<Transform>  FoodSpawnPointList = new List<Transform>();

    //Strings
    private string gameScene = "DemoScene";
    private string foodSourceName = "FoodSource";
    private string playerSpawnName = "player";
    private string menuScene = "Menu";

    //Prefabs
    public GameObject CameraPrefab;
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

    public GameObject DeathCameraPlace
    {
        get
        {
            return deathCameraPlace;
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
    private IEnumerator StartMatch()
    {
        yield return new WaitForSeconds(2.0f);
        lifeCount = maxLifeCount;
        //Stop for a moment to scene to load
        yield return new WaitForSeconds(2.0f);

        //search every spawnpoint for players and foodsources
        for (int i = 0; i < GameObject.FindGameObjectsWithTag(foodSourceName).Length; i++)
        {
            FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag(foodSourceName)[i].transform);
        }


        yield return new WaitForSeconds(1.0f);
        //set the match timer and spawn the objects
        MatchTimer = startingMatchTimer * minutesToSeconds;

        SpawnFoodSources();
        EventManager.Broadcast(EVENT.DoAction);
        FoodSpawnPointList.Clear();
        deathCameraPlace = new GameObject();
        GameObject clone = Instantiate(herbivorePrefabs[0], GameObject.Find("TempLocation").transform.position, Quaternion.identity);
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
        player.Experience -= experiencePenalty;
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
        EventManager.Broadcast(EVENT.RoundEnd);
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
        for (int a = 0; a < FoodSpawnPointList.Capacity; a++)
        {
            Destroy(FoodSpawnPointList[a].gameObject);
        }
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

    #endregion

    /// <summary>
    /// Populates the asset dictionaries
    /// </summary>
    private void LoadAssetToDictionaries()
    {
        List<GameObject> Ctemp = new List<GameObject>();
        List<GameObject> Htemp = new List<GameObject>();

        Ctemp.AddRange(Resources.LoadAll<GameObject>("Character/Carnivore"));
        Htemp.AddRange(Resources.LoadAll<GameObject>("Character/Herbivore"));

        for (int i = 0; i < Ctemp.Count; i++)
        {
            carnivorePrefabs.Add(Ctemp[i]);
        }

        for (int i = 0; i < Htemp.Count; i++)
        {
            herbivorePrefabs.Add(Htemp[i]);
        }
        Ctemp.Clear();
        Htemp.Clear();

        Debug.Log("Carnivores loaded: " + carnivorePrefabs.Count);
        Debug.Log("Herbivores loaded: " + herbivorePrefabs.Count);
    }

    private void LoadGame()
    {
        //Load the match scene
        SceneManager.LoadSceneAsync(gameScene);
        StartCoroutine(StartMatch());
    }


    #region Unity Methods

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
        EventManager.ActionAddHandler(EVENT.Spawn, SpawnFoodSources);
        //SceneManager.LoadSceneAsync(menuScene);
        LoadGame();
    }
    #endregion
}
