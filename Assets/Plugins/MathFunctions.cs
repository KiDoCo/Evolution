using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains calculation functions 
/// </summary>
public static class MathFunctions
{
    /// <summary>
    /// Calculates the scaling of an box object which is a child 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="transform"></param>
    /// <param name="position"></param>
    /// <param name="amount"></param>
    public static void BoxScaleObject(Transform parent,Transform transform,Vector3 position, float amount)
    {
        Vector3 scale = new Vector3(amount, amount, amount);
        transform.localScale = scale;
        transform.position = parent.position + (position * scale.y );
    }
}

