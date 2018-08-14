using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorMovement : MonoBehaviour
{
    public float amplitude;
    public float frequency;

	void Update ()
    {
        transform.position += amplitude * (Mathf.Sin(2 * Mathf.PI * frequency * Time.time) - Mathf.Sin(2 * Mathf.PI * frequency * (Time.time - Time.deltaTime))) * transform.forward;
    }
}
