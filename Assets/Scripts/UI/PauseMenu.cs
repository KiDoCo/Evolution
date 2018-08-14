using UnityEngine;
using UnityEngine.Networking;

public class PauseMenu : MonoBehaviour {

    [SerializeField] private GameObject disconnectButton = null;
    [SerializeField] private GameObject stopGameButton = null;

    private CurMenu activemenu;

    //Menu holders
    public GameObject OptionMenu;
    public GameObject ControlMenu;
    public GameObject SoundMenu;
    public GameObject PauseUI;

    private GameObject curActiveMenu;
    public GameObject backButton;
    public GameObject optionsDefaultButton;

    public static PauseMenu Instance;

    private void Awake()
    {
        Instance = this;

        //UI.SetActive(false);

        //stopGameButton.SetActive(NetworkGameManager.Instance.Hosting);
        //disconnectButton.SetActive(!NetworkGameManager.Instance.Hosting);

    }

    // Each B_ method is used in UI buttons (Button in Unity Editor -> OnClick())

    public void B_Disconnect()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.ServerReturnToLobby();
        }
    }

    public void OpenOptionsMenu()
    {
        activemenu = CurMenu.Options;
        optionsDefaultButton.SetActive(true);
        backButton.SetActive(true);
        ChangeCurrentActiveMenu(OptionMenu);
    }

    public void OpenControlsMenu()
    {
        activemenu = CurMenu.Controls;
        ChangeCurrentActiveMenu(ControlMenu);
        backButton.SetActive(true);
        ControlMenu.GetComponent<ControlsMenu>().Init();
    }

    public void OpenSoundMenu()
    {
        activemenu = CurMenu.Sound;
        optionsDefaultButton.SetActive(true);
        backButton.SetActive(true);
        ChangeCurrentActiveMenu(SoundMenu);

    }

    public void OpenPauseMenu()
    {
        activemenu = CurMenu.Pause;
        backButton.SetActive(false);
        ChangeCurrentActiveMenu(PauseUI);
    }

    public void BackButtonMethod()
    {
        if (activemenu == CurMenu.Options)
        {
            OpenPauseMenu();
        }
        else if (activemenu == CurMenu.Controls)
        {
            OpenOptionsMenu();
        }
        else if (activemenu == CurMenu.Sound)
        {
            OpenOptionsMenu();
        }
    }

    private void ChangeCurrentActiveMenu(GameObject menu)
    {
        if (curActiveMenu != PauseUI)
        {
            if (curActiveMenu != null)
            {
                curActiveMenu.SetActive(false);
            }
        }
        curActiveMenu = menu;
        curActiveMenu.SetActive(true);
    }

}
