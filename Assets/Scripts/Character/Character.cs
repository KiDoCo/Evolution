using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    //values
    public float SpeedValue = 2f; //very important value that can be affected
    [SerializeField] protected float speed = 2f; // speed in character movement
    protected float turnSpeed = 2f;
    [SerializeField] protected float AscendSpeed = 2f;
    [SerializeField] protected float verticalSpeed = 2f;
    [SerializeField] protected float horizontalSpeed = 2f;
    [SerializeField] protected float rotateSpeed = 2f;
    [SerializeField] protected float strafeSpeed = 2f;
    [SerializeField] protected float dashSpeed = 20f;

    protected float velocity;
    protected float restrictAngle = Mathf.Abs(80);
    
    //script reference
    [HideInInspector] public CameraController camerascript;

    //bools
    [SerializeField] protected bool isMoving;
    [SerializeField] protected bool turning;
    [SerializeField] protected bool rolling = false;
    [SerializeField] protected bool isStrafing;
    [SerializeField] protected bool isDashing;

    //ability unlocks from base class
    [SerializeField] protected bool canBarrellRoll;
    [SerializeField] protected bool canStrafe;
    [SerializeField] protected bool canTurn;
    [SerializeField] protected bool canDash;

    //timer bool variables
    [SerializeField] protected bool timerStart;
    [SerializeField] protected bool coolTimer;
    //timer values
    [SerializeField] protected float dashTime = 6f;
    [SerializeField] protected float coolTime = 6f;




    protected float health = 100;
    protected float experience = 0;
    private const float healthMax = 100.0f;
    private const float waitTime = 1.0f;
    private const float experiencePenalty = 25.0f;
    private const float deathpenaltytime = 2.0f;
    private bool ready;
    
    private bool eating;
    protected Animator m_animator;
   
    private Vector3 lastposition = Vector3.zero;
    private Vector3 MovementInputVector;
    private Vector3 rotationInputVector;
    private AudioSource musicSource;
    private AudioSource SFXsource;
    protected GameObject cameraClone;
   
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

    protected float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            speed = value;
        }
    }


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
                    //EventManager.SoundBroadcast(EVENT.PlaySFX, eatObject.Source(), (int)SFXEvent.Eat);
                }
            }
            else
            {
                eating = false;
                //EventManager.SoundBroadcast(EVENT.StopSound, eatObject.Source(), 0);
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


    /// <summary>
    ///  Altitude & Forward/Backwards
    /// </summary>
    protected virtual void Move()
    {
        isMoving = false;
        //Vector3 inputvectorX = (Input.GetAxisRaw("Horizontal") * Vector3.up * turnSpeed);
        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * Speed) * Time.deltaTime;
        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;

        //inputvectorX.magnitude != 0 ||
        if (inputvectorY.magnitude != 0 || inputvectorZ.magnitude != 0)
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
        //rotationInputVector = inputvectorX;
        if (!eating)
        {
            transform.Translate(MovementInputVector);
            //transform.Rotate(rotationInputVector);
        }
    }

    /// <summary>
    /// A and D keys turn
    /// </summary>
    protected virtual void Turn()//I put this separately from "Move" -method, because it has bool check
    {
        if (canTurn)
        {
            isMoving = false;
            
            
            float rotation = (Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime);
            transform.Rotate(0, rotation, 0);

        }
        
    }
    protected virtual void Strafe()//For carnivores
    {
        if(canStrafe)
        {
            
            Vector3 inputStrafeZ = new Vector3(1,0,0) * (Input.GetAxisRaw("Horizontal") * strafeSpeed) * Time.deltaTime;
            transform.Translate(inputStrafeZ);
            if (inputStrafeZ.magnitude !=0)
            {
                isStrafing = true;
                isMoving = true;
            }
            else
            {
                isStrafing = false;
            }
            //float inputStrafeZ = Input.GetAxisRaw("Horizontal") * strafeSpeed * Time.deltaTime;
            //transform.Translate(0, 0, inputStrafeZ);

        }
    }
    protected virtual void BarrellRoll()
    {

        if (canBarrellRoll)
        {
            Vector3 inputRotationZ = new Vector3(0, 0, 1) * (Input.GetAxisRaw("Rotation") * rotateSpeed);
            transform.Rotate(inputRotationZ);
            if (inputRotationZ.magnitude != 0)
            {
                rolling = true;

            }
            else
            {
                rolling = false;
            }

        }

    }
    protected virtual void Dash()
    {
        if (canDash)
        {
            Vector3 inputVectorX = new Vector3(0, 0, 1) * (Input.GetAxisRaw("Dash") * dashSpeed * Time.deltaTime);
            transform.Translate(inputVectorX);
            if (inputVectorX.magnitude != 0)
            {
                isDashing = true;
                isMoving = true;
                StartCoroutine(DashTimer());
            }
            else
            {
                isDashing = false;
                //StopCoroutine(DashTimer());
            }
        }
    }
    public IEnumerator DashTimer()
    {
        timerStart = true;
        yield return new WaitForSeconds(6);
        
        canDash = false;
        timerStart = false;
        yield return StartCoroutine(CoolTimer());
        

    }
    IEnumerator CoolTimer()
    {
        canDash = false;
        coolTimer = true;
        yield return new WaitForSeconds(6);
        coolTimer = false;
        canDash = true;

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
        if (!rolling)
        {

            float z = transform.eulerAngles.z;
            // Debug.Log(z);
            transform.Rotate(0, 0, -z);
        }

    }


    protected virtual void Awake()
    {
        col = GetComponentInChildren<CapsuleCollider>();

        musicSource = GetComponentInChildren<AudioSource>();
        SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        speed = SpeedValue; // can change character speed (by adding value in editor or by a code)
        //Component search
        m_animator = gameObject.GetComponent<Animator>();
        //cameraClone = Instantiate(Gamemanager.Instance.CameraPrefab, transform.position, Quaternion.identity);
        //cameraClone.name = "FollowCamera";


        //Cursor lock state and quaterions
        Cursor.lockState = CursorLockMode.Locked;
        Quaternion myQuat = Quaternion.Euler(transform.localEulerAngles);
        Quaternion targetQuat = Quaternion.Euler(0, 0, 0);
        isMoving = true;
        //UIManager.Instance.InstantiateMatchUI(this);
       // EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
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
        BarrellRoll();
        Turn();
        Strafe();
        Dash();
    }
   
}
