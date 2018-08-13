using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Carnivore : Character
{

    [SerializeField] protected bool canMouseMove = true;
    [SerializeField] private GameObject carnivoreMesh;

    private Collider jawCollider;

    //character stats
    public float stamina;
    private int killCount;
    private float damage = 1;

    #region Charge Variables
    
    //Charge
    [SerializeField] private float coolDownTime;
    [SerializeField] private float chargeTime = 4;
    private float defaultFov;
    private bool onCooldown = false;
    private bool hitTarget = false;
    [SyncVar]
    private bool charging = false;
    private float momentumTimer = 0;
    private float speed;
    
    #endregion
    //Eat
    [SerializeField] private float eatCooldown;
    [SerializeField] [Range(0, 1)] private float slowDown;


    private float FieldOfView
    {
        get
        {
            return spawnedCam.GetComponent<Camera>().fieldOfView;
        }
        set
        {
            spawnedCam.GetComponent<Camera>().fieldOfView = Mathf.Clamp(value, 60f, 90f);
        }
    }

    public int KillCount
    {
        get
        {
            return killCount;
        }

        set
        {
            killCount = value;
        }
    }

    public bool Charging
    {
        get
        {
            return charging;
        }

        set
        {
            charging = value;
        }
    }

    //methods

    protected void AnimationChanger()
    {
        HUDController.Instance.PredatorMouthAnim.SetBool("isMoving", isMoving);
        HUDController.Instance.PredatorMouthAnim.SetBool("isEating", eating && !m_animator.GetCurrentAnimatorStateInfo(1).IsName("Eat"));
        HUDController.Instance.PredatorMouthAnim.SetBool("isCharging", Charging);


        if (!isServer)
            CmdAnimationChanger(isMoving, eating, Charging, Mathf.Clamp(InputManager.Instance.GetAxis("Horizontal"), -1, 1));
        else
            RpcAnimationChanger(isMoving, eating, Charging, Mathf.Clamp(InputManager.Instance.GetAxis("Horizontal"), -1, 1));
    }

    [Command]
    protected void CmdAnimationChanger(bool move, bool eat, bool charge, float hormov)
    {
        //Swim animation
        m_animator.SetBool("IsMoving", move);

        //Eating animation
        m_animator.SetBool("IsEating", eat && !m_animator.GetCurrentAnimatorStateInfo(1).IsName("Eat"));

        //Charge animation
        m_animator.SetBool("IsCharging", charge);

        //Turn animatio
        m_animator.SetFloat("FloatX", hormov);
    }

    [ClientRpc]
    protected void RpcAnimationChanger(bool move, bool eat, bool charge, float hormov)
    {
        //Swim animation
        m_animator.SetBool("IsMoving", move);

        //Eating animation
        m_animator.SetBool("IsEating", eat && !m_animator.GetCurrentAnimatorStateInfo(1).IsName("Eat"));

        //Charge animation
        m_animator.SetBool("IsCharging", charge);

        //Turn animatio
        m_animator.SetFloat("FloatX", hormov);
    }
    
    public void RestoreSpeed()
    {
        defaultSpeed = 1.0f;
    }

    protected override void SpawnCamera()
    {
        GameObject mapCam = GameObject.FindGameObjectWithTag("MapCamera");
        if (mapCam != null)
            Destroy(mapCam);

        spawnedCam = Instantiate(cameraPrefab);
        spawnedCam.GetComponent<CameraController_1stPerson>().InstantiateCamera(this);
    }

    #region EatMethods

    [Command]
    private void CmdEatChecker(bool charge)
    {
        if (!charge)
        {
            foreach (Character p in NetworkGameManager.Instance.InGamePlayerList)
            {
                if (p.GetType() == typeof(Herbivore))
                {
                    if (GetComponent<Collider>().bounds.Intersects(p.GetComponent<Collider>().bounds))
                    {
                        Debug.Log("Intersection");
                        InGameManager.Instance.EatChecker(this);
                    }
                }
            }
        }
        else
        {
            hitTarget = !CollisionCheck();
        }
    }

    public void Eat(Character col)
    {
        Herbivore vor = col as Herbivore;
        if (eating)
        {
            Debug.Log(vor + "  has been hit");
            vor.GetEaten(damage);
            eating = false;
        }
    }

    private void EatInput()
    {
        if (InputManager.Instance.GetButtonDown("Eat") && !eating)
        {
            eating = true;
            StartCoroutine(EatCoolDown());
        }
    }

    public void EatHerbivore(float slow, float hp)
    {
        defaultSpeed *= slow;
        if (hp <= 0)
        {
            killCount++;
        }
    }

    private IEnumerator EatCoolDown()
    {
        defaultSpeed *= slowDown;
        yield return new WaitForSeconds(eatCooldown);
        eating = false;
        Debug.Log("Restoring");
        RestoreSpeed();
    }

    #endregion

    #region MovementMethods

    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        if (!canMouseMove || !InputEnabled) return;

        mouseV = Charging ? verticalSpeed / 10 * Input.GetAxis("Mouse Y") : verticalSpeed * Input.GetAxis("Mouse Y");
        mouseH = Charging ? horizontalSpeed / 10 * Input.GetAxis("Mouse X") : horizontalSpeed * Input.GetAxis("Mouse X");

        transform.Rotate(mouseV, mouseH, 0);
    }

    protected override void SidewayMovement()
    {
        X = new Vector3(1, 0, 0) * (InputManager.Instance.GetAxis("Horizontal") * strafeSpeed) * Time.deltaTime;
    }

    protected override void UpwardsMovement()
    {
        Y = InputManager.Instance.GetAxis("Altitude") * Vector3.up * rotateSpeed * Time.deltaTime;
    }

    protected override void ForwardMovement()
    {

        if (InputManager.Instance.GetAxis("Vertical") != 0)
        {
            if (currentInput != InputManager.Instance.GetAxis("Vertical")) ForwardVelocity = 0;

            currentInput = InputManager.Instance.GetAxis("Vertical");

            ForwardVelocity += InputManager.Instance.GetAxis("Vertical") * accPerSec * 2;
        }
        else
        {
            //deceleration when stopping movement
            ForwardVelocity = Mathf.SmoothStep(ForwardVelocity, 0, -decPerSec);
        }

        Z = (Vector3.forward * ForwardVelocity) * Time.deltaTime;

    }

    protected override void ApplyMovement()
    {
        inputVector = X + Y + Z;
        isMoving = InputVector.normalized.magnitude != 0 ? true : false;
        
        if (CollisionCheck())
        {
            transform.Translate(inputVector * defaultSpeed);
        }
    }

    #endregion

    #region ChargeMethods

    /// <summary>
    /// Non overloaded method that is called in update
    /// </summary>
    private void Charge()
    {
        if (InputManager.Instance.GetButton("Ability"))
        {
            Charge(10);
        }

        if (Charging)
        {
            momentumTimer += Time.deltaTime;
        }
        else
        {
            if (momentumTimer != 0)
                momentumTimer = 0;
        }
    }

    private void ChargeChecker(bool temp)
    {
        Charging = temp;
    }

    public void Charge(float speed)
    {
        if (onCooldown)
        {
            return;
        }
        else
        {
            StartCoroutine(ChargeForward(speed));
            StartCoroutine(ChargeCooldown());
            Charging = true;
        }
    }

    private IEnumerator ChargeForward(float ass)
    {
        speed = ass;

        while (momentumTimer < chargeTime)
        {
            Mathf.Lerp(spawnedCam.GetComponent<Camera>().fieldOfView, spawnedCam.GetComponent<Camera>().fieldOfView + 2 * momentumTimer, 10 * Time.deltaTime);
            FieldOfView += momentumTimer;
            for (int i = 0; i < NetworkGameManager.Instance.InGamePlayerList.FindAll(x => x.GetType() == typeof(Herbivore)).ToArray().Length; i++)
            {
                if (jawCollider.bounds.Intersects(NetworkGameManager.Instance.InGamePlayerList.FindAll(x => x.GetType() == typeof(Herbivore))[i].GetComponent<Collider>().bounds))
                {
                    InGameManager.Instance.EatChecker(this);
                }
            }
            Z += InputManager.Instance.enabled ? (Vector3.forward * momentumTimer * speed) * Time.deltaTime : Vector3.zero;
            yield return null;
        }
        yield return RestoreFov();
    }

    private IEnumerator RestoreFov()
    {
        while (spawnedCam.GetComponent<Camera>().fieldOfView > defaultFov + 2)
        {
            FieldOfView = Mathf.Lerp(spawnedCam.GetComponent<Camera>().fieldOfView, defaultFov, Time.deltaTime);
            yield return null;
        }
        FieldOfView = defaultFov;
    }

    private IEnumerator ChargeCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(coolDownTime);
        Charging = false;
        hitTarget = false;
        onCooldown = false;
    }

    /// <summary>
    /// Checks if we hit herbivore during charge
    /// </summary>
    /// <param name="herb"></param>
    public void HitCheck(Herbivore herb)
    {
        if ((momentumTimer / chargeTime) >= 0.4f)
        {
            Debug.Log("Kill herbivore");
            herb.GetEaten(damage * 2);
            EatHerbivore(0.5f, herb.Health);
        }
        else
        {
            Debug.Log("Hurt herbivore");
            herb.GetEaten(damage);
        }
        hitTarget = true;
        charging = false;
    }

    #endregion

    #region UnityMethods

    protected override void Awake()
    {
        base.Awake();
        gameObject.name = "Carnivore";
        jawCollider = transform.GetChild(0).GetComponent<Collider>();
    }

    protected override void Start()
    {
        if (isLocalPlayer)
        {
            base.Start();
            SpawnCamera();
            defaultFov = spawnedCam.GetComponent<Camera>().fieldOfView;
            slowDown = 1 - slowDown;
            carnivoreMesh.SetActive(false);
        }
    }

    protected override void Update()
    {
        if (isLocalPlayer)
        {
            InputManager.Instance.EnableInput = InputEnabled;
            EatInput();
            base.Update();
            Charge();
            CmdEatChecker(Charging);

        }
    }

    protected override void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            base.FixedUpdate();
            MouseMove();
            ApplyMovement();
            AnimationChanger();
        }

    }

    #endregion
}

