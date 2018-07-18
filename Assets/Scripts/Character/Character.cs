using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public abstract class Character : NetworkBehaviour
{
    #region Floats

    //Movement 
    public float                     SpeedValue = 2f;   //Debug value which is assigned to speed
    protected float                  speed = 2f;     // vertical speed modifier
    [SerializeField] protected float turnSpeed = 2f;     //Rotation horizontal speed modifier
    [SerializeField] protected float AscendSpeed = 2f; //Upwards and downwards speed modifier
    [SerializeField] protected float verticalSpeed = 2f;   //mouse movement vertical speed
    [SerializeField] protected float horizontalSpeed = 2f; // mouse movement horizontal speed
    [SerializeField] protected float rotateSpeed = 2f;     //barrelroll speed
    [SerializeField] protected float strafeSpeed = 2f;     //carnivore strafe
    [SerializeField] protected float dashSpeed = 20f;      //herbivore sprint
    protected float                  velocity;
    protected float                  restrictAngle = Mathf.Abs(80);

    //timer values
    [SerializeField] protected float dashTime = 6f;
    [SerializeField] protected float coolTime = 6f;

    //character stats
    protected float health = 100;
    protected float experience = 0;
    private const float healthMax = 100.0f;
    private const float waitTime = 1.0f;
    private const float deathpenaltytime = 2.0f;

    #endregion
    //script reference

    #region Booleans
    [SerializeField] protected bool turning;
    [SerializeField] protected bool rolling = false;
    public bool                     isDashing;
    public bool                     isStrafing; //1st person kamera käyttää näitä
    protected bool                  isMoving;
    public bool                     hasjustRolled;
    protected bool                  barrelRoll;
    private bool ready;
    protected bool eating;

    //timer bools
    [SerializeField] protected bool timerStart;
    [SerializeField] protected bool coolTimer;

    //ability unlock bools used in editor
    [SerializeField] protected bool canBarrellRoll;
    [SerializeField] protected bool canStrafe;
    [SerializeField] protected bool canTurn;
    [SerializeField] protected bool canDash;

    #endregion 

   [SerializeField] protected GameObject             cameraClone;
    protected CameraController                       camerascript;
    protected Animator                               m_animator;
    private AudioSource                              musicSource;
    protected AudioSource                            SFXsource;
    private Vector3                                  inputVector;
    private Vector3                                  MovementInputVector;
    private Vector3                                  rotationInputVector;

    #region Collider variables
    public float                     Rotatingspeed; private Vector3 moveDirection;
    private Vector3                  surfaceNormal;
    private Vector3                  capsuleNormal;
    private Vector3                  colDirection;
    private Vector3                  colNormal;
    private Vector3                  colPoint;
    private CapsuleCollider          col;
    private bool                     collided = false;

    #endregion

    //End variables

    #region Getter&Setter
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

    public GameObject CameraClone
    {
        get
        {
            return cameraClone;
        }
    }

    public Vector3 InputVector
    {
        get
        {
            return inputVector;
        }
    }
    #endregion

    #region EventMethods

    protected virtual void Death()
    {
        Gamemanager.Instance.RespawnPlayer(this);
    }

    protected virtual void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        Health -= amount;
    }
    
    #endregion

    #region Movement methods

    /// <summary>
    ///  Altitude & Forward/Backwards
    /// </summary>    
    protected virtual void Move()
    {
        Vector3 inputvectorX =  InputManager.Instance.GetAxis("Horizontal") * Vector3.up * turnSpeed;
        Vector3 inputvectorY = (InputManager.Instance.GetAxis("Vertical") * Vector3.forward * Speed);
        Vector3 inputvectorZ = (InputManager.Instance.GetAxis("Jump") * Vector3.forward * rotateSpeed * Time.deltaTime);
        inputVector = inputvectorX + inputvectorY + inputvectorZ;
        //Movement direction checkers
        isMoving = inputVector.normalized.magnitude != 0 ? true : false;
        Turn();

        MovementInputVector = inputvectorY + inputvectorZ;

        moveDirection = Vector3.Cross(colPoint, surfaceNormal);
        moveDirection = Vector3.Cross(surfaceNormal, moveDirection);
        moveDirection = (moveDirection - (Vector3.Dot(moveDirection, surfaceNormal)) * surfaceNormal).normalized;

        if (!eating)
        {
            transform.Translate(MovementInputVector);
        }
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
    /// A and D keys turn
    /// </summary>
    protected virtual void Turn()// is separately from "Move" -method, because it has bool check
    {
        if (canTurn)
        {

            float rotation = (Input.GetAxisRaw("Horizontal") * turnSpeed * Time.deltaTime);
            if (rotation != 0)
            {
                isMoving = true;
            }
            transform.Rotate(0, rotation, 0);

        }
    }

    protected virtual void BarrelRoll() //if needed 
    {

        if (canBarrellRoll)
        {
            Vector3 inputRotationZ = new Vector3(0, 0, 1) * (Input.GetAxisRaw("Rotation") * rotateSpeed);
            transform.Rotate(inputRotationZ);
            if (inputRotationZ.magnitude != 0)
            {
                rolling = true;
                isMoving = true;

            }
            else
            {
                rolling = false;
            }

        }

    }

    protected virtual void Dash() // sprint for herbivores
    {
        if (canDash)
        {
            Vector3 inputVectorX = new Vector3(0, 0, 1) * (Input.GetAxisRaw("Dash") * dashSpeed * Time.deltaTime);
            transform.Translate(inputVectorX);
            if (inputVectorX.magnitude != 0)
            {
                isDashing = true;

                StartCoroutine(DashTimer());
            }
            else
            {
                isDashing = false;
                //StopCoroutine(DashTimer());
            }
        }
    }
    #endregion

    protected virtual void AnimationChanger()
    {
        m_animator.SetBool("isEating", eating);
        m_animator.SetBool("isMoving", isMoving);
    }

    protected virtual IEnumerator DashTimer() //used in Dash();
    {
        timerStart = true;
        yield return new WaitForSeconds(dashTime);

        canDash = false;
        timerStart = false;
        yield return StartCoroutine(CoolTimer());


    }
    
    protected virtual IEnumerator CoolTimer()
    {
        canDash = false;
        coolTimer = true;
        yield return new WaitForSeconds(coolTime);
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
            transform.Rotate(0, 0, -z);
        }

    }

    #region Unity Methods

    protected virtual void Awake()
    {
        col = GetComponentInChildren<CapsuleCollider>();
        musicSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        speed = SpeedValue; 

        //Cursor lock state and quaterions
        Cursor.lockState = CursorLockMode.Locked;

         EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetKey(KeyCode.P))
            experience++;

        CanMove(MovementInputVector);
    }

    protected virtual void FixedUpdate()
    {

        //Stabilize();
        CanMove(MovementInputVector);
        Move();
        BarrelRoll();
        Dash();
        AnimationChanger();
    }
    #endregion

}
