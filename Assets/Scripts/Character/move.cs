using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Species { carnivore, herbivore};

public class move : MonoBehaviour
{
    //public Quaternion        myQuat, targetQuat;
    public float             verticalSpeed     = 2f;
    public float             horizontalSpeed   = 2f;
    public float             speed             = 2f;
    public float             turnSpeed         = 2f;
    public float             rotateSpeed       = 2f;
    public float             quatSpeed         = 1f;
    public float             stabilize         = 0.1f;
    public float             velocity;
    public float             restrictAngle     = Mathf.Abs(80);
    private float            health            = 100;
    private float            experience        = 0;
    private const float      healthMax         = 100.0f;
    private const float      waitTime          = 1.0f;
    private const float      experiencePenalty = 25.0f;
    private const float      deathpenaltytime  = 2.0f;
    private bool             ready;
    public bool              rolling;
    public bool              isMoving;
    private bool             eating;
    private Animator         m_animator;
    public  CameraController camerascript;
    //private Vector3          lastposition      = Vector3.zero;
    private Vector3          MovementInputVector;
    private Vector3          rotationInputVector;
    private AudioSource      musicSource;
    private AudioSource      SFXsource;
    private GameObject       cameraClone;

    public float Maxhealth { get { return healthMax; } }
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            if(health <= 0)
            {
                Death();
                health = Maxhealth;
            }
        }
    }
    public float Experience
    {
        get
        {
            return experience;
        }
        set
        {
            experience = Mathf.Clamp(value, 0, 100);
        }
    }
    public GameObject CameraClone { get { return cameraClone; } }


    /// <summary>
    /// Takes care of the eating for the player
    /// </summary>
    /// <param name="eatObject"></param>
    public void CmdEat(IEatable eatObject)
    {
        if (eatObject == null || eatObject.AmountFood <= 0)
        {
            eating = false;
        }
        else
        {
            if (Input.GetButton("Fire1"))
            {
                eating = true;
                Experience += eatObject.GetAmount();
                m_animator.SetBool("isEating", eatObject.Eaten);
                eatObject.Eaten = eating;
                eatObject.DecreaseFood();
                if (!eatObject.Source().isPlaying)
                {
                    EventManager.SoundBroadcast(EVENT.PlaySFX, eatObject.Source(), (int)SFXEvent.Eat);
                }
            }
            else
            {
                eating = false;
                EventManager.SoundBroadcast(EVENT.StopSound, eatObject.Source(), 0);
                eatObject.Eaten = eating;
                m_animator.SetBool("isEating", eatObject.Eaten);
            }
        }
    }

    private void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        Health -= amount;
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
                 CmdEat(Gamemanager.Instance.FoodPlaceList[i]);
            }
        }
    }

    private void AnimationChanger()
    {
        m_animator.SetBool("isEating", eating);
        m_animator.SetBool("isMoving", isMoving);
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
        Vector3 inputvectorX = (Vector3.up *  Input.GetAxisRaw("Horizontal") * turnSpeed);
        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * speed )* Time.deltaTime;
        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * speed )* Time.deltaTime;

        if (inputvectorX.magnitude != 0 || inputvectorY.magnitude != 0 || inputvectorZ.magnitude != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        MovementInputVector = inputvectorY + inputvectorZ;
        rotationInputVector = inputvectorX;
        if(!eating)
        {
        transform.Translate(MovementInputVector);
        transform.Rotate(rotationInputVector);
        }
    }

    public void BarrelRoll()
    {
        rolling = false;
        //BARREL ROLL
        float inputValue = Input.GetAxisRaw("Rotation") * rotateSpeed;

        if (inputValue == 0) return;

        if (transform.rotation.z >= 180)
        {
            return;
        }
        transform.Rotate(0, 0, inputValue);

        rolling = true;
    }
    
    private void Stabilize()
    {
        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
    }

    public void Death()
    {
        Experience -= experiencePenalty;
        Gamemanager.Instance.RespawnPlayer(this);
    }

    private void Awake()
    {
        health = 100;
        musicSource = GetComponentInChildren<AudioSource>();
        SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
    }

    private void Start()
    {
        //Quaternion myQuat = Quaternion.Euler(transform.localEulerAngles);
        //Quaternion targetQuat = Quaternion.Euler(0, 0, 0);
        m_animator = gameObject.GetComponent<Animator>();
        isMoving = true;
        cameraClone = Instantiate(Gamemanager.Instance.CameraPrefab, transform.position, Quaternion.identity);
        EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
        cameraClone.name = "FollowCamera";
        CameraController.cam.InstantiateCamera(this);
    }

    private void Update()
    {
        InteractionChecker();

        UIManager.Instance.InstantiateMatchUI(this);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(30);
        }
    }

    private void FixedUpdate()
    {
        if (CameraController.cam == null) return;

        if (!CameraController.cam.freeCamera)
        {
            MouseMove();
        }
        Move();
        BarrelRoll();
        Restrict();

        //AFTER ROLL
        if (!rolling)
        {
            Stabilize();
        }
    }

    private void LateUpdate()
    {
        AnimationChanger();
    }
}
