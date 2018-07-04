using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class _LobbyManager : NetworkLobbyManager {

    public static _LobbyManager Instance;

    // TODO: Make a menu dictionary that is possible to edit in Unity inspector.
    // And edit switchEnabledGameObject() to work with dictonaries
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject hostUI = null;
    [SerializeField] private GameObject clientUI = null;

    [SerializeField] private Text hostingText = null;
    [SerializeField] private Text clientAddressText = null;

    // Used in other scripts
    [SerializeField] public GameObject playerListContent = null;
    [SerializeField] public InputField playerName = null;

    [SerializeField] private string hostUIMessage = "Hosting match in\n";

    private bool hosting = false;

    private void Awake ()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        switchEnabledGameObject(new GameObject[] { mainUI, hostUI, clientUI }, 0);
    }

    // Each Pressed method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void HostGamePressed()
    {
        if (!isNetworkActive)
        {
            if (playerName.text == "")
            {
                Debug.Log("_LobbyManager, HostGamePressed(): Insert name!");
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
            Debug.Log("_LobbyManager, HostGamePressed(): Insert name!");
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
        hostingText.text = hostUIMessage + networkAddress + ":" + networkPort;
        switchEnabledGameObject(new GameObject[] {mainUI, hostUI, clientUI}, 1);
        Debug.Log("Hosting started...");
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();

        hosting = false;
        switchEnabledGameObject(new GameObject[] { mainUI, hostUI, clientUI }, 0);
        Debug.Log("Hosting stopped.");
    }

    public override void OnLobbyClientEnter()
    {
        base.OnLobbyClientEnter();

        if (!hosting)
        {
            clientAddressText.text = networkAddress + ":" + networkPort;
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

        CheckReadyToBegin();
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
