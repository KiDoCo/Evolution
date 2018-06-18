using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Gamemanager : NetworkBehaviour
{
    public static Gamemanager Instance;
    [SyncVar]
    private int score;
    [SyncVar]
    private float matchTimer;

    public List<IEatable> FoodPlaceDictionary = new List<IEatable>();

    public float MatchTimer
    {
        get
        {
            return matchTimer;
        }

        set
        {
            matchTimer = value;
        }
    }

    //keeps track of score of the players and the plant growing actions
    public void AddScore(int amount, Test test)//Character person
    {
        //player.score += amount
    }
    
    private void UpdateExperience()//Character person
    {
        //UIManager.Instance.UpdateCharacterExperience();
    }

    private void IncreaseFoodOnSources()
    {
        EventManager.Broadcast(EVENT.Increase);
    }

    private void Update()
    {
    }
    private void Awake()
    {
        Instance = this;
        EventManager.AddHandler(EVENT.UpdateExperience, UpdateExperience);
    }

    private void Start()
    {
        InvokeRepeating("IncreaseFoodOnSources", 2.0f, 2.0f);
    }
}
