using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Herbivore : Character
{


    Quaternion originZ;
    Quaternion currentZ;
    [SerializeField] new GameObject camera;

   
   
    
    //objects
    private Animator m_animator;
    public Transform myT;
    
            

    //object references
    [HideInInspector] public CameraController camerascript;
    [HideInInspector] public static Herbivore herbiv;

    //eulermeter values
    [HideInInspector] public float y;
    

    protected override void Start()
    {
        base.Start();        
        m_animator = gameObject.GetComponent<Animator>();        
        herbiv = this;
        camera.GetComponent<CameraController>().ConstructCamera(this);
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
        if (!CameraController.Cam.FreeCamera)
        {
            MouseMove();
        }

       
        
       
        Restrict();

        //STABILIZE Z 
        if (!rolling && !hasjustRolled)
        {
            //Stabilize();

        }
        if (!rolling && hasjustRolled)
        {
            //will be perhaps something?
        }
        
    }

    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);
                
    }

    
   
  
    

    /// <summary>
    /// Avoid control jerkiness with ristricting x rotation
    /// </summary>
    void Restrict()
    {
        if (transform.rotation.x > 15)
        {
            float x = transform.eulerAngles.x;
            transform.Rotate(-x, 0, 0);
        }

        if (transform.rotation.x < -15)
        {
            float x = transform.eulerAngles.x;
            transform.Rotate(-x, 0, 0);
        }
       
    }

    
    
}
