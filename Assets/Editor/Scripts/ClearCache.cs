using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClearCache : MonoBehaviour
{
    [MenuItem("Examble/ClearCache")]
    public static void CleanCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("Successfully cleaned the cache.");
        }
        else
        {
            Debug.Log("Cache is being used.");
        }
    }
}
