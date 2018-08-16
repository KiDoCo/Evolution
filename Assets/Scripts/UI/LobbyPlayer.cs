using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class LobbyPlayer : NetworkLobbyPlayer {

    // Text component in child object
    [SerializeField] private Text playerNameText = null;

    // All these components are child objects in this gameobject (assigned in Unity Editor)
    [SerializeField] private GameObject readyText = null;
    [SerializeField] private GameObject readyButtonText = null;
    [SerializeField] private GameObject notReadyButtonText = null;
    [SerializeField] private Dropdown characterDropdown = null;
    [SerializeField] private Text characterSelectedText = null;

    public GameObject CharacterSelected
    {
        get
        {
            return NetworkGameManager.Instance.PlayerPrefabs[characterDropdown.options[playerType].text];
        }
    }

    // Syncs name from server to clients and calls in clients changePlayerName
    [SyncVar(hook = "changePlayerName")]
    private string playerName = "";

    // Syncs type from server to clients and calls in clients changePlayerType
    [SyncVar(hook = "changePlayerType")]
    private int playerType = 0;


    // OnClientEnterLobby() is called also when client goes back from in-game to lobby
    public override void OnClientEnterLobby()
    {
        Debug.Log("Client " + netId + " entered lobby");

        // Adds players to dropbox
        if (characterDropdown.options.Count == 0)
        {
            characterDropdown.AddOptions(new List<string>(NetworkGameManager.Instance.PlayerPrefabs.Keys));
        }

        // Puts client in the player list
        transform.SetParent(NetworkGameManager.Instance.PlayerListContent.transform, false);

        // Resets player UI
        readyText.SetActive(false);
        characterDropdown.value = playerType;
        characterSelectedText.text = characterDropdown.options[playerType].text;
        characterDropdown.RefreshShownValue();

        if (!isServer)
        {
            playerNameText.text = playerName;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Resets UI
        SendNotReadyToBeginMessage();
        readyButtonText.SetActive(true);
        notReadyButtonText.SetActive(false);
        characterDropdown.gameObject.SetActive(true);
        characterSelectedText.gameObject.SetActive(false);

        if (NetworkGameManager.Instance != null)
        {
            if (isServer)
                playerName = NetworkGameManager.Instance.PlayerName;
            else
                CmdChangePlayerName(NetworkGameManager.Instance.PlayerName);
        }
        else
        {
            Debug.Log("_LobbyPlayer, OnStartLocalPlayer(): NetworkGameManager instance not found!");
        }
    }

    public override void OnClientReady(bool readyState)
    {
        base.OnClientReady(readyState);

        // Updates ready state in UI
        if (isLocalPlayer)
        {
            readyButtonText.SetActive(!readyState);
            notReadyButtonText.SetActive(readyState);
        }
        else
        {
            readyText.SetActive(readyState);
        }
    }

    // Each B_ method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void B_PlayerReady()
    {
        if (!readyToBegin)
        {
            SendReadyToBeginMessage();
        }
        else
        {
            SendNotReadyToBeginMessage();
        }
    }

    // Character selection dropdown update
    public void OnTypeChange(int type)
    {
        if (isServer)
        {
            playerType = type;
        }
        else
        {
            CmdChangePlayerType(type);
        }
    }

    // Changes name in client + server
    private void changePlayerName(string name)
    {
        playerName = name;
        playerNameText.text = playerName;
    }

    // Changes type in client + server
    private void changePlayerType(int type)
    {
        playerType = type;
        characterSelectedText.text = characterDropdown.options[playerType].text;
    }

    // Sends player name to server
    [Command]
    private void CmdChangePlayerName(string name)
    {
        playerName = name;
    }

    // Sends player type to server
    [Command]
    public void CmdChangePlayerType(int type)
    {
        playerType = type;
    }
}
