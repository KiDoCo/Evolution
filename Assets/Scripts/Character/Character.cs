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
    protected float health = 100;
    protected float experience = 0;
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

    private Vector3 colPosition;
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 colPoint;
    private CapsuleCollider col;
    private bool collided = false;
    private float groundAngle;
    private RaycastHit hitInfo;
    int i = 0;
    private Vector3 dir;
    private bool grounded = false;
    private Vector3 curNormal = Vector3.up;
    private Vector3 curRight = Vector3.right;
    private Vector3 curLeft = Vector3.left;
    private bool canMove = true;
    private bool colFront = false;
    private bool bothSidesCol = false;
    private RaycastHit hitDown;

    private float Height = 0.3f;
    public float HeightPadding = 0.05f;
    public LayerMask ground;
    public float MaxGroundAngle = 120f;

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


        MovementInputVector = inputvectorY + inputvectorZ;
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
    protected void CheckCollision()
    {
        collided = false;
        canMove = true;

        //initialize rays
        Ray rayForward = new Ray(transform.position, transform.forward);
        Ray rayBack = new Ray(transform.position, -transform.forward);
        Ray rayUp = new Ray(transform.position, transform.up);
        Ray rayDown = new Ray(transform.position, -transform.up);
        Ray rayRight = new Ray(transform.position, transform.right);
        Ray rayLeft = new Ray(transform.position, -transform.right);

        //calculating and setting points and distances for capsule cast (and rays)
        float distanceToPoints = col.height / 2 - col.radius;
        Vector3 point1 = transform.position + col.center + Vector3.up * distanceToPoints;
        Vector3 point2 = transform.position + col.center - Vector3.up * distanceToPoints;
        float radius = col.radius;
        float castDistance = 0.5f;
        Height = col.height;

        //shoot capsuleCast
        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, Height + HeightPadding, dir, castDistance, ground);

        // check al collisions for their type and move of not accordingly
        foreach (RaycastHit objectHit in hits)
        {
            if (Physics.Raycast(rayDown, out hitInfo, (radius + HeightPadding)))
            {
                grounded = true;
                colPoint = hitInfo.point;

                //check if ground angle allows movement
                if (groundAngle < MaxGroundAngle)
                {
                    if (Physics.Raycast(transform.position, -surfaceNormal, out hitDown))
                    {
                        print(groundAngle);
                        surfaceNormal = Vector3.Lerp(surfaceNormal, hitDown.normal, 4 * Time.deltaTime);
                    }

                    //Rotate character according to ground angle
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, surfaceNormal), hitInfo.normal), Time.deltaTime * 5.0f);

                    //check distance to 
                    if (Vector3.Distance(transform.position, colPoint) < (radius + HeightPadding))
                    {
                        transform.position = Vector3.Lerp(transform.position, transform.position + surfaceNormal * radius, Time.deltaTime * 5.0f);
                        grounded = true;
                    }
                }
            }
            else
            {
                grounded = false;
            }

            //check direction and determinate angle for normal and colPoint
            if (Physics.Raycast(rayForward, out hitInfo, radius + HeightPadding) || Physics.Raycast(rayBack, out hitInfo, radius + HeightPadding) || Physics.Raycast(rayUp, out hitInfo, radius))
            {

                if (Vector3.Angle(objectHit.normal, hitInfo.normal) > 5)
                {
                    curNormal = objectHit.normal;
                    colPoint = objectHit.point;
                }
                else
                {
                    curNormal = hitInfo.normal;
                    colPoint = hitInfo.point;
                }

                collided = true;
            }

            //Check side collisions and if both sides collide turn canMove to false
            if (Physics.Raycast(rayRight, out hitInfo, Height) || Physics.Raycast(rayLeft, out hitInfo, Height))
            {
                if (Physics.Raycast(rayRight, (radius * 0.5f)) && Physics.Raycast(rayLeft, (radius * 0.5f)))
                {
                    bothSidesCol = true;
                    canMove = false;
                    //return;
                }

                curNormal = hitInfo.normal;
                colPoint = hitInfo.point;

                // turn character at side collision for smoother movement
                if (Vector3.Distance(transform.position, colPoint) < (radius * 1.5f) && !bothSidesCol)
                {
                    Vector3 targetDir = colPoint - transform.position;
                    Vector3 moveDirection = Vector3.RotateTowards(transform.forward, -targetDir, perc, 0.0f);

                    transform.rotation = Quaternion.LookRotation(moveDirection);                   
                }
            }

            //Keep character at given distance of colliding objects
            if (Vector3.Distance(transform.position, colPoint) < radius + HeightPadding)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + curNormal * radius, perc);
            }

            //check if character can fit through caves etc. and if character collides head first
            if (Physics.CapsuleCast(point1, point2, radius, transform.forward, out hitInfo, radius) || Physics.CapsuleCast(point1, point2, radius, transform.up, out hitInfo, radius))
            {
                print(groundAngle);
                canMove = false;
                curNormal = hitInfo.normal;
                colPoint = hitInfo.point;

                print("can't move");
                if (grounded)
                {
                    if (groundAngle < MaxGroundAngle)
                        canMove = true;
                    print("can move");
                }
                return;
            }
            else
            {
                canMove = true;
            }
        }
    }

    /// <summary>
    /// calculate ground angle 
    /// </summary>
    private void CalculateGroundAngle()
    {
        if (!collided)
        {
            groundAngle = 90;
            return;
        }
        groundAngle = Vector3.Angle(dir, hitDown.normal);
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
        UIManager.Instance.InstantiateMatchUI(this);
        EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    float perc;

    protected virtual void FixedUpdate()
    {
        perc = speed * Time.deltaTime;
        Stabilize();
        dir = transform.TransformDirection(MovementInputVector);
        CalculateGroundAngle();
        CheckCollision();

        if (collided || !canMove) return;

        Move();
        BarrelRoll();
    }
}
