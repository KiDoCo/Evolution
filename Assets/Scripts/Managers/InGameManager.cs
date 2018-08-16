using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public enum PredatorRanks { ApexPredator, FishersPrey, Pacifish }

public class InGameManager : NetworkBehaviour
{
    //Ingnore pragmas for unnecessary warnings
#pragma warning disable

    public static InGameManager Instance;

    // Match variables
    private const float minutesToSeconds = 60.0f;
    private float startingMatchTimer = 10.0f * minutesToSeconds;  // Time value in minutes
    [SerializeField] private float interval = 0.1f;  // The time in seconds that spawning will happen
    [SerializeField] private float deathPenaltyTime = 2.0f;
    [SerializeField] private float experiencePenalty = 25.0f;
    [SerializeField] private float endScreenTime = 20f;
    [SerializeField] private int maxLifeCount = 2;
    [SyncVar(hook = "Timer")]
    private float matchTimer;
    [SyncVar] private int lifeCount;
    [SyncVar] private bool matchEnd = false;    // BUG: MatchEnd doesn't sync to other players
    private bool matchStart = true;

    // Gamemanager lists
    private List<GameObject> carnivorePrefabs = new List<GameObject>();
    private List<GameObject> herbivorePrefabs = new List<GameObject>();
    public Dictionary<string, GameObject> PlayerPrefabs = new Dictionary<string, GameObject>();
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

