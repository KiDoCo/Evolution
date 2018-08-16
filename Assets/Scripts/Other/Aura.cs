using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : MonoBehaviour {

    [SerializeField] private AuraAPI.AuraVolume[] volumeScripts = null;
    [SerializeField] private AuraAPI.AuraLight[] lightScripts = null;

	void Start ()
    {
		foreach (AuraAPI.AuraVolume s in volumeScripts)
        {
            s.enabled = true;
        }

        foreach (AuraAPI.AuraLight s in lightScripts)
        {
            s.enabled = true;
        }
    }
}
