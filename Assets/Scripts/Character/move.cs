using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class move : MonoBehaviour
{
    public float verticalSpeed = 2f;
    public float horizontalSpeed = 2f;
    public float speed = 2f;
    public float turnSpeed = 2f;
    public float rotateSpeed = 2f;
    public bool rolling;
    public bool hasjustRolled;
    public Quaternion myQuat, targetQuat;
    public float quatSpeed = 1f;
    public float stabilize = 0.1f;
    private Animator m_animator;
    public bool isMoving;
    public GameObject camera1;
    public GameObject camera2;
    public float velocity;
    Vector3 lastposition = Vector3.zero;
    public float restrictAngle = Mathf.Abs(80);
    public CameraController camerascript;


    void Start()
    {
        Quaternion myQuat = Quaternion.Euler(transform.localEulerAngles);
        Quaternion targetQuat = Quaternion.Euler(0, 0, 0);
        m_animator = gameObject.GetComponent<Animator>();
        isMoving = true;
        
        
    }


    void Update()
    {
        if(isMoving)
        {
            m_animator.SetBool("isMoving", true);
        }
        if (!isMoving)
        {
            m_animator.SetBool("isMoving", false);
        }
    }
    void FixedUpdate()
    {

        if (!CameraController.cam.freeCamera)
        {
            MouseMove();
        }
       
        
        Move();
        BarrelRoll();
        Restrict();
        //AFTER ROLL
        if (!rolling && hasjustRolled)
        {
            Stabilize();
            //Debug.Log("Stabilize");
        }
        //NORMAL STABILIZER
        if (!rolling && !hasjustRolled)
        {
            Stabilize();
            //Debug.Log("FastStabilize");
        }

        /*velocity = Mathf.Clamp(Mathf.Abs((transform.position - lastposition).magnitude), 0f, 1f);
        lastposition = transform.position;

        if (velocity > 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }*/
    }

    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
       float h = horizontalSpeed * Input.GetAxis("Mouse X");
        //float translation = Input.GetAxisRaw("Vertical") * speed;
// v nollaksi jos barrel roll
        transform.Rotate(v, h, 0);
        
        
    }

    public void Move()
    {
        isMoving = false;
        //BASIC MOVE
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //transform.Translate(Vector3.left * speed * Time.deltaTime);
            transform.Rotate(0, -turnSpeed, 0);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            //transform.Translate(Vector3.forward * speed * Time.deltaTime);
            transform.Rotate(0, turnSpeed, 0);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
            isMoving = true;
        }
        // ALTITUDE
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            camera1.SetActive(true);
            camera2.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            camera1.SetActive(false);
            camera2.SetActive(true);
        }
        

    }
    public void BarrelRoll()
    {
        //BARREL ROLL
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, 0, rotateSpeed);
            if (transform.rotation.z >= 180)
            {
                return;
            }
            rolling = true;

        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, 0, -rotateSpeed);
            rolling = true;
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            rolling = false;
            hasjustRolled = true;
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            rolling = false;
            hasjustRolled = true;
        }
    }
    public void Stabilize1()
    {
        if (transform.rotation.z < 0)
        {
            float z = transform.eulerAngles.z - 360;
            //Debug.Log(z);
            transform.Rotate(0, 0, -z * stabilize);

        }
        if (transform.rotation.z >= 0)
        {
            float z = transform.eulerAngles.z;
            //Debug.Log(z);
            transform.Rotate(0, 0, -z * stabilize);
        }

    }
    public void Stabilize2()
    {
        if (transform.rotation.z < -0.1)
        {
            transform.localRotation = Quaternion.RotateTowards(myQuat, targetQuat, quatSpeed);
            myQuat = Quaternion.Euler(transform.localEulerAngles);
            //Debug.Log("Quaternion1");


        }
        else if (transform.rotation.z > 0.1)
        {
            transform.localRotation = Quaternion.RotateTowards(myQuat, targetQuat, quatSpeed);
            myQuat = Quaternion.Euler(transform.localEulerAngles);
            //Debug.Log("Quaternion2");
        }

    }

    public void StabilizeFast()
    {
        if (transform.rotation.z < 0)
        {
            float z = transform.eulerAngles.z - 360;
            //Debug.Log(z);
            transform.Rotate(0, 0, -z);
        }
        if (transform.rotation.z >= 0)
        {
            float z = transform.eulerAngles.z;
            //Debug.Log(z);
            transform.Rotate(0, 0, -z);
        }

    }
    void Stabilize()
    {

        float z = transform.eulerAngles.z;
       // Debug.Log(z);
        transform.Rotate(0, 0, -z);



    }
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
