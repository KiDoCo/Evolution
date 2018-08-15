using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// This class is basically lobby manager but handles also some in-game networking
public class NetworkGameManager : NetworkLobbyManager {

    public static NetworkGameManager Instance;

    [Space]
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
    [SerializeField] private GameObject errorWindow = null;
    [SerializeField] private Text errorWindowText = null;

    private GameObject[] UIWindows;
    private List<Character> inGamePlayerList = new List<Character>();
    private Character localCharacter = null;
    private bool thisIsHosting = false;
    private string externalIP = "";
    private List<GameObject> carnivorePrefabs = new List<GameObject>();
    private List<GameObject> herbivorePrefabs = new List<GameObject>();
    public Dictionary<string, GameObject> PlayerPrefabs = new Dictionary<string, GameObject>();

    public GameObject PlayerListContent { get { return playerListContent; } }
    public string PlayerName { get { return playerName.text; } }
    public bool Hosting { get { return thisIsHosting; } }
    public List<Character> InGamePlayerList { get { return inGamePlayerList; } }
    public Character LocalCharacter { get { return localCharacter; } set { localCharacter = value; } }
    public List<GameObject> HerbivorePrefabs { get { return herbivorePrefabs; } set { herbivorePrefabs = value; } }

    private void Awake ()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadAssetToDictionaries();

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
        thisIsHosting = true;
        StartCoroutine(GetPublicIP());
        hostingText.text = hostUIMessage + networkAddress + ":" + networkPort;  // Temp message before public IP is updated
        UIManager.switchGameObject(UIWindows, hostUI);
        Debug.Log("Hosting started");
    }

    public override void OnLobbyStopHost()
    {
        thisIsHosting = false;
        UIManager.switchGameObject(UIWindows, mainUI);
        insertNameError.SetActive(false);
        Debug.Log("Hosting stopped");
    }

    public override void OnLobbyClientEnter()
    {
        if (!thisIsHosting)
        {
            clientAddressText.text = networkAddress + ":" + networkPort;
            UIManager.switchGameObject(UIWindows, clientUI);
        }
        Debug.Log("Client joined");
    }

    public override void OnLobbyClientExit()
    {
        if (!thisIsHosting)
        {
            UIManager.switchGameObject(UIWindows, mainUI);
            insertNameError.SetActive(false);
        }
        Debug.Log("Client exited");
    }

    public override void OnLobbyServerPlayersReady()
    {
        List<LobbyPlayer> players = new List<LobbyPlayer>();
        List<LobbyPlayer> selectedCarnivores = new List<LobbyPlayer>();

        // Find all lobbyplayers
        foreach (var netObj in NetworkServer.objects)
        {
            LobbyPlayer p = netObj.Value.GetComponent<LobbyPlayer>();

            if (p != null)
            {
                players.Add(p);

                if (p.PlayerCharacter == "Carnivore")
                {
                    selectedCarnivores.Add(p);
                }
            }
        }

        // Picks random carnivore or herbivore
        int carnivoreAmount = selectedCarnivores.Count;
        if (carnivoreAmount > 1)
        {
            int pick = Random.Range(0, carnivoreAmount);
            for (int c = 0; c < carnivoreAmount; c++)
            {
                if (c != pick)
                {
                    selectedCarnivores[c].PlayerCharacter = "Herbivore";
                }
            }
        }
        else if (carnivoreAmount == 0)
        {
            int pick = Random.Range(0, players.Count);
            players[pick].PlayerCharacter = "Carnivore";
        }

        base.OnLobbyServerPlayersReady();
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        if (sceneName == playScene)
        {
            EventManager.Broadcast(EVENT.RoundBegin);
        }
        Debug.Log("Scene changed");
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        // Disables UI if players are in-game
        if (SceneManager.GetActiveScene().name == playScene)
        {
            Instantiate(UIManager.Instance.PauseMenuPrefab);
            lobbyUI.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name == lobbyScene)
        {
            lobbyUI.SetActive(true);
            UIManager.Instance.HideCursor(false);
            InGamePlayerList.Clear();
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

    private void ShowErrorBox(string msg)
    {
        errorWindowText.text = msg;
        errorWindow.SetActive(true);
    }

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
