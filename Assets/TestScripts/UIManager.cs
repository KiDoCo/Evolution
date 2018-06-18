using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private float UImatchTimer;

    public void UpdateCharacterExperience()
    {

    }

    private void DisplayTime()
    {
        UImatchTimer = Gamemanager.Instance.MatchTimer / 60;
    }

    private void Start()
    {
        EventManager.AddHandler(EVENT.UpdateExperience, UpdateCharacterExperience);
    }
}
