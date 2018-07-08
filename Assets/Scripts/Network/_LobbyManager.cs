using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class _LobbyManager : NetworkLobbyManager {

    public static _LobbyManager Instance;

    [SerializeField] private GameObject UI = null;

    // TODO: Make a menu dictionary that is possible to edit in Unity inspector.
    // And edit switchEnabledGameObject() to work with dictonaries
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject hostUI = null;
    [SerializeField] private GameObject clientUI = null;

    [SerializeField] private GameObject insertNameError = null;
    [SerializeField] private Text hostingText = null;
    [SerializeField] private Text clientAddressText = null;
    [SerializeField] private string hostUIMessage = "Hosting match in\n";

    [SerializeField] private GameObject playerListContent = null;
    public GameObject PlayerListContent { get { return playerListContent; } }
    [SerializeField] private InputField playerName = null;
    public string PlayerName { get { return playerName.text; } }

    private bool hosting = false;
    public bool Hosting { get { return hosting; } }
    private string externalIP = "";

    private void Awake ()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        switchEnabledGameObject(new GameObject[] { mainUI, hostUI, clientUI }, 0);
        insertNameError.SetActive(false);
    }

    // Each Pressed method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void HostGamePressed()
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

    public void EndHostedGamePressed()
    {
        if (isNetworkActive)
        {
            // OnLobbyStopHost()
            StopHost();
        }
    }

    public void JoinGamePressed()
    {
        if (playerName.text == "")
        {
            insertNameError.SetActive(true);
        }
        else
        {
            // OnLobbyClientEnter()
            StartClient();
        }
    }

    public void DisconnectGamePressed()
    {
        if (isNetworkActive)
        {
            // OnLobbyClientExit()
            StopClient();
        }
    }

    // InputField update methods

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

    // Lobby network methods

    public override void OnLobbyStartHost()
    {
        base.OnLobbyStartHost();

        hosting = true;
        StartCoroutine(GetExternalIP());
        UpdateText(hostingText, hostUIMessage + networkAddress + ":" + networkPort);
        switchEnabledGameObject(new GameObject[] {mainUI, hostUI, clientUI}, 1);
        Debug.Log("Hosting started...");
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();

        hosting = false;
        switchEnabledGameObject(new GameObject[] { mainUI, hostUI, clientUI }, 0);
        insertNameError.SetActive(false);
        Debug.Log("Hosting stopped.");
    }

    public override void OnLobbyClientEnter()
    {
        base.OnLobbyClientEnter();

        if (!hosting)
        {
            UpdateText(clientAddressText, networkAddress + ":" + networkPort);
            switchEnabledGameObject(new GameObject[] { mainUI, hostUI, clientUI }, 2);
        }
        Debug.Log("Client joined!");
    }

    public override void OnLobbyClientExit()
    {
        base.OnLobbyClientExit();

        if (!hosting)
        {
            switchEnabledGameObject(new GameObject[] { mainUI, hostUI, clientUI }, 0);
            insertNameError.SetActive(false);
        }
        Debug.Log("Client exited.");
    }

    public override void OnLobbyServerConnect(NetworkConnection conn)
    {
        base.OnLobbyServerConnect(conn);
        Debug.Log("Client " + conn.connectionId + " connected!");
    }

    public override void OnLobbyServerPlayersReady()
    {
        base.OnLobbyServerPlayersReady();

        Debug.Log("Players are ready!");
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);

        if (SceneManager.GetActiveScene().name == playScene)
        {
            UI.SetActive(false);
        }
        else
        {
            UI.SetActive(true);
        }
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        base.OnLobbyServerSceneChanged(sceneName);

        Debug.Log("Scene changed! Players: " + numPlayers);

        if (sceneName == lobbyScene)
        {
            foreach (NetworkLobbyPlayer player in lobbySlots)
            {
                player.SendNotReadyToBeginMessage();
            }
        }
    }

    IEnumerator GetExternalIP()
    {
        using (WWW www = new WWW("https://api.ipify.org"))
        {
            yield return www;
            externalIP = www.text;
            UpdateText(hostingText, hostUIMessage + externalIP + ":" + networkPort);
        }
    }

    private void SetExternalIP(string IP)
    {
        externalIP = IP;
    }

    private void UpdateText(Text textElement, string message)
    {
        textElement.text = message;
    }

    // Enables only one of the objects in GameObject[] and disables others (NEEDS REPLACEMENT! See the top TODO)
    private void switchEnabledGameObject(GameObject[] obj, int index)
    {
        for (int o = 0; o < obj.Length; o++)
        {
            if (o == index)
            {
                obj[o].SetActive(true);
            }
            else if (obj[o].activeSelf)
            {
                obj[o].SetActive(false);
            }
        }
    }
}
