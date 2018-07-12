using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MMStatus { main,inGame,all}

public class MainMenu : MonoBehaviour
{
    public bool Loading;
    public Text Loadtext;
    public GameObject LoadUI;

    private string TargetModelId;

    private float loadT;
    private int index;
    private string dot = ".";

    private bool EnableMenu = true;

    private bool InGameMenu;

    public GameObject MainMenuGrid;

    public GameObject OptionMenu;

    public GameObject ControlMenu;

    private MMStatus mmStatus;

    public List<Menubutton> MButtons = new List<Menubutton>();

    public GameObject backButton;
    public GameObject optionsDefaultButton;
    public GameObject controlsDefaultButton;


    private GameObject curActiveMenu;

    public bool FreezeOnMenu;

    private void CloseAllButtons()
    {
        foreach(Menubutton b in MButtons)
        {
            b.go.SetActive(false);
        }

        OptionMenu.SetActive(false);
        backButton.SetActive(false);
        ControlMenu.SetActive(false);
    }

    private void OpenButtons()
    {
        CloseAllButtons();

        foreach(Menubutton b in MButtons)
        {
            if(b.enabled)
            {
                if(b.Formenu == MMStatus.all)
                {
                    b.go.SetActive(true);
                    b.go.transform.SetSiblingIndex(b.preferredPosition);
                }
            }
        }
    }

    public void StartNewGame()
    {
        //jne.
        backButton.SetActive(true);
    }

    public void LoadLevel()
    {
        backButton.SetActive(false);
        ChangeCurrentActiveMenu(MainMenuGrid);
        mmStatus = MMStatus.inGame;

        Loading = true;
    }

    public void OpenOptionsMenu()
    {
        optionsDefaultButton.SetActive(true);
        backButton.SetActive(true);
        ChangeCurrentActiveMenu(OptionMenu);
        OptionMenu.GetComponent<OptionsMenu>().LoadSettings();
    }

    public void OpenMainMenu()
    {
        if (curActiveMenu == OptionMenu) OptionsMenu.Instance.SaveOnTextFile();

        ChangeCurrentActiveMenu(MainMenuGrid);

        optionsDefaultButton.SetActive(false);
        backButton.SetActive(false);
    }

    public void OpenControlsMenu()
    {
        ChangeCurrentActiveMenu(ControlMenu);
        backButton.SetActive(true);
        ControlMenu.GetComponent<ControlsMenu>().Init();
    }

    public void LoadMainMenu()
    {
        EnableMenu = false;
        InGameMenu = false;
        ChangeCurrentActiveMenu(MainMenuGrid);
        mmStatus = MMStatus.main;
        Loading = true;
        //loadlevel hommeli
    }

    public void Quit()
    {
        Application.Quit();
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

        OpenButtons();
        ControlMenu.SetActive(true);
        ControlMenu.SetActive(false);
    }

    private void Update()
    {
        if(Loading)
        {
            EnableMenu = false;
            if (LoadUI == null) return;
            //LoadUI.SetActive(true);

            loadT += Time.deltaTime;

            if(loadT > 0.5f)
            {
                loadT = 0;

                if(index < 3)
                {
                    Loadtext.text += dot;
                    index++;
                }
                else
                {
                    Loadtext.text = "Loading";
                    index = 0;
                }
            }
        }
        else
        {
            EnableMenu = true;
            //LoadUI.SetActive(false);
        }

        //switch(mmStatus)
        //{
        //    case MMStatus.main:
        //        curActiveMenu.SetActive(EnableMenu);
        //        break;
        //    case MMStatus.inGame:
        //        curActiveMenu.SetActive((!Loading) ? InGameMenu : false);
        //        break;
        //    default:
        //        break;
        //}
    }
}
