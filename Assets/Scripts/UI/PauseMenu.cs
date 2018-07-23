using UnityEngine;
using UnityEngine.Networking;

public class PauseMenu : NetworkBehaviour {

    // All these components are child objects in this gameobject (assigned in Unity Editor)
    [SerializeField] public GameObject UI = null;   // GameObject that is parent to all UI elements in pause menu
    [SerializeField] private GameObject disconnectButton = null;
    [SerializeField] private GameObject stopGameButton = null;

    public static PauseMenu Instance;

    private void Awake()
    {
        Instance = this;

        UI.SetActive(false);

        stopGameButton.SetActive(NetworkGameManager.Instance.Hosting);
        disconnectButton.SetActive(!NetworkGameManager.Instance.Hosting);
    }

    // Each B_ method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void B_Disconnect()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.SendReturnToLobby();
        }
    }
}
