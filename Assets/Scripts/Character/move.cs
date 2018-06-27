using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Species { carnivore, herbivore};

public class move : MonoBehaviour
{
    public Quaternion        myQuat, targetQuat;
    public float             verticalSpeed   = 2f;
    public float             horizontalSpeed = 2f;
    public float             speed           = 2f;
    public float             turnSpeed       = 2f;
    public float             rotateSpeed     = 2f;
    public float             quatSpeed       = 1f;
    public float             stabilize       = 0.1f;
    public float             velocity;
    public float             restrictAngle   = Mathf.Abs(80);
    private float            health = 100;
    private float            experience = 0;
    private const float      healthMax       =100.0f;
    private const float      waitTime        = 1.0f;
    private bool             ready;
    public bool              rolling;
    public bool              hasjustRolled;
    public bool              isMoving;
    private bool             Eating;
    private Animator         m_animator;
    public  GameObject       camera1;
    public  GameObject       camera2;
    public  CameraController camerascript;
    private Vector3          lastposition    = Vector3.zero;
    private AudioSource      source;

    public float Maxhealth { get { return healthMax; } }
    public float Health { get { return health; } }
    public float Experience { get { return experience; } }


    public void CmdEat(IEatable eatObject)
    {

        if (Input.GetButton("Fire1"))
        {
            experience += eatObject.GetAmount();
            Debug.Log(experience);
            m_animator.SetBool("isEating", eatObject.Eaten);
            eatObject.Eaten = true;
            eatObject.DecreaseFood();
            if (!source.isPlaying)
            {
                EventManager.SoundBroadcast(EVENT.PlaySFX, eatObject.Source(), (int)SFXEvent.Eat);
            }
        }
        else
        {
            eatObject.Eaten = false;
            m_animator.SetBool("isEating", eatObject.Eaten);
        }
    }

    private void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, source, (int)SFXEvent.Hurt);
        health -= amount;
    }

    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    private void InteractionChecker()
    {
        for (int i = 0; Gamemanager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.FoodPlaceList[i].GetCollider().bounds))
            {
                Gamemanager.Instance.FoodPlaceList[i].Interact(this);
            }
        }
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

    private void Awake()
    {
        health = 100;
        source = GetComponentInChildren<AudioSource>();
    }

    void Start()
    {
        Quaternion myQuat = Quaternion.Euler(transform.localEulerAngles);
        Quaternion targetQuat = Quaternion.Euler(0, 0, 0);
        m_animator = gameObject.GetComponent<Animator>();
        isMoving = true;
        EventManager.SoundBroadcast(EVENT.PlayMusic, source, (int)MusicEvent.Ambient);
    }

    void Update()
    {
        InteractionChecker();
        if (isMoving)
        {
            m_animator.SetBool("isMoving", true);
        }
        if (!isMoving)
        {
            m_animator.SetBool("isMoving", false);
        }
        UIManager.Instance.InstantiateMatchUI(this);
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

}
