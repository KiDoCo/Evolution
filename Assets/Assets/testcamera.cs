using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testcamera : MonoBehaviour
{
    [SerializeField] Vector3 defaultDistance = new Vector3(0f, 2f, -10f);
    public float RotationsSpeed;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	
	
    {
        if (Input.GetMouseButton(2))
        {
            Quaternion camTurnAngle = Quaternion.AngleAxis(2 * RotationsSpeed, Vector3.up);
            defaultDistance = camTurnAngle * defaultDistance;
        }
       
    }
}
