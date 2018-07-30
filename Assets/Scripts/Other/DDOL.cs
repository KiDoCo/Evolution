using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDOL : MonoBehaviour {

    //Used to prevent this object from being destroyed upon scene change
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
