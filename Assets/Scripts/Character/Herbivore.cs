﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Herbivore : Character
{


    Quaternion originZ;
    Quaternion currentZ;
    private bool Iseating;
    //objects
    public Transform myT;
    [SerializeField] private GameObject cameraClone;
    
    //object references
    [HideInInspector] public static Herbivore herbiv;

    //eulermeter values
    [HideInInspector] public float y;
    
    public GameObject CameraClone { get { return cameraClone; } }

    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);
                
    }
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();        
        herbiv = this;
        cameraClone = Instantiate(Gamemanager.Instance.CameraPrefab, transform.position, Quaternion.identity);
        cameraClone.name = "FollowCamera";
        CameraClone.GetComponent<CameraController>().InstantiateCamera(this);
    }

    protected override void Update()
    {
        base.Update();
        
        if(isMoving)
        {
            m_animator.SetBool("isMoving", true);
        }
        if (!isMoving)
        {
            m_animator.SetBool("isMoving", false);
        }
        m_animator.SetBool("isEating", Iseating);

        if(Input.GetKey(KeyCode.N))
        {
            Iseating = true;
        }
        else
        {
            Iseating = false;
        }
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //CameraClone.GetComponent<CameraController>().FreeCamera();
        MouseMove();
       
        Restrict();

        if (!rolling) return;

        Stabilize();
    }
    
}
