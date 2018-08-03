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
    public bool isDashing;
    public bool isStrafing; //1st person kamera käyttää näitä
    protected bool isMoving;
    public bool hasjustRolled;
    protected bool barrelRoll;
    private bool ready;
    protected bool eating;
    protected bool inputEnabled = true;

    //timer bools
    [SerializeField] protected bool coolTimer;

    //ability unlock bools used in editor
    [SerializeField] protected bool canBarrellRoll;
    [SerializeField] protected bool canStrafe;
    [SerializeField] protected bool canTurn;
    [SerializeField] protected bool canDash;

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

    //End variables

    #region Getter&Setter

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
    
    public bool InputEnabled
    {
        get
        {
            return inputEnabled;
        }
        set
        {
            inputEnabled = value;
        }
    }
    
    #endregion

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
        transform.rotation = Quaternion.Euler(new Vector3(strangeAxisClamp(transform.rotation.eulerAngles.x, 75, 275), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
    }

    // Clamps angle (different from the normal clamp function)
    private float strangeAxisClamp(float value, float limit1, float limit2)
    {
        if (value > limit1 && value < 180f)
            value = limit1;
        else if (value > 180f && value < limit2)
            value = limit2;
        return value;
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

    #endregion

    public bool CollisionCheck()
    {
        float distanceToPoints = col.height / 2 - col.radius;

        //calculating start and en point  of capuleCollider for capsuleCast to use
        Vector3 point1 = transform.position + col.center + Vector3.up * distanceToPoints;
        Vector3 point2 = transform.position + col.center - Vector3.up * distanceToPoints;
        Vector3 dir = InputVector;
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
        transform.Rotate(0, 0, -z);
    }

    protected abstract void SpawnCamera();

    [ServerCallback]
    public void EnablePlayer(bool enabled)
    {
        playerMesh.enabled = enabled;
        inputEnabled = enabled;
        col.enabled = enabled;
        RpcEnablePlayer(enabled);
    }

    [ClientRpc]
    private void RpcEnablePlayer(bool enabled)
    {
        playerMesh.enabled = enabled;
        col.enabled = enabled;
        inputEnabled = enabled;
    }

    #region Unity Methods

    protected virtual void Awake()
    {
        col = GetComponentInChildren<CapsuleCollider>();
        musicSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        if (isLocalPlayer)
        {
            NetworkGameManager.Instance.LocalCharacter = this;
            UIManager.Instance.InstantiateInGameUI(this);
        }

        inputEnabled = true;
        accPerSec = maxSpeed / accTimeToMax;
        decPerSec = -maxSpeed / decTimeToMin;
        EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
    }

    protected virtual void Update()
    {
        ForwardMovement();
        UpwardsMovement();
        SidewayMovement();
    }

    protected virtual void FixedUpdate()
    {
        Restrict();
        Stabilize();
    }

    #endregion
}
