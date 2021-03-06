﻿using System.Collections;
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
    [SyncVar] private float matchTimer;      // Mathf clamppaa juttuja    [SyncVar] private int lifeCount;
    [SyncVar] private bool inMatch = false;
    private GameObject mapCamera = null;
    private int herbivoresWon = 0;
    private int lifeCount;
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

    // Void to IEnumerable
    [ServerCallback]
    public void StartGame()
    {
        StartCoroutine(StartMatch());
    }

    /// <summary>
    /// Starts the match between players. Must be called after loading the game scene
    /// </summary>
    private IEnumerator StartMatch()
    {
        if (SceneManager.GetActiveScene().name != gameScene) yield return null;
        matchTimer = startingMatchTimer;
        LifeCount = maxLifeCount * NetworkGameManager.Instance.InGamePlayerList.Count(x => x.GetType() == typeof(Herbivore));
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


    /// <summary>    /// Ends the match when time ends or one of the sides wins the game
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
        }

        // Show match screen
        UIManager.Instance.MatchResultScreen();
        RpcShowResultScreen();

        StartCoroutine(ReturnToLobby(endScreenTime));
    }

    [ClientRpc]
    private void RpcShowResultScreen()
    {
        UIManager.Instance.MatchResultScreen();
    }

    private IEnumerator ReturnToLobby(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkGameManager.Instance.SendReturnToLobby();
    }

    /// <summary>
    /// Checks if the player can be spawned
    /// </summary>
    /// <param name="player"></param>
    [ServerCallback]
    public void RespawnPlayer(Herbivore player)
    {
        LifeCount--;
        StartCoroutine(Respawn(player));
    }

    [ServerCallback]
    public void KillPlayer(Herbivore player)
    {
        player.EnablePlayer(false);
        player.EnablePlayerCamera(false);
    }


    /// <summary>
    /// Ends the match for a single player
    /// </summary>
    [ServerCallback]
    public void EndMatchForPlayer(Herbivore player)
    {
        herbivoresWon += 1;
        player.EnablePlayer(false);
        player.EnablePlayerCamera(false);
        if (herbivoresWon == NetworkGameManager.Instance.InGamePlayerList.Count - 1)
        {
            EndMatch();
        }
    }

    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
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

    #endregion

    /// <summary>
    /// Hides scene food spawnpoint boxes
    /// </summary>
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
        mapCamera = GameObject.FindGameObjectWithTag("MapCamera");
    }

    private void OnDestroy()
    {
        EventManager.ActionDeleteHandler(EVENT.RoundBegin);
        EventManager.ActionDeleteHandler(EVENT.RoundEnd);
    }

    #endregion
}