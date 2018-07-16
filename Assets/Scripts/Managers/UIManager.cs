using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject hud = null;
    [SerializeField] private GameObject pauseMenu = null;

    public static void switchGameObject(GameObject[] list, GameObject obj)
    {
        foreach (GameObject o in list)
        {
            if (o == obj)
                o.SetActive(true);
            else
                o.SetActive(false);
        }
    }

    //Match UI
    public void UpdateMatchUI(Character source)
    {
        if (HUDController.instance != null)
        {
            HUDController.instance.MaxHealth   = (int)source.Maxhealth;
            HUDController.instance.CurProgress = source.Experience;
            HUDController.instance.CurHealth   = (int)source.Health;
        }
    }

    public void InstantiateInGameUI()
    {
        Instantiate(hud);
        Instantiate(pauseMenu);
    }

    public void UpdateCharacterExperience()
    {

    }

    private void DisplayTime()
    {
        // Gamemanager.Instance.MatchTimer / 60;
    }

//-------------------------------------------------------------------------------------
    //Main Menu UI

    private void InstantiateMainMenuUI()
    {

    }

    private void MatchResultScreen(Character source)
    {

    }

    private void Awake()
    {
        Instance = this;
    }

    //unity methods
    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.Instance.UI.SetActive(!PauseMenu.Instance.UI.activeSelf);

            if (PauseMenu.Instance.UI.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
