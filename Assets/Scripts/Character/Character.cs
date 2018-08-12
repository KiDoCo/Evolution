using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Character : MonoBehaviour
{

    //values
    public float SpeedValue = 2f;   //very important value that can be affected
    protected float speed = 2f;     // speed in character movement
    [SerializeField] protected float turnSpeed = 2f;     //herbivore A & D turn
    [SerializeField] protected float AscendSpeed = 2f; //altitude shift & ctrl
    [SerializeField] protected float verticalSpeed = 2f; //mouse movement vertical speed
    [SerializeField] protected float horizontalSpeed = 2f; // mouse movement horizontal speed
    [SerializeField] protected float rotateSpeed = 2f; //barrelroll speed
    [SerializeField] protected float strafeSpeed = 2f; //carnivore strafe
    [SerializeField] protected float dashSpeed = 20f; //herbivore sprint

    protected float velocity;
    protected float restrictAngle = Mathf.Abs(80);

    //script reference
    [HideInInspector] public CameraController camerascript;

    //bools
    protected bool isMoving;
    [SerializeField] protected bool turning;
    [SerializeField] protected bool rolling = false;
    public bool isReversing; //3rd person camera käyttää
    public bool isDashing;
    public bool isStrafing; //1st person kamera käyttää näitä
    public bool isMovingVertical; // --"--
    public bool isMovingForward;

    //ability unlock bools used in editor
    [SerializeField] protected bool canBarrellRoll;
    [SerializeField] protected bool canStrafe;
    [SerializeField] protected bool canTurn;
    [SerializeField] protected bool canDash;

    //timer bools
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

    private CapsuleCollider col;
    private bool collided = false;
    private bool grounded = false;
    private Vector3 dir;
    private Vector3 curNormal = Vector3.up;
    private Vector3 colPosition;
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 colPoint;
    private RaycastHit hitDown;
    private RaycastHit hitInfo;
    public LayerMask ground;
    private float Height = 0.3f;
    public float HeightPadding = 0.05f;
    public float MaxGroundAngle = 120f;
    private float step;
    private float groundAngle;
    private float smooth;
    int hitCount;
    RaycastHit[] hits = new RaycastHit[12];
    float minMoveDistance = 0.001f;
    float maxBounces = 2;

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
        transform.rotation = Quaternion.Euler(new Vector3(strangeAxisClamp(transform.rotation.eulerAngles.x, 60, 300), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
    }

    // Clamps angle (different from the normal clamp function)
    private float strangeAxisClamp(float value, float limit1, float limit2)
    {
        if (value > limit1 && value < 160f)
            value = limit1;
        else if (value > 160f && value < limit2)
            value = limit2;
        return value;
    }

    /// <summary>
    ///  Altitude & Forward/Backwards
    /// </summary>
    protected virtual void Move()
    {
        isMoving = false;

        //tarkista peruuttaako

        if (Input.GetAxisRaw("Vertical") < 0)
        {
            isReversing = true;
            isMovingForward = false;
        }
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            isReversing = false;
            isMovingForward = true;
        }
        else if (Input.GetAxisRaw("Vertical") == 0)
        {
            isReversing = false;
            isMovingForward = false;
        }

        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * Speed) * Time.deltaTime;
        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;
        Turn();
        if (inputvectorY.magnitude != 0 || inputvectorZ.magnitude != 0)
        {
            isMoving = true;
        }
        //tarkista nouseeko laskeeko
        if (inputvectorZ.magnitude != 0)
        {
            isMovingVertical = true;
        }
        else if (inputvectorZ.magnitude == 0)
        {
            isMovingVertical = false;
        }

        MovementInputVector = inputvectorY + inputvectorZ;

        if (!eating)
        {
            transform.Translate(MovementInputVector);
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

    protected virtual void BarrellRoll() //if needed 
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
        if (canDash && isMoving)
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

    public IEnumerator DashTimer() //used in Dash();
    {
        timerStart = true;
        yield return new WaitForSeconds(dashTime);

        canDash = false;
        timerStart = false;
        yield return StartCoroutine(CoolTimer());


    }
    // -->
    IEnumerator CoolTimer()
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
    private void CheckCollision()
    {
        Vector3 direction = dir.normalized;        
        float castDistance;
        if (isDashing)
        {
            smooth = dashSpeed;
            castDistance = (MovementInputVector * dashSpeed).magnitude;
            print("castdistance when dashing: " + castDistance);
        }
        else
        {
            smooth = speed;
            castDistance = 0.5f;
        }

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
        float radius = col.radius * 1.1f;
        Height = col.height;

        if (Physics.Raycast(rayDown, out hitInfo, castDistance + HeightPadding))
        {
            grounded = true;
            colPoint = hitInfo.point;
            print("ground");
            //check if ground angle allows movement
            if (groundAngle < MaxGroundAngle)
            {
                if (Physics.Raycast(transform.position, -surfaceNormal, out hitDown))
                {
                    print(groundAngle);
                    surfaceNormal = Vector3.Lerp(surfaceNormal, hitDown.normal, 4 * Time.deltaTime);
                }

                //Rotate character according to ground angle
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, surfaceNormal), hitInfo.normal), step);

                //check distance to 
                if (Vector3.Distance(transform.position, colPoint) < radius + HeightPadding)
                {
                    transform.position = Vector3.Lerp(transform.position, transform.position + surfaceNormal * radius, Time.fixedDeltaTime * smooth);                  
                }
            }
        }

        //if (Physics.Raycast(rayRight, out hitInfo, radius + HeightPadding) && Physics.Raycast(rayLeft, out hitInfo, radius + HeightPadding))
        //{
        //    print("both sides collided");
        //    transform.rotation = Quaternion.Euler(new Vector3(strangeAxisClamp(transform.rotation.eulerAngles.x, 60, 300), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        //}
        //if (Physics.Raycast(rayRight, out hitInfo, radius + HeightPadding))
        //{
        //    Vector3 temp = Vector3.Cross(transform.up, hitInfo.normal);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(temp), smooth);
        //}
        //if (Physics.Raycast(rayLeft, out hitInfo, radius + HeightPadding))
        //{
        //    Vector3 temp = Vector3.Cross(transform.up, hitInfo.normal);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-temp), smooth);
        //}

        hitCount = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, hits, castDistance + 0.02f, ground, QueryTriggerInteraction.Ignore);

        if (hitCount > 0)
        {
            collided = true;
            for (int i = 0; i < hitCount; i++)
            {
                if (Vector3.Angle(hits[i].normal, hitInfo.normal) > 5)
                {
                    curNormal = hits[i].normal;
                    colPoint = hits[i].point;
                }
                else
                {
                    curNormal = hitInfo.normal;
                    colPoint = hitInfo.point;
                }             
            }
        }
        else
        {
            collided = false;
        }

        if (Vector3.Distance(transform.position, colPoint) < radius + HeightPadding)
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + curNormal * 0.7f, step * Time.fixedDeltaTime);
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
        //SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
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


        //UIManager.Instance.InstantiateMatchUI(this);
        // EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    protected virtual void FixedUpdate()
    {
        step = speed * Time.deltaTime;
        //Stabilize();

        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * Speed) * Time.deltaTime;
        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;
        MovementInputVector = inputvectorY + inputvectorZ;

        dir = transform.TransformDirection(MovementInputVector);
        CalculateGroundAngle();
        CheckCollision();

        if (collided)
        {
            canDash = false;
            return;
        }
        else
        {
            canDash = true;
            Move();
            BarrellRoll();
            Dash();
        }
    }

}
