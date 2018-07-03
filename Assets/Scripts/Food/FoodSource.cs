using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSource : MonoBehaviour
{
    private List<Transform> berryLocations = new List<Transform>();


    private void Start()
    {
        GameObject go = new GameObject();
        go.name = "Berries";
        berryLocations.AddRange(transform.GetChild(1).GetComponentsInChildren<Transform>());
        for(int i = 0; i < berryLocations.ToArray().Length; i++)
        {
            GameObject clone = Instantiate(Gamemanager.Instance.BerryPrefab, berryLocations[i].position, Quaternion.identity);
            clone.transform.parent = go.transform;
            clone.name = "berry";
        }

        for(int i = 0; i < berryLocations.ToArray().Length; i++)
        {
            Destroy(berryLocations[i].gameObject);
        }
    }


}
