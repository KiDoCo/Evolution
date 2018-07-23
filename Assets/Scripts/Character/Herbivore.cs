using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class Herbivore : Character
{

    Quaternion originZ;
    Quaternion currentZ;
    public bool mouseInput;

    protected float health = 2;
    protected float experience = 0;
    private const float healthMax = 2;
    private const float waitTime = 1.0f;
    private const float deathpenaltytime = 2.0f;

    //timer values
    [SerializeField] protected float dashTime = 2f;
    [SerializeField] protected float coolTime = 6f;
    [SerializeField] private float dashSpeed = 2.0f;

    Quaternion targetRotation;

    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }

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

    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);

        if (Input.GetAxisRaw("Mouse Y") != 0 || Input.GetAxisRaw("Mouse X") != 0)
        {
            mouseInput = true;
        }
        else
            mouseInput = false;
    }

    #region dash

    protected virtual void Dash() // sprint for herbivores
    {
        if (canDash)
        {
            Vector3 inputVectorZ = InputManager.Instance.GetButton("Ability") ? Vector3.forward * dashSpeed * Time.deltaTime : Vector3.zero;

            if (inputVectorZ.magnitude != 0)
            {
                isDashing = true;

                StartCoroutine(DashTimer());
            }
            else
            {
                isDashing = false;
            }
            Z += inputVectorZ;
        }
    }

    protected virtual IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(dashTime);
        canDash = false;
        yield return StartCoroutine(CoolTimer());
    }

    protected virtual IEnumerator CoolTimer()
    {
        yield return new WaitForSeconds(coolTime);
        canDash = true;

    }

    #endregion

    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    protected virtual void InteractionChecker()
    {
        for (int i = 0; Gamemanager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.FoodPlaceList[i].GetComponent<FoodBaseClass>().GetCollider().bounds))
            {
                Eat(Gamemanager.Instance.FoodPlaceList[i]);
            }
        }
    }

    protected override void AnimationChanger()
    {
        m_animator.SetBool("IsEating", eating);
        m_animator.SetBool("IsMoving", isMoving);
    }

    protected void Death()
    {
        Gamemanager.Instance.RespawnPlayer(this);
    }

    protected void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        Health -= amount;
    }

    /// <summary>
    /// Takes care of the eating for the player
    /// </summary>
    /// <param name="eatObject"></param>
    private void Eat(GameObject go)
    {
        FoodBaseClass eatObject = go.GetComponent<FoodBaseClass>();
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
                CmdEat(go);

            }
            else
            {
                eating = false;
                EventManager.SoundBroadcast(EVENT.StopSound, eatObject.Source(), 0);
                eatObject.Eaten = eating;
            }
        }
    }

    [Command]
    private void CmdEat(GameObject go)
    {
        Debug.Log(go);
        if (go != null)
        {
            FoodBaseClass eatObject = go.GetComponent<FoodBaseClass>();
            eatObject.Eaten = eating;
            eatObject.DecreaseFood(eatObject.FoodPerSecond * Time.deltaTime);
            StartCoroutine(eatObject.EatChecker());
            eatObject.CoolDownTime = 5.0f;

            if (!eatObject.Source().isPlaying)
            {
                EventManager.SoundBroadcast(EVENT.PlaySFX, eatObject.Source(), (int)SFXEvent.Eat);
            }
        }
    }

    #region movement

    protected override void ForwardMovement()
    {

        if (InputManager.Instance.GetAxis("Vertical") != 0)
        {
            if (currentInput != InputManager.Instance.GetAxis("Vertical")) ForwardVelocity = 0;

            currentInput = InputManager.Instance.GetAxis("Vertical");

            ForwardVelocity += InputManager.Instance.GetAxis("Vertical") * accPerSec * 2;
        }

        //deceleration when stopping movement
        else
        {
            ForwardVelocity = Mathf.SmoothStep(ForwardVelocity, 0, -decPerSec);
        }

        Z = (Vector3.forward * ForwardVelocity) * Time.deltaTime;
    }

    protected override void SidewayMovement()
    {
        Y = InputManager.Instance.GetAxis("Horizontal") * Vector3.down * turnSpeed * Time.deltaTime;
    }

    protected override void UpwardsMovement()
    {
        X = (InputManager.Instance.GetAxis("Altitude") * Vector3.up * rotateSpeed * Time.deltaTime);
    }

    protected override void ApplyMovement()
    {
        inputVector = X + Y + Z;
        isMoving = inputVector.normalized.magnitude != 0 ? true : false;
        AnimationChanger();

        if (CollisionCheck())
        {
            transform.Translate((X + Z) * defaultSpeed);
            transform.Rotate(Y * defaultSpeed);
        }
        else
        {
            moveDirection = Vector3.Cross(colPoint, surfaceNormal);
            moveDirection = Vector3.Cross(surfaceNormal, moveDirection);
            moveDirection = (moveDirection - (Vector3.Dot(moveDirection, surfaceNormal)) * surfaceNormal).normalized;
        }
    }

    #endregion

    public void GetEaten(float dmg)
    {
        TakeDamage(dmg);
    }

    protected override void Awake()
    {

        base.Awake();
    }

    protected override void Start()
    {
        if (isLocalPlayer)
        {
            base.Start();
            cameraClone = Instantiate(cameraClone);
            cameraClone.GetComponent<CameraController>().target = this.transform;
            SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
            m_animator = gameObject.GetComponent<Animator>();
            UIManager.Instance.InstantiateInGameUI(this);
            canBarrellRoll = true;
            canTurn = true;
        }
    }

    protected override void Update()
    {
        if (isLocalPlayer)
        {
            base.Update();
            InteractionChecker();
            UIManager.Instance.UpdateMatchUI(this);
        }
    }

    protected override void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            base.FixedUpdate();
            if (!CameraClone.GetComponent<CameraController>().FreeCamera)
            {
                MouseMove();
            }
            ApplyMovement();
        }
    }

}
