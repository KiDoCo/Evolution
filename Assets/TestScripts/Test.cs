using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Test : MonoBehaviour
{ 
    //ADD necessary components to playable character
    private bool ready;
    private bool Eating;
    private float health;
    private float experience;
    private const float waitTime = 1.0f;
    private AudioSource source;
    //ADD
    public void CmdEat(IEatable eatObject)
    {
        if (Input.GetKey(KeyCode.E))
        {
            StartCoroutine(Eat(eatObject));
        }
    }
    //ADD
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
        EventManager.SoundBroadcast(EVENT.PlaySFX, source, (int)SFXEvent.Eat);
        yield return new WaitForSeconds(waitTime);
        Eating = false;
    }
    //ADD
    private void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, source, (int)SFXEvent.Hurt);
        health -= amount;
    }
    //ADD
    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    private void InteractionChecker()
    {
        for (int i = 0; Gamemanager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.FoodPlaceList[i].GetCollider().bounds))
            {
                Gamemanager.Instance.FoodPlaceList[i].Interact(this);
            }
        }
    }

    private void Death()
    { 

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
