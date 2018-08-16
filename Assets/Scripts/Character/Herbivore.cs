using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;


public class Herbivore : Character
{

    //Component search
    private Quaternion originZ;
    private Quaternion currentZ;
    [SyncVar]
    private Color visibilityColor;
    public Material defaultMat;
    public Material glassMat;
    private ParticleSystem blood;

    [SyncVar]
    private bool cloaked = false;
    private bool canDash = true;
    private bool HasRespawned;
    private bool dashing;
    [SyncVar]
    private float cloakTimer;
    [SyncVar]
    private float invincibleTimer;
    [SyncVar]
    private float horMov;
    private float surTime;
    public float defSmooth = 0;

    [SyncVar]
    protected bool hunt = false;

    //timer values
    [SerializeField] private float dashTime = 2f;

    #region Character stats

    protected float health = 2;
    private const float healthMax = 2;
    private const float waitTime = 1.0f;
    private const float deathpenaltytime = 2.0f;

    [SyncVar]
    protected float experience = 0;
    private int deathcount;

    #endregion

    #region Getters&Setters

    public float Maxhealth
    {
        get
        {
            return healthMax;
        }
    }

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

    protected float DashTime
    {
        get
        {
            return dashTime;
        }

        set
        {
            dashTime = Mathf.Clamp(value, 0, Mathf.Infinity);
        }
    }

    public float InvincibleTimer
    {
        get
        {
            return invincibleTimer;
        }

        set
        {
            invincibleTimer = Mathf.Clamp01(value);
        }
    }

    #endregion

    //Methods

    #region Geteaten methods

    //methods
    [Command]
    private void CmdTakeDamage(float amount)
    {
        Debug.Log("dmg : " + amount);
        blood.Play();
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        InvincibleTimer = 1.0f;
        Health -= amount;
    }

    [ClientRpc]
    private void RpcTakeDamage(float amount)
    {
        Debug.Log("dmg : " + amount);
        blood.Play();
        EventManager.SoundBroadcast(EVENT.PlaySFX, SFXsource, (int)SFXEvent.Hurt);
        InvincibleTimer = 1.0f;
        Health -= amount;
    }

    public void GetEaten(float dmg)
    {
        if (InvincibleTimer <= 0)
        {
            if (isServer)
                RpcTakeDamage(dmg);
            else
                CmdTakeDamage(dmg);

        }
    }

    #endregion

