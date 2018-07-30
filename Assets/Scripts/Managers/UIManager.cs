﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject hud = null;
    [SerializeField] private GameObject helix;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject matchResultScreen;
    [SerializeField] private GameObject helixCamera;
    
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


    private void InstantiateMainMenuUI()
    {

    } //Eh

    #region MatchUI


    public void InstantiateInGameUI(Character source)
    {
        Instantiate(hud);
        Instantiate(pauseMenu);

        if (source.GetType() == typeof(Herbivore))
        {
            GameObject clone = Instantiate(helix, GameObject.Find("HelixLocation").transform.position, Quaternion.identity);
            HUDController.Instance.Inst(clone);
            GameObject cameraClone = Instantiate(helixCamera, GameObject.Find("CameraLocation").transform.position, Quaternion.identity);
            cameraClone.transform.rotation = Quaternion.Euler(90, 180, 0);
        }
    }

    public void UpdateMatchUI(Herbivore source)
    {
        HUDController.Instance.MaxHealth = (int)source.Maxhealth;
        HUDController.Instance.CurProgress = source.Experience;
        HUDController.Instance.CurHealth = (int)source.Health;
    }

    /// <summary>
    /// Calls the hudcontroller to print the desired screen
    /// </summary>
    /// <param name="source"></param>
    public void MatchResultScreen(Character source)
    {
        if(source.GetType() == typeof(Herbivore))
        {
            var a = source.GetComponent<Herbivore>();
            HUDController.Instance.ResultScreen(a.Experience, a.Deathcount, InGameManager.Instance.MatchTimer, a.SurTime, true);
        }
        else if(source.GetType() == typeof(Carnivore))
        {
            var a = source.GetComponent<Carnivore>();
            HUDController.Instance.ResultScreen(a.KillCount, a.KillCount,InGameManager.Instance.MatchTimer,0, false);
        }
    }

    #endregion

    #region UnityMethods

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideCursor(false);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == NetworkGameManager.Instance.playScene && !Gamemanager.Instance.MatchEnd)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu.Instance.UI.SetActive(!PauseMenu.Instance.UI.activeSelf);

                if (PauseMenu.Instance.UI.activeSelf)
                {
                    NetworkGameManager.Instance.LocalCharacter.InputEnabled = false;
                    HideCursor(false);
                }
                else
                {
                    NetworkGameManager.Instance.LocalCharacter.InputEnabled = true;
                    HideCursor(true);
                }
            }
        }
    }

    public void HideCursor(bool hide)
    {
        Cursor.lockState = hide ? CursorLockMode.Locked : CursorLockMode.None; 
        Cursor.visible = !hide;
    }

    #endregion
}
