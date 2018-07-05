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

    [SyncVar(hook = "changePlayerName")]
    private string playerName = "";


    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        transform.SetParent(_LobbyManager.Instance.playerListContent.transform, false);

        if (isClient)
        {
            updateNameText();
        }
    }

    public override void OnClientExitLobby()
    {
        base.OnClientExitLobby();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        readyButton.SetActive(true);

        CmdChangePlayerName(_LobbyManager.Instance.playerName.text);
    }

    public override void OnClientReady(bool readyState)
    {
        base.OnClientReady(readyState);

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
