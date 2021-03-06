﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EndLine : NetworkBehaviour {

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Herbivore"))
        {
            Herbivore p = other.gameObject.GetComponent<Herbivore>();

            if (p.Experience == 100)
            {
                Debug.Log("Herbivore won!");
                InGameManager.Instance.EndMatchForPlayer(p);
            }
        }
    }
}
