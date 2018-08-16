using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public abstract class Character : NetworkBehaviour
{

    #region Floats    
    [SerializeField] protected float verticalSpeed = 2f;   //mouse movement vertical speed    
    [SerializeField] protected float horizontalSpeed = 2f; // mouse movement horizontal speed
    [SerializeField] protected float rotateSpeed = 2f;     //barrelroll speed
    [SerializeField] protected float strafeSpeed = 2f;     //carnivore strafe

    //Movement variables
    [SerializeField] protected float maxSpeed = 10.0f;
    [SerializeField] protected float accTimeToMax = 1.5f;
    [SerializeField] protected float decTimeToMin = 1.0f;
    protected float accPerSec;
    protected float decPerSec;
    protected float forwardVelocity;
    protected float backwardVelocity;
    protected float currentInput;
    protected float restrictAngle = Mathf.Abs(80);
    public float turnSpeed = 2.0f;
    protected float defaultSpeed = 1.0f;
    #endregion

    #region Booleans
    [SerializeField] protected bool turning;
    [SerializeField] protected bool rolling = false;
    public bool isStrafing; //1st person kamera käyttää näitä
    protected bool isMoving;


    private bool ready;
    protected bool eating;

    //timer bools
    [SerializeField] protected bool coolTimer;    //ability unlock bools used in editor

    #endregion

    [SerializeField] protected GameObject cameraPrefab;
    protected GameObject spawnedCam;
    protected Animator m_animator;
    private AudioSource musicSource;
    protected AudioSource SFXsource;

    //Movement vectors
    protected Vector3 Y;
    protected Vector3 X;
    protected Vector3 Z;
    protected Vector3 inputVector;
    private Vector3 rotationInputVector;

    #region Collider variables
    public float Rotatingspeed;
    protected Vector3 moveDirection;
    protected Vector3 surfaceNormal;
    protected Vector3 capsuleNormal;
    protected Vector3 colDirection;
    protected Vector3 colNormal;
    protected Vector3 colPoint;
    protected CapsuleCollider col;
    protected bool collided = false;

    #endregion

    [SerializeField] protected Renderer playerMesh = null;
    protected Vector3 dir;
    private Vector3 curNormal = Vector3.up;
    private Vector3 colPosition;
    private RaycastHit hitDown;
    private RaycastHit hitInfo;
    public LayerMask CollisionMask;
    private float Height = 0.3f;
    public float HeightPadding = 0.05f;
    public float MaxGroundAngle = 120f;
    private float step;
    private float groundAngle;
    RaycastHit[] hits = new RaycastHit[12];
    public float SideColDistance, Buffer;
    private float smooth;

    public float ForwardVelocity
    {
        get
        {
            return forwardVelocity;
        }

        set
        {
            forwardVelocity = Mathf.Clamp(value, -maxSpeed, maxSpeed);
        }
    }


    protected float Speed
    {
        get
        {
            return defaultSpeed;
        }

        set
        {
            defaultSpeed = value;
        }
    }

    public Vector3 InputVector
    {
        get
        {
            return inputVector;
        }
    }
    public GameObject PlayerCamera
 {
        get
        {
            return spawnedCam;
        }
    }

    #region Movement methods

    protected abstract void ForwardMovement();

    protected abstract void SidewayMovement();

    protected abstract void UpwardsMovement();

    protected abstract void ApplyMovement();

    protected abstract void AnimationChanger();

    /// <summary>
    /// Avoid control jerkiness with restricting x rotation
    /// </summary>
    protected virtual void Restrict()
    {
        transform.rotation = Quaternion.Euler(new Vector3(strangeAxisClamp(transform.rotation.eulerAngles.x, 75, 275), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)); }

    // Clamps angle (different from the normal clamp function)
    private float strangeAxisClamp(float value, float limit1, float limit2)
    {
        if (value > limit1 && value < 160f)
            value = limit1;
        else if (value > 160f && value < limit2)
            value = limit2;
        return value;
    }

    #endregion

    /// <summary>
    /// Checks if player can move in wanted direction
    /// returns true if there is not another bject's collider in way
    /// and false if player would collide with another collider
    /// </summary>
    protected void CheckCollision()
    {
        collided = false;
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
        float castDistance;
        castDistance = Height / 2 + Buffer;

        //shoot capsuleCast and save hit collisions to hits-array
        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, Height + HeightPadding, dir, castDistance * smooth, CollisionMask);

        // check all collisions for their type and move of not accordingly
        foreach (RaycastHit objectHit in hits)
        {
            if (Physics.Raycast(rayDown, out hitInfo, castDistance + Buffer))
            {
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

                    //check distance to collision point and move target away from it
                    if (Vector3.Distance(transform.position, colPoint) < Height + HeightPadding)
                    {
                        transform.position = Vector3.Lerp(transform.position, transform.position + surfaceNormal * Buffer, Time.fixedDeltaTime * 4);
                    }
                }
            }

            //check direction and determinate angle for normal and colPoint
            if (Physics.Raycast(transform.position, dir, out hitInfo, castDistance))
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

            // check if sides of character hit something
            // if only one side collides turn character slightly away from collision
            if (Physics.Raycast(rayRight, out hitInfo, SideColDistance) && Physics.Raycast(rayLeft, out hitInfo, SideColDistance))
            {
                transform.rotation = Quaternion.Euler(new Vector3(strangeAxisClamp(transform.rotation.eulerAngles.x, 60, 300), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            }
            else if (Physics.CapsuleCast(point1, point2, radius, transform.right, out hitInfo, SideColDistance))
            {
                Vector3 temp = Vector3.Cross(transform.up, hitInfo.normal);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(temp), step);
            }
            else if (Physics.CapsuleCast(point1, point2, radius, -transform.right, out hitInfo, SideColDistance))
            {
                Vector3 temp = Vector3.Cross(transform.up, hitInfo.normal);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-temp), step);
            }

            //Keep character at given distance of colliding objects
            if (Vector3.Distance(transform.position, colPoint) < Height + HeightPadding)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + curNormal * (radius + Buffer), step * Time.fixedDeltaTime);
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

    private void PauseMenuUpdate()
    {
        if (InGameManager.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && InGameManager.Instance.InMatch)
            {
                PauseMenu.Instance.UI.SetActive(!PauseMenu.Instance.UI.activeSelf);

                if (NetworkGameManager.Instance.LocalCharacter != null)
                {
                    InputManager.Instance.EnableInput = !PauseMenu.Instance.UI.activeSelf;
                }

                UIManager.Instance.HideCursor(!PauseMenu.Instance.UI.activeSelf);
            }
        }
    }

    /// <summary>
    /// Reset z rotation to 0 every frame
    /// </summary>
    protected virtual void Stabilize()
    {
        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
    }

    protected abstract void SpawnCamera();
    [ServerCallback]
    public void EnablePlayer(bool enabled)
    {
        playerMesh.enabled = enabled;
        InputManager.Instance.EnableInput = enabled;
        col.enabled = enabled;
        RpcEnablePlayer(enabled);
    }

    [ClientRpc]
    private void RpcEnablePlayer(bool enabled)
    {
        playerMesh.enabled = enabled;
        col.enabled = enabled;
        InputManager.Instance.EnableInput = enabled;
    }

    [ServerCallback]
    public void EnablePlayerCamera(bool enabled)
    {
        if (isLocalPlayer)
            spawnedCam.SetActive(enabled);
        if (InGameManager.Instance != null)
            InGameManager.Instance.MapCamera.SetActive(!enabled);

        RpcEnablePlayerCamera(enabled);
    }

    [ClientRpc]
    private void RpcEnablePlayerCamera(bool enabled)
    {
        if (isLocalPlayer)
            spawnedCam.SetActive(enabled);
        if (InGameManager.Instance != null)
            InGameManager.Instance.MapCamera.SetActive(!enabled);
    }

    #region Unity Methods

    protected virtual void Awake()
    {
        col = GetComponentInChildren<CapsuleCollider>();
        musicSource = GetComponent<AudioSource>();
        //SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        Debug.Log("Character activated");
        if (isLocalPlayer)
        {
            NetworkGameManager.Instance.LocalCharacter = this;
            UIManager.Instance.InstantiateInGameUI(this);
            SpawnCamera();
            EnablePlayerCamera(true);
        }

        InputManager.Instance.EnableInput = true;        accPerSec = maxSpeed / accTimeToMax;
        decPerSec = -maxSpeed / decTimeToMin;
        EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient); }

    protected virtual void Update()
    {

        PauseMenuUpdate();
    }

    protected virtual void FixedUpdate()
    {
        step = defaultSpeed * Time.deltaTime;

        dir = transform.TransformDirection(InputVector);


        CalculateGroundAngle();
        CheckCollision();
        AnimationChanger();
        Debug.Log(collided);
        if (collided)
        {
            return;
        }
        else
        {
            ForwardMovement();
            UpwardsMovement();
            SidewayMovement();
            ApplyMovement();
        }
    }

    #endregion

}