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

    [SerializeField] private List<InspectorObject> UIWindows; [Space]

    // All these components are child objects in this gameobject (assigned in Unity Editor)
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
        InspectorObject.switchGameObject(UIWindows, "MainUI");

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
        InspectorObject.switchGameObject(UIWindows, "HostUI");
        Debug.Log("Hosting started");
    }

    public override void OnLobbyStopHost()
    {
        base.OnLobbyStopHost();

        thisIsHosting = false;
        InspectorObject.switchGameObject(UIWindows, "MainUI");
        insertNameError.SetActive(false);
        Debug.Log("Hosting stopped");
    }

    public override void OnLobbyClientEnter()
    {
        base.OnLobbyClientEnter();

        if (!thisIsHosting)
        {
            clientAddressText.text = networkAddress + ":" + networkPort;
            InspectorObject.switchGameObject(UIWindows, "ClientUI");
        }
        Debug.Log("Client joined!");
    }

    public override void OnLobbyClientExit()
    {
        base.OnLobbyClientExit();

        if (!thisIsHosting)
        {
            InspectorObject.switchGameObject(UIWindows, "MainUI");
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
}
