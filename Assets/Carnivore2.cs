using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Carnivore2 : Character
{


    //object references
    [HideInInspector] public static Carnivore2 carniv;
    

    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);

    }
    protected override void Awake()
    {
        base.Awake();
        canStrafe = true;
        canTurn = false;
        canDash = true;
    }

    protected override void Start()
    {
        base.Start();
        carniv = this;
        
        
    }

    protected override void Update()
    {
        base.Update();

        if (isMoving)
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
        MouseMove();
        Restrict();
        Stabilize();

    }

}
