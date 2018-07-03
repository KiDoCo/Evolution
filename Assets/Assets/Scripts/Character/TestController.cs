using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestController : MonoBehaviour
{
    public Transform Character;
    public Vector3 offset;
    public Quaternion rotation;
    // Use this for initialization
    void Start()
    {
        //offset = transform.position - Character.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
       
        rotation = Character.transform.rotation;
        transform.rotation = rotation;
        transform.position = Character.transform.position + offset;
        //Offset: 0, 1.3, -5.169985
    }
    
}
