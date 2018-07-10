using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookAbility : MonoBehaviour {


    private GameObject hookObj;
    [SerializeField] private GameObject hookPrefab = null;
    [SerializeField] private float hookDistance = 0f;
    [SerializeField] private float hookSpeed = 0f;
    //[SerializeField] private float slowDown = 0f;
    private bool hookShot = false;

    

    void Start ()
    {
		
	}
	


	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.E))
        {
            ShootHook();
        }
	}

    void ShootHook()
    {
        if (!hookShot)
        {
            hookObj = Instantiate(hookPrefab, transform.position, transform.rotation);
            hookObj.GetComponent<Hook>().SetValues(gameObject, transform.forward, hookSpeed);
            Invoke("ResetHook", hookDistance);
            hookShot = true;
        }
    }

    void ResetHook()
    {
        if (!hookObj.GetComponent<Hook>().hitTarget)
        {
            Destroy(hookObj);
            hookShot = false;
        }
    }

}
