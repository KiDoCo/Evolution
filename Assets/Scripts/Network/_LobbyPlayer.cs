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

        if (isServer)
            Debug.Log("Server");
        if (isClient)
            Debug.Log("Client");

        CmdChangePlayerName(_LobbyManager.Instance.playerName.text);

        transform.SetParent(_LobbyManager.Instance.playerListContent.transform, false);
    }

    public override void OnClientExitLobby()
    {
        base.OnClientExitLobby();
    }

    private void changePlayerName(string name)
    {
        Debug.Log("Client name: " + name);
        playerName = name;
        playerNameText.text = name;
    }

    [Command]
    private void CmdChangePlayerName(string name)
    {
        Debug.Log("Server name: " + name);
        playerName = name;
    }
}
