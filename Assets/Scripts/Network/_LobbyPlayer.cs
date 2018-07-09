using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class _LobbyPlayer : NetworkLobbyPlayer {

    // Text component in child object
    [SerializeField] private Text playerNameText = null;

    [SerializeField] private GameObject readyText = null;
    [SerializeField] private GameObject readyButton = null;
    [SerializeField] private GameObject readyButtonText = null;
    [SerializeField] private GameObject notReadyButtonText = null;

    // Syncs name from server to clients and calls in clients changePlayerName
    [SyncVar(hook = "changePlayerName")]
    private string playerName = "";


    // OnClientEnterLobby() is called also when client goes back from in game to lobby
    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        Debug.Log("Client enter lobby");

        // Puts client in the player list
        transform.SetParent(_LobbyManager.Instance.PlayerListContent.transform, false);

        // Resets player ready state
        readyText.SetActive(false);

        if (isClient)
        {
            updateNameText();
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Resets ready state and button
        SendNotReadyToBeginMessage();
        readyButtonText.SetActive(true);
        notReadyButtonText.SetActive(false);
        readyButton.SetActive(true);

        if (_LobbyManager.Instance != null)
            CmdChangePlayerName(_LobbyManager.Instance.PlayerName);
        else
            Debug.Log("_LobbyPlayer, OnStartLocalPlayer(): _LobbyManager instance not found!");
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

    public void PlayerReadyPressed()
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

    // Changes name in client
    private void changePlayerName(string name)
    {
        playerName = name;
        updateNameText();
    }

    private void updateNameText()
    {
        playerNameText.text = playerName;
    }

    // Sends player name to server
    [Command]
    private void CmdChangePlayerName(string name)
    {
        playerName = name;
    }
}
