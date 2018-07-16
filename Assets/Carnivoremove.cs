using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carnivoremove : MonoBehaviour
{
    public float Speed = 20f;
    public float AscendSpeed = 20f;
    public Vector3 InputVector;
    public bool isMoving;
    public float strafeSpeed = 5f;
    public float verticalSpeed = 1;
    public float horizontalSpeed = 1;
    //acceleration
    public float maxSpeed = 20f;
    public float timeZeroToMax = 2.5f;
    public float accelRatePerSecond;
    public float forwardVelocity;

    private void Awake()
    {
        accelRatePerSecond = maxSpeed / timeZeroToMax;
        forwardVelocity = 0f;
    }


    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
        Accelerate();
        ForwardMovement();
        Strafe();
        MouseMove();
    }

    void ForwardMovement()
    {

        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * forwardVelocity) * Time.deltaTime;
        //Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;
        InputVector = inputvectorY; // + inputvectorZ;

        transform.Translate(InputVector);
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
        if (Input.GetKey(KeyCode.W))
        {
            forwardVelocity += accelRatePerSecond * Time.deltaTime;
            forwardVelocity = Mathf.Min(forwardVelocity, maxSpeed);


            Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * forwardVelocity) * Time.deltaTime;
            Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;
            InputVector = inputvectorY + inputvectorZ;

            transform.Translate(inputvectorY);
        }

    }

    private void LateUpdate()
    {
        
    }


}


