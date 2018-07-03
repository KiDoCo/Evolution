using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBlender : MonoBehaviour {

    private SkinnedMeshRenderer sr;
    private float XD;

    public float XD1
    {
        get
        {
            return XD;
        }

        set
        {
            XD = Mathf.Clamp(value, 0, 100);
        }
    }

    // Use this for initialization
    void Start () {
        sr = GetComponent<SkinnedMeshRenderer>();
	}
	
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.RightArrow))
        {
            XD1++;
        }

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            XD1--;
        }

        sr.SetBlendShapeWeight(0, XD1);
    }
}
