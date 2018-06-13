using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


    
public class Gamemanager : MonoBehaviour
{
    public delegate void AddScoreDelegate(int amount);
    public static event AddScoreDelegate increasescore;
    public static Gamemanager Instance;
    private int score;
    
    //keeps track of score of the players and the plant growing actions
    private void AddScore()
    {
        //score += PlayerScoreWhich is fetched somehow
    }

    private void Awake()
    {
        EventManager.AddHandler(EVENT.AddScore, AddScore);
    }
}
