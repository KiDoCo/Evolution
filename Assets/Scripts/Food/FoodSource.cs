using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FoodSource : NetworkBehaviour
{
    private List<Transform> berryLocations = new List<Transform>();


    private void Start()
    {
        if (isServer)
        {
            GameObject go = new GameObject();
            go.name = "Berries";
            for (int i = 0; i < transform.GetChild(0).childCount; i++)
            {
                if (transform.GetChild(0).GetChild(i).tag == "BerrySpawn")
                {
                    berryLocations.Add(transform.GetChild(0).GetChild(i).GetComponent<Transform>());

                }
            }
            for (int i = 0; i < berryLocations.ToArray().Length; i++)
            {
                GameObject clone = Instantiate(InGameManager.Instance.BerryPrefab, berryLocations[i].position, Quaternion.identity);
                clone.transform.parent = go.transform;
                NetworkServer.Spawn(clone);
                clone.name = "berry";
            }

            for (int i = 0; i < berryLocations.ToArray().Length; i++)
            {
                Destroy(berryLocations[i].gameObject);
            }
        }
    }


}
