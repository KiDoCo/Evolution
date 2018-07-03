using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookAbility : MonoBehaviour {


    private GameObject hookObj;
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private float hookDistance;
    [SerializeField] private float hookSpeed;
    [SerializeField] private float slowDown;
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
