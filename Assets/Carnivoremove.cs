using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carnivoremove : MonoBehaviour
{
    public float Speed = 20f;
    public float AscendSpeed = 20f;
    public Vector3 InputVector;
    public Vector3 inputVectorY2;

    public bool isMoving;
    public float strafeSpeed = 5f;
    public float verticalSpeed = 1;
    public float horizontalSpeed = 1;
    //acceleration
    public float maxSpeed = 20f;
    public float timeZeroToMax = 2.5f;
    public float accelRatePerSecond;
    public float forwardVelocity = 0.2f;
    public float forwardVelocity2;
    public float forwardVelocity3;

    public bool start;
    public bool isMovingForward;
    public bool isAscending;

    public bool ascend;
    public Vector3 inputVectorZ;
    public Vector3 inputVectorY;
    public Rigidbody rb;
    public float thrust = 20f;
    public float gravity = 3f;
    public float jumpforce = 5f;
    public bool gravityOn;

    private void Awake()
    {
        accelRatePerSecond = maxSpeed / timeZeroToMax;
        forwardVelocity = 0f;
        Rigidbody rb = GetComponent<Rigidbody>();
    }


    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        ascend = true;
        start = true;
        
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        ForwardMovement();
        //Force();
        Altitude();
        Strafe();
        MouseMove();
        Gravity();
        Stabilize();
    }

    void ForwardMovement()
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            forwardVelocity2 = Mathf.Clamp(forwardVelocity, 0.0001f, 1);
            //forwardVelocity2 = Mathf.Log(forwardVelocity) * 3;

            forwardVelocity2 = Mathf.Exp(forwardVelocity);
            //forwardVelocity = Mathf.Exp(forwardVelocity);
            




            forwardVelocity += accelRatePerSecond * Time.deltaTime;
            forwardVelocity = Mathf.Min(forwardVelocity, maxSpeed);
            isMovingForward = true;
        }
        else if (Input.GetAxisRaw("Vertical") == 0)
        {
            forwardVelocity = 0;
            forwardVelocity2 = 0;

        }
        else
            isMovingForward = false;

        if (isMovingForward)
        {
            inputVectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * forwardVelocity) * Time.deltaTime;
            Vector3 input = inputVectorY;
            transform.Translate(input);
        }
        if (!isMovingForward)
        {
            inputVectorY2 = (Input.GetAxisRaw("Vertical") * Vector3.forward * Speed) * Time.deltaTime;
            transform.Translate(inputVectorY2);
        }
    }
    void Altitude()
    {
        Vector3 inputVectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;
        transform.Translate(inputVectorZ);
        if (inputVectorZ.magnitude != 0)
        {
            isAscending = true;

        }
        else
            isAscending = false;
    }

    protected void Stabilize()
    {

            float z = transform.eulerAngles.z;
            // Debug.Log(z);
            transform.Rotate(0, 0, -z);
       

    }
    void Gravity()
    {
        if (gravityOn)
        {
            AscendSpeed = -gravity * Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
               // AscendSpeed = jumpforce;
            }
            else
            {
                AscendSpeed -= gravity * Time.deltaTime;
            }
            Vector3 moveVector = new Vector3(0, AscendSpeed, 0);
            transform.Translate(moveVector);

        }


    }

    
    void Force()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            rb.velocity = Vector3.zero;
            rb.AddRelativeForce(new Vector3 (0, thrust));
        }
        else
        {
            rb.AddRelativeForce(new Vector3 (0, 0, 0));
        }
            
        if (Input.GetKeyDown(KeyCode.S))
        {
            rb.AddRelativeForce(-Vector3.up * thrust);
        }

       
    }
    void Strafe()
    {


        Vector3 inputStrafeZ = new Vector3(1, 0, 0) * (Input.GetAxisRaw("Horizontal") * strafeSpeed) * Time.deltaTime;
        transform.Translate(inputStrafeZ);
        if (inputStrafeZ.magnitude != 0)
        {

            isMoving = true;
        }
        else
        {
            isMoving = false;
        }


    }

    void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);
    }

    void Accelerate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            forwardVelocity += accelRatePerSecond * Time.deltaTime;
            forwardVelocity = Mathf.Min(forwardVelocity, maxSpeed);


            Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * forwardVelocity) * Time.deltaTime;
            
            InputVector = inputvectorY;

          
                transform.Translate(inputvectorY);
          
            
        }

    }
   

   
}


