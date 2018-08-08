using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Carnivore : Character
{

    [SerializeField] protected bool canMouseMove = true;
    [SerializeField] private GameObject carnivoreMesh;

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
    private bool eatIsplaying = false;
    [SyncVar]
    private bool charging = false;
    private float momentumTimer = 0;
    private float speed;

    #endregion

    #region Eat Variables
    //Eat
    [SerializeField] private float eatCooldown;
    [SerializeField] [Range(0, 1)] private float slowDown;

    #endregion

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

    //methods
    [Command]
    protected override void CmdAnimationChanger()
    {
        //Swim animation
        m_animator.SetBool("IsMoving", isMoving);
        HUDController.Instance.PredatorMouthAnim.SetBool("isMoving", isMoving);

        if (!eatIsplaying)
        {
            //Eating animation
            m_animator.SetBool("IsEating", eating);
            HUDController.Instance.PredatorMouthAnim.SetBool("isEating", eating);
            eatIsplaying = true;
        }

        //Charge animation
        m_animator.SetBool("IsCharging", charging);
        HUDController.Instance.PredatorMouthAnim.SetBool("isCharging", charging);

        //Turn animation
        m_animator.SetFloat("FloatX", Mathf.Clamp(InputManager.Instance.GetAxis("Horizontal"), -1, 1));
    }

    #region variableCheckers

    private void ChargeChecker(bool temp)
    {
        charging = temp;
    }


    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        if (!canMouseMove) return;

        mouseV = charging ? verticalSpeed / 10 * Input.GetAxis("Mouse Y") : verticalSpeed * Input.GetAxis("Mouse Y");
        mouseH = charging ? horizontalSpeed / 10 * Input.GetAxis("Mouse X") : horizontalSpeed * Input.GetAxis("Mouse X");

        transform.Rotate(mouseV, mouseH, 0);
    }

    #endregion

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

    [ServerCallback]
    private void EatChecker()
    {
        foreach (Character p in NetworkGameManager.Instance.InGamePlayerList)
        {
            if (GetComponent<Collider>().bounds.Intersects(p.GetComponent<Collider>().bounds))
            {
                if (col.GetType() == typeof(Herbivore))
                {
                    if (eating)
                    {
                        Eat(p);
                    }
                }
            }
            else if (!CollisionCheck() && charging)
            {
                Debug.Log("osu");
                hitTarget = true;
            }
        }
    }

    /// <summary>
    /// Damages the herbivore if player presses the eat button
    /// </summary>
    /// <param name="col"></param>
    private void Eat(Character col)
    {
        Herbivore vor = col as Herbivore;
        vor.Health--;
        Debug.Log("mums, mums....");
        EatHerbivore(slowDown, vor.Health);
        vor.GetEaten(damage);
    }

    private void EatInput()
    {
        if (InputManager.Instance.GetButtonDown("Eat", InputManager.Instance.GetButton("Eat")) && !eating)
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
        eatIsplaying = false;
        Debug.Log("Restoring");
        RestoreSpeed();
    }

    #endregion

    #region MovementMethods

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

        if (InputManager.Instance.GetAxis("Vertical") != 0 || InputManager.Instance.enabled == true)
        {
            if (currentInput != InputManager.Instance.GetAxis("Vertical")) ForwardVelocity = 0;

            currentInput = InputManager.Instance.GetAxis("Vertical");

            ForwardVelocity += InputManager.Instance.GetAxis("Vertical") * accPerSec * 2;
        }

        //deceleration when stopping movement
        else
        {
            Debug.Log("Called");
            ForwardVelocity = Mathf.SmoothStep(ForwardVelocity, 0, -decPerSec);
        }

        Z = (Vector3.forward * ForwardVelocity) * Time.deltaTime;

    }

    protected override void ApplyMovement()
    {
        inputVector = X + Y + Z;
        if (!hitTarget || CollisionCheck())
        {
            transform.Translate(inputVector * defaultSpeed);
            lastposition = curPos;
            curPos = transform.position;
            pos = curPos - lastposition;
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

        if (charging)
        {
            momentumTimer += Time.deltaTime;
        }
        else
        {
            if (momentumTimer != 0)
                momentumTimer = 0;
        }
        Z += (Vector3.forward * momentumTimer * speed) * Time.deltaTime;
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
            charging = true;
        }
    }

    private IEnumerator ChargeForward(float ass)
    {
        speed = ass;
        while (momentumTimer < chargeTime || hitTarget)
        {
            Mathf.Lerp(spawnedCam.GetComponent<Camera>().fieldOfView, spawnedCam.GetComponent<Camera>().fieldOfView + 2 * momentumTimer, 10 * Time.deltaTime);
            FieldOfView += momentumTimer;
            for (int i = 0; i < InGameManager.Instance.HerbivorePrefabs.ToArray().Length; i++)
            {
                if (GetComponent<Collider>().bounds.Intersects(InGameManager.Instance.HerbivorePrefabs[i].GetComponent<Collider>().bounds))
                {
                    HitCheck(InGameManager.Instance.HerbivorePrefabs[i].GetComponent<Herbivore>());
                    yield break;
                }
            }
            yield return null;
        }
        charging = false;
        hitTarget = false;
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
        charging = true;
        yield return new WaitForSeconds(coolDownTime);
        hitTarget = false;
        onCooldown = false;
    }

    /// <summary>
    /// Checks if we hit herbivore during charge
    /// </summary>
    /// <param name="herb"></param>
    private void HitCheck(Character herb)
    {
        if (herb.GetType() == typeof(Herbivore) && charging)
        {
            Herbivore vor = herb as Herbivore;
            Debug.Log("charge");
            if ((momentumTimer / chargeTime) >= 0.4f)
            {
                Debug.Log("Kill herbivore");
                vor.GetEaten(damage * 2);
                EatHerbivore(0.5f, vor.Health);
            }
            else
            {
                Debug.Log("Hurt herbivore");
                vor.GetEaten(damage);
            }
            hitTarget = true;
        }

    }

    #endregion

    #region UnityMethods

    protected override void Awake()
    {
        gameObject.name = "Carnivore";
        base.Awake();
    }

    protected override void Start()
    {
        if (isLocalPlayer)
        {
            base.Start();
            m_animator = gameObject.GetComponent<Animator>();
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
            InputManager.Instance.enabled = InputEnabled ? true : false;
            EatInput();
            base.Update();
            Charge();
            if (isServer)
            {
                EatChecker();
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            base.FixedUpdate();
            MouseMove();
            ApplyMovement();
            CmdAnimationChanger();
        }

    }

    #endregion
}

