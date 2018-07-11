using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Carnivore : Character
{
    //bools
    public bool IsCharging;
    [SerializeField] protected bool canCharge;
    [SerializeField] protected float chargeSpeed = 50f;

    [SerializeField] protected bool canMouseMove = true;
    [SerializeField] protected float chargeTime = 2f;
    [SerializeField] protected float chargeCoolTime = 6f;
    
    //objects 
    [SerializeField] GameObject Camera1;

    public void MouseMove()
    {
        if (!PauseMenu.Instance.UI.activeSelf)
        {
            if (canMouseMove)
            {
                float v = verticalSpeed * Input.GetAxis("Mouse Y");
                float h = horizontalSpeed * Input.GetAxis("Mouse X");
                transform.Rotate(v, h, 0);
            }
        }

    }
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        if (isLocalPlayer)
        {
            base.Start();

            GameObject cam = Instantiate(Camera1); // Instantioi cameran ja pistää cam muuttujaksi

            cam.GetComponent<CameraController_1stPerson>().target = this.transform; //pääsee käsiksi koodiin ja kohteeseen

            UIManager.Instance.InstantiateInGameUI();
        }
    }


    protected override void Update()
    {
        if (isLocalPlayer)
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
    }
    protected override void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            base.FixedUpdate();
            MouseMove();
            Restrict();
            Stabilize();
            Strafe();

            //Charge();
        }
    }


    protected virtual void Strafe()//For carnivores?
    {
        if (canStrafe) //bools are checked/unchecked in editor
        {

            Vector3 inputStrafeZ = new Vector3(1, 0, 0) * (Input.GetAxisRaw("Horizontal") * strafeSpeed) * Time.deltaTime;
            transform.Translate(inputStrafeZ);
            if (inputStrafeZ.magnitude != 0)
            {
                isStrafing = true;
                isMoving = true;
            }
            else
            {
                isStrafing = false;
            }


        }
    }

    /// <summary>
    /// IN TESTING
    /// </summary>
    public void Charge() 
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
    } 


    public IEnumerator ChargeTimer() //Charge uses this
    {
        timerStart = true;
        yield return new WaitForSeconds(chargeTime);

        canCharge = false;
        canMouseMove = false;
        timerStart = false;
        yield return StartCoroutine(CoolTimer());


    }
   
    IEnumerator CoolTimer() 
    {
        canCharge = false;
        coolTimer = true;
        yield return new WaitForSeconds(coolTime);
        canCharge = true;
        coolTimer = false;
        canMouseMove = true;

    }

}
