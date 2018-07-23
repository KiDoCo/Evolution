using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndConditions : MonoBehaviour {

    [SerializeField] private List<Herbivore> herbivores;
    [SerializeField] private float roundTime;




    public void CheckEndConditions()
    {
        
        //Check if herbivores are enabled
        foreach (Herbivore h in herbivores)
        {
            if (h.enabled) { return; }
        }
        //Check timer
        if (Gamemanager.Instance.MatchTimer < roundTime)
        {
            return;
        }
        //Game end
        Debug.Log("GAME ENDED");
    }

}
