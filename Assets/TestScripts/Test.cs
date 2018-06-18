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

    public void CmdEat(IEatable eatObject)
    {
        if (Input.GetKey(KeyCode.E))
        {
            StartCoroutine(Eat(eatObject));
        }
    }

    /// <summary>
    /// Coroutine which will be triggered every second
    /// </summary>
    /// <param name="eatObject"></param>
    /// <returns></returns>
    IEnumerator Eat(IEatable eatObject)
    {
        if (Eating) yield break;

        Eating = true;
        experience += eatObject.GetAmount();
        eatObject.DecreaseFood();
        EventManager.Broadcast(EVENT.UpdateExperience);
        Debug.Log(eatObject.AmountFood);
        yield return new WaitForSeconds(1.0f);
        Eating = false;
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
