using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class _LobbyPlayer : NetworkLobbyPlayer {

    // Text component in child object
    [SerializeField] private Text playerNameText = null;

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

    public override void OnStartLocalPlayer()
    {
        CmdChangePlayerName(_LobbyManager.Instance.playerName.text);
    }

    public void PlayerReadyPressed()
    {
        SendReadyToBeginMessage();
    }

    public override void OnClientExitLobby()
    {
        base.OnClientExitLobby();
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

    [Command]
    private void CmdChangePlayerName(string name)
    {
        Debug.Log("Server name: " + name);
        playerName = name;
    }
}
