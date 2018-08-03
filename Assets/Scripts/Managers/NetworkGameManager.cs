﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// This class is basically lobby manager but handles also some in-game networking
public class NetworkGameManager : NetworkLobbyManager {

    public static NetworkGameManager Instance;

    [Space]
    [SerializeField] private GameObject[] spawnedNetManagers;
    [SerializeField] private GameObject lobbyUI = null;

    // All these components are child objects in this gameobject (assigned in Unity Editor)
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject hostUI = null;
    [SerializeField] private GameObject clientUI = null;
    [SerializeField] private GameObject insertNameError = null;
    [SerializeField] private Text hostingText = null;
    [SerializeField] private Text clientAddressText = null;
    [SerializeField] private string hostUIMessage = "Hosting match in\n";
    [SerializeField] private GameObject playerListContent = null;
    [SerializeField] private InputField playerName = null;

    public GameObject PlayerListContent { get { return playerListContent; } }
    public string PlayerName { get { return playerName.text; } }
    public bool Hosting { get { return thisIsHosting; } }
    public List<Character> InGamePlayerList { get { return inGamePlayerList; } }
    public Character LocalCharacter { get { return localCharacter; } set { localCharacter = value; } }

    private GameObject[] UIWindows;
    private List<Character> inGamePlayerList = new List<Character>();
    private Character localCharacter = null;
    private bool thisIsHosting = false;
    private string externalIP = "";

    private void Awake ()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (spawnedNetManagers.Length != 0)
        {
            foreach (GameObject g in spawnedNetManagers)
            {
                Instantiate(g);
            }
        }

        SceneManager.LoadScene(lobbyScene);
        UIWindows = new GameObject[] { mainUI, hostUI, clientUI };

        // Resets UI
        UIManager.switchGameObject(UIWindows, mainUI);
        insertNameError.SetActive(false);
    }

    // --- Lobby button methods
    // Each B_ method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void B_HostGame()
    {
        if (!isNetworkActive)
        {
            if (playerName.text == "")
            {
                insertNameError.SetActive(true);
            }
            else
            {
                // Called from NetworkManager. NetworkManager calls then OnLobbyStartHost()
                StartHost();
            }
        }
    }

    public void B_EndHostedGame()
    {
        if (isNetworkActive)
        {
            // Calls OnLobbyStopHost() etc.
            StopHost();
        }
    }

    public void B_JoinGame()
    {
        if (playerName.text == "")
        {
            insertNameError.SetActive(true);
        }
        else
        {
            // Calls OnLobbyClientEnter() etc.
            StartClient();
        }
    }

    public void B_DisconnectGame()
    {
        if (isNetworkActive)
        {
            // Calls OnLobbyClientExit() etc.
            StopClient();
        }
    }

    // --- InputField update methods

    public void IPChanged(string IP)
    {
        networkAddress = IP;
    }

    public void PortChanged(string port)
    {
        int parsedPort = 0;
        if (int.TryParse(port, out parsedPort))
        {
            networkPort = parsedPort;
        }
    }

    // --- Lobby network methods

    public override void OnLobbyStartHost()
    {
        base.OnLobbyStartHost();

        thisIsHosting = true;
        StartCoroutine(GetPublicIP());
        hostingText.text = hostUIMessage + networkAddress + ":" + networkPort;  // Temp message before public IP is updated
        UIManager.switchGameObject(UIWindows, hostUI);
        Debug.Log("Hosting started");
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();

        thisIsHosting = false;
        UIManager.switchGameObject(UIWindows, mainUI);
        insertNameError.SetActive(false);
        Debug.Log("Hosting stopped");

    }

    public override void OnLobbyClientEnter()
    {
        base.OnLobbyClientEnter();

        if (!thisIsHosting)
        {
            clientAddressText.text = networkAddress + ":" + networkPort;
            UIManager.switchGameObject(UIWindows, clientUI);
        }
        Debug.Log("Client joined");
    }

    public override void OnLobbyClientExit()
    {
        base.OnLobbyClientExit();

        if (!thisIsHosting)
        {
            UIManager.switchGameObject(UIWindows, mainUI);
            insertNameError.SetActive(false);
        }
        Debug.Log("Client exited");
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        base.OnLobbyServerSceneChanged(sceneName);

        Debug.Log("Scene changed");

        if (sceneName == playScene)
        {
            EventManager.Broadcast(EVENT.RoundBegin);
        }
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);

        // Disables UI if players are in-game
        if (SceneManager.GetActiveScene().name == playScene)
        {
            Instantiate(UIManager.Instance.PauseMenuPrefab);
            lobbyUI.SetActive(false);
            UIManager.Instance.HideCursor(true);
        }
        else if (SceneManager.GetActiveScene().name == lobbyScene)
        {
            lobbyUI.SetActive(true);
            UIManager.Instance.HideCursor(false);
            InGamePlayerList.Clear();
            InGameManager.Instance.DestroyLists();
        }
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        LobbyPlayer player = null;

        // Finds the lobby player
        foreach (LobbyPlayer p in lobbySlots)
        {
            if (p.netId == conn.playerControllers[playerControllerId].unetView.netId)
            {
                player = p;
                break;
            }
        }

        if (player == null)
        {
            Debug.Log("NetworkGameManager, OnLobbyServerCreateGamePlayer: Player not found!");
            return null;
        }

        Debug.Log("Client " + conn.playerControllers[playerControllerId].unetView.netId + " selected " + player.CharacterSelected.name);

        GameObject spawnedPlayer;

        // Spawns corresponding player prefab
        if (!spawnPrefabs.Contains(player.CharacterSelected))
        {
            spawnedPlayer = Instantiate(gamePlayerPrefab);
        }
        else
        {
            spawnedPlayer = Instantiate(player.CharacterSelected, startPositions[Random.Range(0, startPositions.Count)].position, player.CharacterSelected.transform.rotation);
            InGamePlayerList.Add(spawnedPlayer.GetComponent<Character>());
        }

        return spawnedPlayer;
    }

    // --- Other private methods

    private IEnumerator GetPublicIP()
    {
        using (WWW www = new WWW("https://api.ipify.org"))
        {
            yield return www;

            // After the file has downloaded
            externalIP = www.text;
            hostingText.text = hostUIMessage + externalIP + ":" + networkPort;
        }
    }
}
