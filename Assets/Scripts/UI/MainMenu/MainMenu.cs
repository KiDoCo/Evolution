using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CurMenu { Main,Options,Controls,Sound,Credit,Pause }

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    private CurMenu activemenu;

    #region GameObjects
    //Menu holders
    public GameObject MainMenuGrid;
    public GameObject OptionMenu;
    public GameObject ControlMenu;
    public GameObject CreditMenu;
    public GameObject SoundMenu;

    //Buttons
    public GameObject backButton;
    public GameObject optionsDefaultButton;
    public GameObject controlsDefaultButton;
    public GameObject soundDefaultButton;

    private GameObject curActiveMenu;
    #endregion

    public bool FreezeOnMenu;
    public Image canvasImage;


    private void CloseAllButtons()
    {
        OptionMenu.SetActive(false);
        backButton.SetActive(false);
        ControlMenu.SetActive(false);
        SoundMenu.SetActive(false);
    }

    public void StartNewGame()
    {
        backButton.SetActive(true);
        // Change to lobby
        canvasImage.enabled = false;
        
       
    }

    public void LoadLevel()
    {
        backButton.SetActive(false);
        ChangeCurrentActiveMenu(MainMenuGrid);
    }

    #region Button methods

    public void OpenOptionsMenu()
    {
        activemenu = CurMenu.Options;
        optionsDefaultButton.SetActive(true);
        backButton.SetActive(true);
        ChangeCurrentActiveMenu(OptionMenu);
    }

    public void OpenMainMenu()
    {
        activemenu = CurMenu.Main;
        ChangeCurrentActiveMenu(MainMenuGrid);

        backButton.SetActive(false);
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

    public void OpenCreditMenu()
    {
        activemenu = CurMenu.Credit;
        ChangeCurrentActiveMenu(CreditMenu);
        backButton.SetActive(true);
        CreditMenu.GetComponent<CreditsScreen>();
    }

    public void BackButtonMethod()
    {

        if(activemenu == CurMenu.Options)
        {
            OpenMainMenu();
        }
        else if(activemenu == CurMenu.Credit)
        {
            OpenMainMenu();
        }
        else if(activemenu == CurMenu.Controls)
        {
            OpenOptionsMenu();
        }
        else if(activemenu == CurMenu.Sound)
        {
            OpenOptionsMenu();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
    #endregion

    public void LoadMainMenu()
    {
        ChangeCurrentActiveMenu(MainMenuGrid);
    }


    private void ChangeCurrentActiveMenu(GameObject menu)
    {
        if(curActiveMenu != null) curActiveMenu.SetActive(false);
        curActiveMenu = menu;
        curActiveMenu.SetActive(true);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        curActiveMenu = MainMenuGrid;
        ControlMenu.SetActive(true);
        CloseAllButtons();
    }
}
