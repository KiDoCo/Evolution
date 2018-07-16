using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnivoreEat : MonoBehaviour {

    [SerializeField] private float eatCooldown;
    [SerializeField] private Carnivore carnivore;
    private bool eatReady = true;
    
    [SerializeField] private float xpReward;
    [SerializeField] private float damage;
    [SerializeField] [Range(0, 1)] private float slowDown;

	void Start ()
    {
        slowDown = 1 - slowDown;
	}
	

    private void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Player" && Input.GetButtonDown("Fire1") && eatReady)
        {
            Debug.Log("mums, mums....");
            StartCoroutine(EatCoolDown());
            carnivore.EatHerbivore(xpReward, slowDown);
            col.gameObject.GetComponent<Herbivore>().GetEaten(damage);
        }
    }

    private IEnumerator EatCoolDown()
    {
        eatReady = false;
        yield return new WaitForSeconds(eatCooldown);
        carnivore.RestoreSpeed();
        eatReady = true;
    }
}
