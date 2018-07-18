using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// This class is basically lobby manager but handles also some in-game networking
public class NetworkGameManager : NetworkLobbyManager {

    public static NetworkGameManager Instance;

    [SerializeField] private GameObject UI = null;

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

    private GameObject[] UIWindows;
    private bool thisIsHosting = false;
    private string externalIP = "";

    private void Awake ()
    {
        Instance = this;
        UIWindows = new GameObject[] { mainUI, hostUI, clientUI };
        DontDestroyOnLoad(gameObject);

        // Resets UI
        UIManager.switchGameObject(UIWindows, mainUI);
        UI.SetActive(true);
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
        Debug.Log("Client joined!");
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

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);

        // Disables UI if players are in-game
        if (SceneManager.GetActiveScene().name == playScene)
        {
            UI.SetActive(false);
        }
        else
        {
            UI.SetActive(true);
        }
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        LobbyPlayer player = null;

        foreach (LobbyPlayer p in lobbySlots)
        {
            if (p.playerControllerId == playerControllerId)
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

        Debug.Log("Selected: " + player.CharacterSelected.name);

        GameObject spawnedPlayer = Instantiate(spawnPrefabs.Find(x => x.name == player.CharacterSelected.name));

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
