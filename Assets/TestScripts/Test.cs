using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Test : MonoBehaviour
{

    private bool ready;
    private bool Eating;
    private float health;
    private float experience;
    private AudioSource source;

    public void CmdEat(IEatable eatObject)
    {
        if (Input.GetKey(KeyCode.E))
        {
            StartCoroutine(Eat(eatObject));
        }
    }

    /// <summary>
    /// Player Eating coroutine
    /// </summary>
    /// <param name="eatObject"></param>
    /// <returns></returns>
    IEnumerator Eat(IEatable eatObject)
    {
        if (Eating) yield break;

        Eating = true;
        experience += eatObject.GetAmount();
        eatObject.DecreaseFood();
        EventManager.SoundBroadcast(EVENT.Eat, source);
        yield return new WaitForSeconds(1.0f);
        Eating = false;
    }

    private void TakeDamage(float amount)
    {
        Debug.Log("Health");
        EventManager.SoundBroadcast(EVENT.PlayerHit, source);
        health -= amount;
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

    //baseclass methods

    private void Awake()
    {
        health = 100;
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        InteractionChecker();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(20);
        }
    }
}
