using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //Match UI
    public void InstantiateMatchUI(move source)
    {
        HUDController.instance.MaxHealth   = (int)source.Maxhealth;
        HUDController.instance.CurProgress = source.Experience;
        HUDController.instance.CurHealth =   (int)source.Health;
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

    private void Awake()
    {
        Instance = this;
    }

    //unity methods
    private void Start()
    {
        EventManager.ActionAddHandler(EVENT.UpdateExperience, UpdateCharacterExperience);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
        Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
