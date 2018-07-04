﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    //values
    [SerializeField] protected float verticalSpeed = 2f;
    [SerializeField] protected float horizontalSpeed = 2f;
    [SerializeField] protected float speed = 2f;
    [SerializeField] protected float AscendSpeed = 2f;
    [SerializeField] protected float turnSpeed = 2f;
    [SerializeField] protected float rotateSpeed = 2f;
    public Quaternion myQuat, targetQuat;
    public float quatSpeed = 1f;
    public float stabilize = 0.1f;
    public float velocity;
    public float restrictAngle = Mathf.Abs(80);
    private float health = 100;
    private float experience = 0;
    private const float healthMax = 100.0f;
    private const float waitTime = 1.0f;
    private const float experiencePenalty = 25.0f;
    private const float deathpenaltytime = 2.0f;
    private bool ready;
    public bool rolling;
    public bool isMoving;
    private bool eating;
    protected Animator m_animator;
    public CameraController camerascript;
    private Vector3 lastposition = Vector3.zero;
    private Vector3 MovementInputVector;
    private Vector3 rotationInputVector;
    private AudioSource musicSource;
    private AudioSource SFXsource;
    private GameObject cameraClone;
    //bools
    public bool hasjustRolled;

    public float Rotatingspeed;
    private Vector3 moveDirection;
    private Vector3 surfaceNormal;
    private Vector3 capsuleNormal;
    private Vector3 colDirection;
    private Vector3 colNormal;
    private Vector3 colPoint;
    private CapsuleCollider col;
    private bool collided = false;

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
            if (health <= 0)
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
    protected virtual void CmdEat(IEatable eatObject)
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

    protected virtual void Death()
    {
        Experience -= experiencePenalty;
        Gamemanager.Instance.RespawnPlayer(this);
    }

    protected virtual void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        Health -= amount;
    }

    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    protected virtual void InteractionChecker()
    {
        for (int i = 0; Gamemanager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.FoodPlaceList[i].GetCollider().bounds))
            {
                CmdEat(Gamemanager.Instance.FoodPlaceList[i]);
            }
        }
    }

    protected virtual void AnimationChanger()
    {
        m_animator.SetBool("isEating", eating);
        m_animator.SetBool("isMoving", isMoving);
    }

    /// <summary>
    /// Avoid control jerkiness with ristricting x rotation
    /// </summary>
    protected virtual void Restrict()
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



    protected virtual void Move()
    {
        isMoving = false;
        Vector3 inputvectorX = (Vector3.up * Input.GetAxisRaw("Horizontal") * turnSpeed);
        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * speed) * Time.deltaTime;
        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * speed) * Time.deltaTime;

        if (inputvectorX.magnitude != 0 || inputvectorY.magnitude != 0 || inputvectorZ.magnitude != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if(collided)
        {
            moveDirection = Vector3.Cross(colPoint, surfaceNormal);
            moveDirection = Vector3.Cross(surfaceNormal, moveDirection);
            moveDirection = (moveDirection - (Vector3.Dot(moveDirection, surfaceNormal)) * surfaceNormal).normalized;


            MovementInputVector = moveDirection;
        }
        else
        {
            MovementInputVector = inputvectorY + inputvectorZ;
        }
        rotationInputVector = inputvectorX;
        if (!eating)
        {
            transform.Translate(MovementInputVector);
            transform.Rotate(rotationInputVector);
        }
    }

    /// <summary>
    /// Checks if player can move in wanted direction
    /// returns true if there is not another bject's collider in way
    /// and false if player would collide with another collider
    /// </summary>
    protected bool CanMove(Vector3 dir)
    {

        float distanceToPoints = col.height / 2 - col.radius;

        //calculating start and en point  of capuleCollider for capsuleCast to use
        Vector3 point1 = transform.position + col.center + Vector3.up * distanceToPoints;
        Vector3 point2 = transform.position + col.center - Vector3.up * distanceToPoints;

        float radius = col.radius * 1.1f;
        float castDistance = 0.5f;

        //shoot capsuleCast
        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, radius, dir, castDistance);

        foreach (RaycastHit objectHit in hits)
        {
            collided = false;

            if (objectHit.transform.tag == "Ground")
            {
                colPoint = objectHit.point;

                RaycastHit hit;

                Physics.Raycast(point1, objectHit.point, out hit);
                Debug.DrawRay(point1, objectHit.point, Color.red);

                if (Vector3.Angle(objectHit.normal, hit.normal) > 5)
                {
                    surfaceNormal = objectHit.normal;
                }
                else
                {
                    surfaceNormal = hit.normal;
                }

               

                collided = true;

                return false;
            }
        }

        return true;
    }



    /// <summary>
    /// Reset z rotation to 0 every frame
    /// </summary>
    protected virtual void Stabilize()
    {
        float z = transform.eulerAngles.z;
        // Debug.Log(z);
        transform.Rotate(0, 0, -z);
    }

    protected virtual void BarrelRoll()
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

    protected virtual void Awake()
    {
        col = GetComponentInChildren<CapsuleCollider>();

        health = 100;
        musicSource = GetComponentInChildren<AudioSource>();
        SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        //Component search
        m_animator = gameObject.GetComponent<Animator>();
        cameraClone = Instantiate(Gamemanager.Instance.CameraPrefab, transform.position, Quaternion.identity);
        cameraClone.name = "FollowCamera";


        //Cursor lock state and quaterions
        Cursor.lockState = CursorLockMode.Locked;
        Quaternion myQuat = Quaternion.Euler(transform.localEulerAngles);
        Quaternion targetQuat = Quaternion.Euler(0, 0, 0);
        isMoving = true;
        EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        CanMove(MovementInputVector);
    }

    protected virtual void FixedUpdate()
    {
        //Stabilize();
        CanMove(MovementInputVector);
        Move();
        BarrelRoll();
    }
}