    public float MatchTimer
    {
        get
        {
            return matchTimer;
        }

        set
        {
            matchTimer = Mathf.Clamp(value, 0, Mathf.Infinity);
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

    public int LifeCount
    {
        get
        {
            return lifeCount;
        }

        set
        {
            lifeCount = (int)Mathf.Clamp(value, 0f, Mathf.Infinity);
        }
    }

    public bool MatchEnd
    {
        get
        {
            return matchEnd;
        }
    }

    public float StartingMatchTimer
    {
        get
        {
            return startingMatchTimer;
        }
    }

    #endregion

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
        if (SceneManager.GetActiveScene().name != gameScene) yield return null;
        MatchTimer = StartingMatchTimer;
        Debug.Log(MatchTimer);
        MatchTimer = startingMatchTimer;
        LifeCount = maxLifeCount;
        yield return SpawnFoodSources();
        EventManager.Broadcast(EVENT.AINodeSpawn);
        matchStart = false;
        InvokeRepeating("IncreaseFoodOnSources", interval, interval);
    }

    /// <summary>
    /// Respawns the player 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private List<Transform> distanceList = new List<Transform>();

    private IEnumerator Respawn(Herbivore player)
    {
        player.EnablePlayer(false);
        // - Change camera to fixed camera
        yield return new WaitForSeconds(deathPenaltyTime);

        Carnivore carnivore = GameObject.FindObjectOfType<Carnivore>();

        player.transform.GetComponent<NetworkPlayerCaveFog>().useFog = false;

        if (carnivore)  //  If carnivore in game
        {
            Vector3 carnivoreLocation = carnivore.transform.position;

            distanceList.Clear();

            foreach (Transform startPos in NetworkGameManager.Instance.herbivoreStartPoints)    //  Check starting positions if  is empty
            {
                if (startPos.GetComponent<StartPositionCheck>().isEmpty == true)
                {
                    //  Debugging Distances >>     float dist = Vector3.Distance(startPos.position, carnivore.transform.position); Debug.Log("" + startPos.transform.name + " - matka: " + dist);
                    distanceList.Add(startPos);
                }
            }

            distanceList.Sort(delegate (Transform c, Transform s)   //  Sort list by: smallest distance >>  biggest distance. Between starting positions and carnivore
            {
                return Vector3.Distance(carnivore.transform.position, c.transform.position).CompareTo((Vector3.Distance(carnivore.transform.position, s.transform.position)));
            }
            );

            player.transform.position = distanceList.ElementAt(distanceList.Count - 1).position;    //  Last in list is most far from carnivore and is herbivores starting position.
            //  Debugging Herbivore Starting Position >>    Debug.Log("Herbivore started at: " + distanceList.ElementAt(distanceList.Count - 1));

        }
        else  //  If NOT carnivore in game then random spawn
        {
            //  Debugging   If Not Carnivore >>     Debug.Lornivore not found.");
            player.transform.position = NetworkGameManager.Instance.herbivoreStartPoints[Random.Range(0, NetworkGameManager.Instance.herbivoreStartPoints.Count)].position;
        }

        // - Reset values
        player.Experience -= experiencePenalty;
        player.EnablePlayer(true);
        player.gameObject.SetActive(true);
        yield return null;
    }


    /// <summary>
    /// Ends the match when time ends or one of the sides wins the game
    /// </summary>
    [ServerCallback]
    private void EndMatch()
    {
        matchEnd = true;
        CancelInvoke();
        // Get stats and stop/end match for in game players
        foreach (Character p in NetworkGameManager.Instance.InGamePlayerList)
        {
            p.EnablePlayer(false);
            // - Get stats for end screen (ShowEndScreen())
            // - Fixed camera in scene with end screen
        }

        StartCoroutine(ReturnToLobby(endScreenTime));
    }

    private IEnumerator ReturnToLobby(float time)
    {
        yield return new WaitForSeconds(time);
        matchEnd = false;
        NetworkGameManager.Instance.SendReturnToLobby();
    }

    /// <summary>
    /// Ends the match for a single player, kills it
    /// </summary>
    public void KillPlayer(Character player)
    {
        player.EnablePlayer(false);
        // - Fixed camera in scene. Spectate others?
    }

    /// <summary>
    /// Spawns the sources to the environment
    /// </summary>
    private IEnumerator SpawnFoodSources()
    {
        while (!NetworkServer.active && !NetworkServer.localClientActive) yield return null;

        if (isServer)
        {
            for (int i = 0; i < GameObject.FindGameObjectsWithTag(foodSourceName).Length; i++)
            {
                FoodSpawnPointList.Add(GameObject.FindGameObjectsWithTag(foodSourceName)[i].transform);
            }
            for (int i = 0; i < FoodSpawnPointList.Capacity; i++)
            {
                GameObject clone = Instantiate(foodsources[0], FoodSpawnPointList[i].position, Quaternion.identity);
                NetworkServer.Spawn(clone);
                clone.name = foodSourceName + i;
            }
        }
        yield return 1;
    }

    /// <summary>
    /// Checks if the player can be spawned
    /// </summary>
    /// <param name="player"></param>
    public void RespawnPlayer(Herbivore player)
    {
        if (LifeCount > 0)
        {
            LifeCount--;
            StartCoroutine(Respawn(player));
        }
        else
        {
            KillPlayer(player);
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


    private void Timer(float time)
    {
        MatchTimer = time;
    }

    // Void to IEnumerable
    [ServerCallback]
    public void StartGame()
    {
        StartCoroutine(StartMatch());
    }

    public void ClearBoxes()
    {
        for (int a = 0; a < FoodSpawnPointList.Capacity; a++)
        {
            Destroy(FoodSpawnPointList[a].gameObject);
        }
        FoodSpawnPointList.Clear();
    }

    public void DestroyFoodPlaceLists()
    {
        foreach (GameObject g in FoodPlaceList)
        {
            if (g != null)
                Destroy(g);
        }
        FoodPlaceList.Clear();
    }

    #region Unity Methods

    [ServerCallback]
    private void Update()
    {
        if (matchEnd || matchStart) return;

        MatchTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.O)) // for testing purposes
        {
            Debug.Log("Decrease");
            MatchTimer -= 20;
        }

        if (MatchTimer == 0 && SceneManager.GetActiveScene().name == gameScene)
        {
            Debug.Log("Time's up!");
            EventManager.Broadcast(EVENT.RoundEnd);
        }
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAssetToDictionaries();
        EventManager.ActionAddHandler(EVENT.RoundBegin, StartGame);
        EventManager.ActionAddHandler(EVENT.RoundEnd, EndMatch);
    }

    #endregion
}