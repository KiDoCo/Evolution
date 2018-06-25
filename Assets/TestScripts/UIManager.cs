using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //Match UI
    private void InstantiateMatchUI()
    {

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

    //unity methods
    private void Start()
    {
        EventManager.ActionAddHandler(EVENT.UpdateExperience, UpdateCharacterExperience);
    }
}
