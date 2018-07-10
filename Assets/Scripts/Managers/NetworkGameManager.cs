using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkGameManager : NetworkLobbyManager {

    public static NetworkGameManager Instance;

    [SerializeField] private GameObject UI = null;

    // TODO: Make a menu dictionary that is possible to edit in Unity inspector.
    // And edit switchUIWindow() to work with dictonaries
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject hostUI = null;
    [SerializeField] private GameObject clientUI = null;
    private GameObject[] UIWindows;

    [SerializeField] private GameObject insertNameError = null;
    [SerializeField] private Text hostingText = null;
    [SerializeField] private Text clientAddressText = null;
    [SerializeField] private string hostUIMessage = "Hosting match in\n";
    [SerializeField] private GameObject playerListContent = null;
    [SerializeField] private InputField playerName = null;

    public GameObject PlayerListContent { get { return playerListContent; } }
    public string PlayerName { get { return playerName.text; } }
    public bool Hosting { get { return thisIsHosting; } }

    private bool thisIsHosting = false;
    private string externalIP = "";

    private void Awake ()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Resets UI
        UIWindows = new GameObject[] { mainUI, hostUI, clientUI };
        switchUIWindow(UIWindows, 0);
        UI.SetActive(true);
        insertNameError.SetActive(false);
    }

    // --- Button methods
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
        switchUIWindow(UIWindows, 1);   // Host window
        Debug.Log("Hosting started");
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();

        thisIsHosting = false;
        switchUIWindow(UIWindows, 0);   // Main window
        insertNameError.SetActive(false);
        Debug.Log("Hosting stopped");
    }

    public override void OnLobbyClientEnter()
    {
        base.OnLobbyClientEnter();

        if (!thisIsHosting)
        {
            clientAddressText.text = networkAddress + ":" + networkPort;
            switchUIWindow(UIWindows, 2);   // Client window
        }
        Debug.Log("Client joined!");
    }

    public override void OnLobbyClientExit()
    {
        base.OnLobbyClientExit();

        if (!thisIsHosting)
        {
            switchUIWindow(UIWindows, 0);   // Main window
            insertNameError.SetActive(false);
        }
        Debug.Log("Client exited");
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);

        // Disables UI if players are in game
        if (SceneManager.GetActiveScene().name == playScene)
        {
            UI.SetActive(false);
        }
        else
        {
            UI.SetActive(true);
        }
    }

    // --- Other private methods

    private IEnumerator GetPublicIP()
    {
        using (WWW www = new WWW("https://api.ipify.org"))
        {
            yield return www;
            externalIP = www.text;
            hostingText.text = hostUIMessage + externalIP + ":" + networkPort;
        }
    }

    // Enables only one of the objects in GameObject[] and disables others (NEEDS REPLACEMENT! See the top TODO)
    private void switchUIWindow(GameObject[] obj, int index)
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
