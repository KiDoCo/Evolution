using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CurMenu { Main,Options,Controls,Sound,Credit }

public class MainMenu : MonoBehaviour
{
    private CurMenu activemenu;

    #region GameObjects
    //Menu holders
    public GameObject MainMenuGrid;
    public GameObject OptionMenu;
    public GameObject ControlMenu;
    public GameObject Credit;

    //Buttons
    public GameObject backButton;
    public GameObject optionsDefaultButton;
    public GameObject controlsDefaultButton;

    private GameObject curActiveMenu;
    #endregion

    public bool FreezeOnMenu;

    private void CloseAllButtons()
    {
        OptionMenu.SetActive(false);
        backButton.SetActive(false);
        ControlMenu.SetActive(false);
    }

    public void StartNewGame()
    {
        backButton.SetActive(true);
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
    }

    public void OpenCreditMenu()
    {
        activemenu = CurMenu.Credit;
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

    private void Start()
    {
        curActiveMenu = MainMenuGrid;
        ControlMenu.SetActive(true);
        CloseAllButtons();
    }
}
