using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class Herbivore : Character
{
    //rest of the variables can be found from character

    //Component search
    private Quaternion originZ;
    private Quaternion currentZ;
    private Color visibilityColor;
    public Material defaultMat;
    public Material glassMat;

    private bool HasRespawned;
    private float surTime;
    public bool mouseInput;
    private bool cloaked = false;
    public float defSmooth = 0;

    //Animation variables
    private float horizontalMovement;
    private float verticalMovement;

    //timer values
    [SerializeField] protected float dashTime = 2f;
    [SerializeField] protected float coolTime = 6f;
    [SerializeField] private float dashSpeed = 2.0f;

    #region Character stats

    protected float health = 2;
    private const float healthMax = 2;
    private const float waitTime = 1.0f;
    private const float deathpenaltytime = 2.0f;
    protected float experience = 0;
    private int deathcount;

    #endregion

    #region Getters&Setters

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
    public int Deathcount
    {
        get
        {
            return deathcount;
        }

        set
        {
            deathcount = value;
        }
    }
    public float SurTime
    {
        get
        {
            return surTime;
        }
    }

    #endregion

    //methods

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

    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    protected virtual void InteractionChecker()
    {
        for (int i = 0; InGameManager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(InGameManager.Instance.FoodPlaceList[i].GetComponent<FoodBaseClass>().GetCollider().bounds))
            {

                Eat(InGameManager.Instance.FoodPlaceList[i]);
            }
        }
    }

    protected override void AnimationChanger()
    {
        playerMesh.SetBlendShapeWeight(0, Mathf.Clamp(Experience, 25, 100));
        m_animator.SetBool("isEating", eating);
        m_animator.SetBool("isMoving", isMoving);
        m_animator.SetFloat("Horizontal", horizontalMovement);
        m_animator.SetFloat("Vertical", verticalMovement);
        m_animator.SetFloat("Evolution", Experience);
    }

    protected void Death()
    {
        if (InGameManager.Instance.LifeCount > 0)
        {
            if (!HasRespawned)
            {
                surTime = InGameManager.Instance.MatchTimer;
                HasRespawned = true;
            }
            Deathcount++;
            InGameManager.Instance.RespawnPlayer(this);
        }
        else
        {
            InGameManager.Instance.KillPlayer(this);
        }
    }

    protected void TakeDamage(float amount)
    {
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        Health -= amount;
    }

    private void ComponentSearch()
    {
        visibilityColor = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color;
        SFXsource = transform.GetChild(2).GetComponent<AudioSource>();
        m_animator = gameObject.GetComponent<Animator>();
        playerMesh = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
    }

    protected override void SpawnCamera()
    {
        GameObject mapCam = GameObject.FindGameObjectWithTag("MapCamera");
        if (mapCam != null)
            Destroy(mapCam);

        spawnedCam = Instantiate(cameraPrefab);
        spawnedCam.GetComponent<CameraController>().InstantiateCamera(this);
    }

    public void GetEaten(float dmg)
    {
        TakeDamage(dmg);
    }

    #region Eat methods

    /// <summary>
    /// Takes care of the eating for the player
    /// </summary>
    /// <param name="eatObject"></param>
    private void Eat(GameObject go)
    {
        FoodBaseClass eatObject = go.GetComponent<FoodBaseClass>();

        if (InputManager.Instance.GetButton("Eat"))
        {
            eating = true;
            Experience += eatObject.GetAmount();

            if (isServer) // for sound playing, need to send info to clients and host
                RpcEat(go);
            else
                CmdEat(go);
        }
        else
        {
            eating = false;
            EventManager.SoundBroadcast(EVENT.StopSound, eatObject.Source(), 0);
            eatObject.Eaten = eating;
        }
    }

    /// <summary>
    /// Sends information to the server
    /// </summary>
    /// <param name="go"></param>
    [ClientRpc]
    private void RpcEat(GameObject go)
    {
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

    /// <summary>
    /// Sends information to other players
    /// </summary>
    /// <param name="go"></param>
    [Command]
    private void CmdEat(GameObject go)
    {
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

    #endregion

    #region dash

    protected void Dash() // sprint for herbivores
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

    protected IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(dashTime);
        canDash = false;
        yield return StartCoroutine(CoolTimer());
    }

    protected IEnumerator CoolTimer()
    {
        yield return new WaitForSeconds(coolTime);
        canDash = true;

    }

    #endregion

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
        Y = InputManager.Instance.GetAxis("Horizontal") * Vector3.down * turnSpeed;
    }

    protected override void UpwardsMovement()
    {
        X = (InputManager.Instance.GetAxis("Altitude") * Vector3.up * rotateSpeed * Time.deltaTime);
    }

    protected override void ApplyMovement()
    {
        if (eating) return;
        inputVector = X + Y + Z;
        verticalMovement = InputManager.Instance.GetAxis("Vertical");
        horizontalMovement = InputManager.Instance.GetAxis("Horizontal");
        isMoving = inputVector.normalized.magnitude != 0 ? true : false;

        if (CollisionCheck())
        {
            transform.Translate((X + Z) * defaultSpeed);
            transform.Rotate(Y * defaultSpeed);
        }
    }



    #endregion

    //BUG: teeth can be seen
    #region Cloak

    private void ToggleCloak() //Change this to non-toggle if needed
    {
        Debug.Log("toggle cloak");
        cloaked = !cloaked;
        if (cloaked)
        {
            visibilityColor.a = 0;
            StartCoroutine(SwapColor(visibilityColor));

        }
        else
        {
            StartCoroutine(RemoveGlass());
        }
    }

    private void SetNormal() //Change material
    {
        RpcMaterialChange(false);
    }

    private void SetGlass() //Change material
    {
        RpcMaterialChange(true);
        StartCoroutine(SetBump());
    }

    private IEnumerator SwapColor(Color goal) //Change visiblity of normal material
    {
        playerMesh.material.SetFloat("_Glossiness", 0);
        float backUpTimer = 0;
        while (Mathf.Abs(playerMesh.material.color.a - goal.a) > 0.05f && backUpTimer < 2)
        {
            backUpTimer += Time.deltaTime;
            RpcColorChange(Color.Lerp(playerMesh.material.color, goal, 15 * Time.deltaTime));
            yield return null;

        }
        RpcColorChange(goal);
        if (cloaked) { SetGlass(); }
        else { playerMesh.material.SetFloat("_Glossiness", defSmooth); }
    }

    private IEnumerator SetBump() //Increase distorion for glass
    {
        float backUpTimer = 0;
        float i = 0;
        while (i < 50 && backUpTimer < 2)
        {
            i = Mathf.Lerp(i, 60, Time.deltaTime * 5);
            backUpTimer += Time.deltaTime;
            playerMesh.material.SetFloat("_BumpAmt", i);
            yield return null;
        }
    }
    private IEnumerator RemoveGlass() //Decrease distortion for glass
    {
        float backUpTimer = 0;
        float i = 50;
        while (i > 8 && backUpTimer < 2)
        {
            i = Mathf.Lerp(i, 0, Time.deltaTime * 5);
            backUpTimer += Time.deltaTime;
            playerMesh.material.SetFloat("_BumpAmt", i);
            yield return null;

        }

        SetNormal();
        visibilityColor.a = 1;
        StartCoroutine(SwapColor(visibilityColor));
    }

    [ClientRpc]
    private void RpcMaterialChange(bool glass)
    {
        Material mat = glass ? glassMat : defaultMat;
        playerMesh.material = mat;
    }

    [ClientRpc]
    private void RpcColorChange(Color col)
    {
        playerMesh.material.color = col;
    }

    #endregion 

    #region Unitymethods

    protected override void Awake()
    {
        gameObject.name = "Herbivore";
        base.Awake();
    }

    protected override void Start()
    {
        ComponentSearch();
        if (isLocalPlayer)
        {
            base.Start();
            SpawnCamera();
            canBarrellRoll = true;
            canTurn = true;
        }

    }

    protected override void Update()
    {
        if (isLocalPlayer && inputEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Q)) //also debug
            {
                ToggleCloak();
            }

            if (Input.GetKey(KeyCode.B))
            {
                Experience++;
            }
            base.Update();
            Dash();
            InteractionChecker();
            UIManager.Instance.UpdateMatchUI(this);
        }
    }

    protected override void FixedUpdate()
    {
        if (isLocalPlayer && inputEnabled)
        {
            base.FixedUpdate();
            if (!spawnedCam.GetComponent<CameraController>().FreeCamera)
            {
                MouseMove();
            }
            ApplyMovement();
        }
        AnimationChanger();
    }

    #endregion

}
