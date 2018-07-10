using UnityEngine;
using UnityEngine.Networking;

public class PauseMenu : NetworkBehaviour {

    [SerializeField] private GameObject UI = null;
    [SerializeField] private GameObject disconnectButton = null;
    [SerializeField] private GameObject stopGameButton = null;

    private void Awake()
    {
        stopGameButton.SetActive(NetworkGameManager.Instance.Hosting);
        disconnectButton.SetActive(!NetworkGameManager.Instance.Hosting);
    }

    // Each B_ method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void B_Disconnect()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.ServerReturnToLobby();
        }
    }

    public void B_StopGame()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.SendReturnToLobby();
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
