using UnityEngine;
using UnityEngine.Networking;

public class PauseMenu : NetworkBehaviour {

    [SerializeField] private GameObject UI = null;
    [SerializeField] private GameObject disconnectButton = null;
    [SerializeField] private GameObject stopGameButton = null;

    private void Awake()
    {
        stopGameButton.SetActive(_LobbyManager.Instance.Hosting);
        disconnectButton.SetActive(!_LobbyManager.Instance.Hosting);
    }

    public void DisconnectPressed()
    {
        if (_LobbyManager.Instance != null)
        {
            _LobbyManager.Instance.ServerReturnToLobby();
        }
    }

    public void StopGamePressed()
    {
        if (_LobbyManager.Instance != null)
        {
            _LobbyManager.Instance.SendReturnToLobby();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI.SetActive(!UI.activeSelf);
        }
    }
}
