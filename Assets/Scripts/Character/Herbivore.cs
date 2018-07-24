using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Herbivore : Character
{

    
    //object references
    [HideInInspector] public static Herbivore herbiv;
    [SerializeField] new GameObject  CameraClone;
    [SerializeField] GameObject Camera3rd;
    public bool mouseInput;
    Camera camCol;
    CameraController cc = new CameraController();

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

        if (Input.GetAxisRaw("Mouse Y") != 0 || Input.GetAxisRaw("Mouse X") != 0)
        {
            mouseInput = true;
        }
        else
            mouseInput = false;

    }
    
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        targetRotation = transform.rotation;
        base.Start();
        herbiv = this;
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
        
        if (!CameraClone.GetComponent<CameraController>().FreeCamera) //lock mouse move controls if looking around with camera
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
