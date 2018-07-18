using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Carnivore : Character
{

    public bool IsCharging;
    [SerializeField] protected bool canCharge;
    [SerializeField] protected float chargeSpeed = 50f;

    [SerializeField] new GameObject camera;
    [SerializeField] protected bool canMouseMove = true;
    [SerializeField] protected float chargeTime = 2f;
    [SerializeField] protected float chargeCoolTime = 6f;
    public float stamina;
    private bool isEating;
    private bool isCharging;
    private const float staminaValue = 20.0f;
    private Vector3 inputStrafeZ;
    public Vector3  InputStrafeZ;

    {
        get
        {
            return inputStrafeZ;
        }
    }

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();
        m_animator = gameObject.GetComponent<Animator>();
        barrelRoll = false;
        stamina = staminaValue;
        camera.GetComponent<CameraController>().InstantiateCamera(this);
        cameraClone = Instantiate(Gamemanager.Instance.CameraPrefab, transform.position, Quaternion.identity);
        cameraClone.name = "FollowCamera";
    }

    protected override void Update()
    {
        base.Update();

        if (stamina > 0)
        {
            canDash = true;
        }

        else if (stamina <= 0)
        {
            canDash = false;
            dashSpeed = Speed;
        }


        if (Input.GetKeyDown(KeyCode.H))
        {
            isEating = true;
        }
        else
        {
            isEating = false;
        }

        if (Input.GetKey(KeyCode.T))
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
        MouseMove();
        Restrict();
        Stabilize();
        Strafe();
        Dash();
        MouseMove();
        AnimationChanger();
        isMoving = false;
        Charge();
    }

    protected virtual void Strafe()//For carnivores?
    {
        if (canStrafe) //bools are checked/unchecked in editor
        {
            Vector3 inputStrafeZ = new Vector3(1, 0, 0) * (Input.GetAxisRaw("Horizontal") * strafeSpeed) * Time.deltaTime;
            transform.Translate(inputStrafeZ);
        }
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
        if (canMouseMove) return;

        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");

        if (v != 0 || h != 0)
        {
            isMoving = true;
        }
        m_animator.SetFloat("FloatX", Mathf.Clamp01(h) + InputManager.Instance.GetAxis("Horizontal"));
        m_animator.SetFloat("FloatY", Mathf.Clamp01(v) + InputManager.Instance.GetAxis("Vertical"));
        transform.Rotate(v, h, 0);
    }

    /// <summary>
    /// IN TESTING
    /// </summary>
    private void Charge()
    {

        if (canCharge) //ability check
        {
            if (!IsCharging)
            {
                canMouseMove = true;
                // CameraController_1stPerson.cam1.m_FieldOfView = CameraController_1stPerson.cam1.FOVValue; //reset CAM FOV in camerascipt
            }

            if (IsCharging) // put down mousecontrols when charging
            {
                canMouseMove = false;
                // CameraController_1stPerson.cam1.m_FieldOfView += 60f;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift)) //charge function
            {

                Vector3 inputVectorX = new Vector3(0, 0, chargeSpeed) * Time.deltaTime;
                transform.Translate(inputVectorX);
                if (inputVectorX.magnitude != 0)
                {
                    IsCharging = true;
                    isMoving = true;
                    StartCoroutine(ChargeTimer());
                }
            }
            else
            {
                IsCharging = false;
                StopAllCoroutines();
            }
        }
        else
        {
            isCharging = false;
            StopAllCoroutines();
        }
    }

public void EatHerbivore(float xp, float slow)
    {
        speed *= slow;
        Experience += xp;
    }




    public void RestoreSpeed()
    {
        speed = SpeedValue;
    }    public IEnumerator ChargeTimer() //Charge uses this
    {
        timerStart = true;
        yield return new WaitForSeconds(chargeTime);

        canCharge = false;
        canMouseMove = false;
        timerStart = false;
        yield return StartCoroutine(CoolTimer());


    }

}

