using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Test : MonoBehaviour
{
    public int Score { get; set; }
    private bool ready;
    private float health;
    private float experience;

    public void CmdEat(IEatable asdf)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Broadcast(EVENT.Eat);
            EventManager.Broadcast(EVENT.UpdateExperience);
        }
    }

    private void TakeDamage()
    {
    }

    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    private void InteractionChecker()
    {
        for (int i = 0; Gamemanager.Instance.FoodPlaceDictionary.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.FoodPlaceDictionary[i].GetCollider().bounds))
            {
                Gamemanager.Instance.FoodPlaceDictionary[i].Interact(this);
            }
        }
    }

    private void Awake()
    {
        EventManager.AddHandler(EVENT.PlayerHit, TakeDamage);
    }
    private void Update()
    {
        InteractionChecker();
    }
}