    [ClientRpc]
    public void RpcMusicChanger(bool hunted)
    {
        if (!isLocalPlayer) return;

        if (!hunted)
        {
            if (mClip != MusicEvent.Ambient)
            {
                EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Ambient);
                mClip = MusicEvent.Ambient;
            }

        }
        else
        {
            if (mClip != MusicEvent.Hunting)
            {
                EventManager.SoundBroadcast(EVENT.PlayMusic, musicSource, (int)MusicEvent.Hunting);
                mClip = MusicEvent.Hunting;
            }

        }
    }

    [Command]
    private void CmdMusicChangerCheck()
    {
        InGameManager.Instance.MusicChecker(this, false);
    }

    /// <summary>
    /// Checks for interaction when player enters the eatable bounding box
    /// </summary>
    protected void InteractionChecker()
    {
        for (int i = 0; InGameManager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(InGameManager.Instance.FoodPlaceList[i].GetComponent<FoodBaseClass>().GetCollider().bounds))
            {
                Eat(InGameManager.Instance.FoodPlaceList[i]);
            }
        }


    }

    protected void Death()
    {
        if (InGameManager.Instance.LifeCount > 0)
        {
            FindObjectOfType<Carnivore>().KillCount++;
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

    protected override void SpawnCamera()
    {
        GameObject mapCam = GameObject.FindGameObjectWithTag("MapCamera");
        if (mapCam != null)
            Destroy(mapCam);

        spawnedCam = Instantiate(cameraPrefab);
        spawnedCam.GetComponent<CameraController>().InstantiateCamera(this);
    }

    #region AnimationMethods

    protected void AnimationChanger()
    {
        if (!isServer)
            CmdAnimationChanger(isMoving, horMov, eating, Experience);
        else
            RpcAnimationChanger(isMoving, horMov, eating, Experience);
    }

    [Command]
    protected void CmdAnimationChanger(bool move, float hor, bool eat, float exp)
    {
        playerMesh.SetBlendShapeWeight(0, Mathf.Clamp(exp, 25, 100));
        m_animator.SetBool("isEating", eat && !m_animator.GetCurrentAnimatorStateInfo(1).IsName("Eat"));
        m_animator.SetBool("isMoving", move);
        m_animator.SetFloat("Evolution", exp);
        m_animator.SetFloat("Horizontal", hor);
    }

    [ClientRpc]
    protected void RpcAnimationChanger(bool move, float hor, bool eat, float exp)
    {
        playerMesh.SetBlendShapeWeight(0, Mathf.Clamp(exp, 25, 100));
        m_animator.SetBool("isEating", eat && !m_animator.GetCurrentAnimatorStateInfo(1).IsName("Eat"));
        m_animator.SetBool("isMoving", move);
        m_animator.SetFloat("Evolution", exp);
        m_animator.SetFloat("Horizontal", hor);
    }

    #endregion

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
        if (InputManager.Instance.GetButtonDown("Ability"))
        {
            if (canDash)
            {
                StartCoroutine(DashTimer());
            }
        }
    }

    protected IEnumerator DashTimer()
    {
        defaultSpeed = 3;
        yield return new WaitForSeconds(dashTime);
        defaultSpeed = 1;
        CoolDownTime = C_CooldownTime;
        canDash = false;
        yield return StartCoroutine(CoolTimer());
    }

    protected IEnumerator CoolTimer()
    {
        while (CoolDownTime > 0) yield return null;
        canDash = true;
    }

    #endregion

    #region movement

    public void MouseMove()
    {
        if (!InputEnabled) return;

        mouseV = verticalSpeed * Input.GetAxis("Mouse Y");
        mouseH = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(mouseV, mouseH, 0);
    }

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
        horMov = Mathf.Clamp(InputManager.Instance.GetAxis("Horizontal"), -1, 1);
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

        isMoving = inputVector.normalized.magnitude != 0 ? true : false;

        if (CollisionCheck())
        {
            transform.Translate((X + Z) * defaultSpeed);
            transform.Rotate(Y * defaultSpeed);
        }
    }

    #endregion

    #region Cloak

    private void ApplyCloak()
    {
        if (inputVector.normalized.magnitude == 0 && mouseH == 0 && mouseV == 0)
        {
            cloakTimer -= Time.deltaTime;
        }
        else
        {
            cloakTimer = 10.0f;
            cloaked = false;
        }

        ToggleCloak();

    }

    private void ToggleCloak() //Change this to non-toggle if needed
    {
        if (cloakTimer <= 0)
        {
            visibilityColor.a = 0;
            StartCoroutine(SwapColor(visibilityColor));
            cloaked = true;

        }
        else if (!cloaked && cloakTimer > 0)
        {
            StartCoroutine(RemoveGlass());

        }
    }

    private void SetNormal() //Change material
    {
        MaterialChange(false);
    }

    private void SetGlass() //Change material
    {
        MaterialChange(true);
        StartCoroutine(SetBump());
    }

    private IEnumerator SwapColor(Color goal) //Change visiblity of normal material
    {
        if (cloaked) yield break;
        playerMesh.material.SetFloat("_Glossiness", 0);
        float backUpTimer = 0;
        while (Mathf.Abs(playerMesh.material.color.a - goal.a) > 0.05f && backUpTimer < 2)
        {
            backUpTimer += Time.deltaTime;
            ColorChange(Color.Lerp(playerMesh.material.color, goal, 15 * Time.deltaTime));
            yield return null;

        }

        ColorChange(goal);
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

    private void MaterialChange(bool glass)
    {
        Material mat = glass ? glassMat : defaultMat;
        if (playerMesh.material == mat) return;
        playerMesh.material = mat;
        if (!isServer)
            CmdMaterialChange(glass);
        else
            RpcMaterialChange(glass);
    }

    private void ColorChange(Color col)
    {
        if (playerMesh.material.color == col) return;
        playerMesh.material.color = col;
        if (!isServer)
            CmdColorChange(col);
        else
            RpcColorChange(col);
    }

    [Command]
    private void CmdMaterialChange(bool glass)
    {
        Material mat = glass ? glassMat : defaultMat;
        playerMesh.material = mat;
        transform.GetChild(3).gameObject.SetActive(!glass);
    }

    [ClientRpc]
    private void RpcMaterialChange(bool glass)
    {
        Material mat = glass ? glassMat : defaultMat;
        playerMesh.material = mat;
        transform.GetChild(3).gameObject.SetActive(!glass);
    }

    [Command]
    private void CmdColorChange(Color col)
    {
        playerMesh.material.color = col;
    }

    [ClientRpc]
    private void RpcColorChange(Color col)
    {
        playerMesh.material.color = col;
    }

    #endregion

    private void ComponentSearch()
    {
        visibilityColor = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color;
        SFXsource = transform.GetChild(2).GetComponent<AudioSource>();
        blood = transform.GetChild(4).GetComponent<ParticleSystem>();
        triggerCollider = transform.GetChild(0).GetComponent<Collider>();
    }

    #region Unitymethods

    protected override void Awake()
    {
        gameObject.name = "Herbivore";
        base.Awake();
        ComponentSearch();
    }

    protected override void Start()
    {
        if (isLocalPlayer)
        {
            cloakTimer = 10.0f;
            base.Start();
            SpawnCamera();
        }

    }

    protected override void Update()
    {
        InvincibleTimer -= Time.deltaTime;
        if (isLocalPlayer)
        {
            if(!canDash)
            CoolDownTime -= Time.deltaTime;
            InputManager.Instance.EnableInput = InputEnabled;
            ApplyCloak();
            base.Update();
            Dash();
            InteractionChecker();
            CmdMusicChangerCheck();
            UIManager.Instance.UpdateMatchUI(this);

        }
    }

    protected override void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            base.FixedUpdate();
            if (!spawnedCam.GetComponent<CameraController>().FreeCamera)
            {
                MouseMove();
            }
            ApplyMovement();
            AnimationChanger();
        }
    }

    #endregion

}
