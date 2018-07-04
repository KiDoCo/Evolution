using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Herbivore : Character
{


    Quaternion originZ;
    Quaternion currentZ;

    //objects
    public Transform myT;
    
    //object references
    [HideInInspector] public static Herbivore herbiv;

    //eulermeter values
    [HideInInspector] public float y;
    

    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);
                
    }

    protected override void Start()
    {
        base.Start();        
        herbiv = this;
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
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        CameraClone.GetComponent<CameraController>().FreeCamera();
        MouseMove();
       
        Restrict();

        if (!rolling) return;

        Stabilize();
    }
    
}
