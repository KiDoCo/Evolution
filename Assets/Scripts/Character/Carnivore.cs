using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Carnivore : Character
{

    //objects
    [SerializeField] new GameObject camera;

    
    //script references
    [HideInInspector] public static Carnivore carniv;

    //eulermeter values
    [HideInInspector] public float y;
    public bool barrelRoll;
    public float dashSpeed;
    public float strafeSpeed;
    //dash
    [SerializeField] protected bool dashing;
    [SerializeField] protected bool canDash;
    [SerializeField] protected float dashSpeedValue = 100f;
    [SerializeField] protected float staminaValue = 100f;
    [SerializeField] protected float stamina = 100f;
    [SerializeField] protected float staminaDecrement = 2f;
    [SerializeField] protected float staminaIncrement = 2f;

    private bool isEating;
    private bool isCharging;


    protected override void Start()
    {
        base.Start();
        m_animator = gameObject.GetComponent<Animator>();
        carniv = this;
        barrelRoll = false;
        stamina = staminaValue;
        camera.GetComponent<CameraController>().InstantiateCamera(this);
    }

    protected override void Update()
    {
        base.Update();
        if (stamina < staminaValue && !dashing)
        {
            stamina += staminaIncrement * Time.deltaTime;
            if (stamina > staminaValue)
            {
                stamina = staminaValue;
            }

        }
        if (stamina > 0 && dashing)
        {
            stamina -= staminaDecrement * Time.deltaTime;
            if (stamina < 0)
            {
                stamina = 0;
            }
        }
        if (stamina > 0)
        {
            canDash = true;
            dashSpeed = dashSpeedValue;

        }
        else if (stamina <= 0)
        {
            canDash = false;
            dashSpeed = Speed;
            dashing = false;
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            isEating = true;
        }
        else
        {
            isEating = false;
        }

        if(Input.GetKey(KeyCode.T))
        {
            isCharging = true;
        }
        else
        {
            isCharging = false;
        }

    }


    protected override void AnimationChanger()
    {
        m_animator.SetBool("IsMoving", isMoving);
        m_animator.SetBool("IsEating", isEating);
        m_animator.SetBool("IsCharging", isCharging);
    }

    protected override void FixedUpdate()
    {

        base.FixedUpdate();
        Strafe();
        Dash();
        MouseMove();
        AnimationChanger();
        isMoving = false;


        //if (!cameraClone.GetComponent<CameraController>().freeCamera)
        //{
        //    MouseMove();
        //}

    }

    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");

        if(v != 0 || h != 0)
        {
            isMoving = true;
        }
            m_animator.SetFloat("FloatX", Mathf.Clamp01(h) + Input.GetAxis("Horizontal"));
            m_animator.SetFloat("FloatY", Mathf.Clamp01(v) + Input.GetAxis("Vertical"));
            transform.Rotate(v, h, 0);
    }

    public void Strafe()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * strafeSpeed * Time.deltaTime);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * strafeSpeed * Time.deltaTime);
        }
    }


    public void Dash()
    {

        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            transform.Translate(Vector3.forward * dashSpeed * Time.deltaTime);
            dashing = true;
            dashSpeed = dashSpeedValue;
        }
        if (Input.GetKey(KeyCode.LeftShift) && stamina <= 0)
        {
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
            Debug.Log("Out of stamina");

        }


    }
}






