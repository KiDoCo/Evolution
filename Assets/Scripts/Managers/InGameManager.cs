using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SyncVar] private float matchTimer;      // Mathf clamppaa juttuja
    [SyncVar] private int lifeCount;
    [SyncVar] private bool inMatch = false;
    private GameObject mapCamera = null;

    // Gamemanager lists
    [SerializeField] private List<GameObject> foodSources;
    public List<GameObject> FoodPlaceList = new List<GameObject>();
    private List<GameObject> FoodSpawnPointList;

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

    public bool InMatch
    {
        get
        {
            return inMatch;
        }
    }

    public float StartingMatchTimer
    {
        get
        {
            return startingMatchTimer;
        }
    }

    public GameObject MapCamera
    {
        get
        {
            return mapCamera;
        }
        set
        {
            mapCamera = value;
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
        matchTimer = startingMatchTimer;
        LifeCount = maxLifeCount;
        yield return SpawnFoodSources();
        EventManager.Broadcast(EVENT.AINodeSpawn);
        inMatch = true;
        InvokeRepeating("IncreaseFoodOnSources", interval, interval);
    }

    /// <summary>
    /// Respawns the player 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator Respawn(Herbivore player)
    {
        player.EnablePlayer(false);
        player.EnablePlayerCamera(false);
        yield return new WaitForSeconds(deathPenaltyTime);
        // - Reset values
        player.Experience -= experiencePenalty;
        player.EnablePlayer(true);
        player.EnablePlayerCamera(true);
        player.gameObject.SetActive(true);
        yield return null;
    }

    /// <summary>
    /// Ends the match when time ends or one of the sides wins the game
    /// </summary>
    [ServerCallback]
    private void EndMatch()
    {
        inMatch = false;
        CancelInvoke();
        // Get stats and stop/end match for in game players
        foreach (Character p in NetworkGameManager.Instance.InGamePlayerList)
        {
            p.EnablePlayerCamera(false);
            p.EnablePlayer(false);
            UIManager.Instance.MatchResultScreen(p);
        }

        StartCoroutine(ReturnToLobby(endScreenTime));
    }

    private IEnumerator ReturnToLobby(float time)
    {
        yield return new WaitForSeconds(time);
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
            while (FoodSpawnPointList == null)
                yield return null;

            for (int i = 0; i < FoodSpawnPointList.Count; i++)
            {
                GameObject clone = Instantiate(foodSources[0], FoodSpawnPointList[i].transform.position, Quaternion.identity);
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

    // Void to IEnumerable
    [ServerCallback]
    public void StartGame()
    {
        StartCoroutine(StartMatch());
    }

    public void HideBoxes()
    {
        foreach (GameObject g in FoodSpawnPointList)
        {
            if (g != null)
                g.SetActive(false);
        }
    }

    public void DestroyFoodPlaceList()
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
        if (!inMatch) return;

        MatchTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.O)) // for testing purposes
        {
            MatchTimer -= 20;
        }

        if (matchTimer == 0 && SceneManager.GetActiveScene().name == gameScene)
        {
            Debug.Log("Time's up!");
            EventManager.Broadcast(EVENT.RoundEnd);
        }
    }

    private void Awake()
    {
        Instance = this;

        EventManager.ActionAddHandler(EVENT.RoundBegin, StartGame);
        EventManager.ActionAddHandler(EVENT.RoundEnd, EndMatch);

        UIManager.Instance.HideCursor(true);
        Debug.Log("InGameManager awake");
    }

    private void Start()
    {
        FoodSpawnPointList = new List<GameObject>(GameObject.FindGameObjectsWithTag(foodSourceName));
        HideBoxes();
    }

    private void OnDestroy()
    {
        EventManager.ActionDeleteHandler(EVENT.RoundBegin);
        EventManager.ActionDeleteHandler(EVENT.RoundEnd);
    }

    public override void PreStartClient()
    {
        Debug.Log("InGameManager, PreStartClient");
    }

    #endregion
}
