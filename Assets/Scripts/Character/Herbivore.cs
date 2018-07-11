using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Herbivore : Character
{

    
    //object references
    [HideInInspector] public static Herbivore herbiv;
    [SerializeField] new GameObject  CameraClone;
    [SerializeField] GameObject Camera3rd;

    Quaternion targetRotation;
    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }
    
 
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
        targetRotation = transform.rotation;
        base.Start();

        GameObject cam = Instantiate(Camera3rd);
        cam.GetComponent<CameraController>().target = this.transform;
        canBarrellRoll = true;
        canTurn = true;
        //CameraClone.GetComponent<CameraController>().InstantiateCamera(this);
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
        
        if (!CameraClone.GetComponent<CameraController>().FreeCamera) 
        {
            MouseMove();
        }
        
       
        Restrict();

        if (!rolling)
        {
           Stabilize();
        }

        
    }
    
}
