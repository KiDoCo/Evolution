using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour {

	void Start ()
    {
        Debug.Log("MapCamera spawned");
        InGameManager.Instance.MapCamera = gameObject;
	}

}
