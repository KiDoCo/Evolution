using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public void UpdateCharacterExperience()
    {

    }

    private void DisplayTime()
    {
        // Gamemanager.Instance.MatchTimer / 60;
    }

    private void Start()
    {
        EventManager.ActionAddHandler(EVENT.UpdateExperience, UpdateCharacterExperience);
    }
}
