using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Carnivore : Character
{

    [SerializeField] protected bool canMouseMove = true;

    //character stats
    public float stamina;
    private bool isEating;
    private const float staminaValue = 20.0f;

    #region Charge Variables

    //Charge
    [SerializeField] private float coolDownTime;
    [SerializeField] private float chargeTime = 4;
    private Camera playerCam;
    private float defaultFov;
    private bool onCooldown = false;
    private bool hitTarget = false;
    private bool charging = false;
    private float momentumTimer = 0;
    private float speed;

    #endregion

    #region Eat Variables
    //Eat
    [SerializeField] private float eatCooldown;
    [SerializeField] private float xpReward;
    [SerializeField] private float damage;
    [SerializeField] [Range(0, 1)] private float slowDown;

    #endregion

    private float FieldOfView
    {
        get
        {
            return playerCam.fieldOfView;
        }
        set
        {
            playerCam.fieldOfView = Mathf.Clamp(value, 60f, 90f);
        }
    }

    //methods

    protected override void AnimationChanger()
    {
        m_animator.SetBool("IsMoving", isMoving);
        m_animator.SetBool("IsEating", isEating);
        m_animator.SetBool("IsCharging", charging);
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
                    Eat(p);
                }
            }
        }
    }

    private void Eat(Character col)
    {
        Herbivore vor = col as Herbivore;
        Debug.Log("mums, mums....");
        StartCoroutine(EatCoolDown());
        EatHerbivore(xpReward, slowDown);
        vor.GetEaten(damage);
    }

    private IEnumerator EatCoolDown()
    {
        yield return new WaitForSeconds(eatCooldown);
        RestoreSpeed();
    }

    #endregion

    #region MovementMethods

    /// <summary>
    /// Inputs for rotating with the mouse
    /// </summary>
    public void MouseMove()
    {
        if (!canMouseMove) return;

        float v = charging ? verticalSpeed / 10 * Input.GetAxis("Mouse Y") : verticalSpeed * Input.GetAxis("Mouse Y");
        float h = charging ? horizontalSpeed / 10 * Input.GetAxis("Mouse X") : horizontalSpeed * Input.GetAxis("Mouse X");

        if (v != 0 || h != 0)
        {
            isMoving = true;
        }
        m_animator.SetFloat("FloatX", Mathf.Clamp01(h) + InputManager.Instance.GetAxis("Horizontal"));
        m_animator.SetFloat("FloatY", Mathf.Clamp01(v) + InputManager.Instance.GetAxis("Vertical"));
        transform.Rotate(v, h, 0);
    }

    protected override void SidewayMovement()
    {
        X = new Vector3(1, 0, 0) * (Input.GetAxisRaw("Horizontal") * strafeSpeed) * Time.deltaTime;
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

        //deceleration when stopping movement
        else
        {
            ForwardVelocity = Mathf.SmoothStep(ForwardVelocity, 0, -decPerSec);
        }

        Z = (Vector3.forward * ForwardVelocity) * Time.deltaTime;

    }

    protected override void ApplyMovement()
    {
        inputVector = X + Y + Z;
        transform.Translate(inputVector);
    }

    #endregion

    #region ChargeMethods

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
            if(momentumTimer != 0)
            momentumTimer = 0;
        }
        GetComponent<Carnivore>().Z += (Vector3.forward * momentumTimer * speed) * Time.deltaTime;
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
            Mathf.Lerp(playerCam.fieldOfView, playerCam.fieldOfView + 2 * momentumTimer, 10 * Time.deltaTime);
            FieldOfView += momentumTimer;
            for (int i = 0; i < Gamemanager.Instance.HerbivorePrefabs.ToArray().Length; i++)
            {
                if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.HerbivorePrefabs[i].GetComponent<Collider>().bounds))
                {
                    HitCheck(Gamemanager.Instance.HerbivorePrefabs[i].GetComponent<Herbivore>());

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
        while (playerCam.fieldOfView > defaultFov + 2)
        {
            FieldOfView = Mathf.Lerp(playerCam.fieldOfView, defaultFov, Time.deltaTime);
            yield return null;
        }
        FieldOfView = defaultFov;
    }

    private IEnumerator ChargeCooldown()
    {
        onCooldown = true;
        charging = true;
        yield return new WaitForSeconds(coolDownTime);
        onCooldown = false;
    }

    private void HitCheck(Character herb)
    {
        if (herb.GetType() == typeof(Herbivore) && charging)
        {
            Herbivore vor = herb as Herbivore;
            Debug.Log("charge");
            if ((momentumTimer / chargeTime) >= 0.4f)
            {
                Debug.Log("Kill herbivore");
                //Kill target
                vor.gameObject.GetComponent<Herbivore>().GetEaten(500);
            }
            else
            {
                Debug.Log("Hurt herbivore");
                //1 damage
                vor.gameObject.GetComponent<Herbivore>().GetEaten(1);
            }
            hitTarget = true;
        }
        else if (GetComponent<Carnivore>().CollisionCheck() && charging)
        {
            Debug.Log("osu");
            hitTarget = true;
            //Stun for 1.5s
        }
    }

    #endregion

    public void EatHerbivore(float xp, float slow)
    {
        defaultSpeed *= slow;
    }

    public void RestoreSpeed()
    {
        defaultSpeed = 1.0f;
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
            m_animator = gameObject.GetComponent<Animator>();
            stamina = staminaValue;
            cameraClone = Instantiate(cameraClone);
            cameraClone.GetComponent<CameraController_1stPerson>().InstantiateCamera(this);
            cameraClone.name = "FollowCamera";
            UIManager.Instance.InstantiateInGameUI(this);
            playerCam = GetComponent<Carnivore>().CameraClone.GetComponent<Camera>();
            defaultFov = playerCam.fieldOfView;
            slowDown = 1 - slowDown;
        }
    }

    protected override void Update()
    {
        if (isLocalPlayer)
        {
            base.Update();
            Charge();

            if (isServer)
            {
                EatChecker();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                isEating = true;
            }
            else
            {
                isEating = false;
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            base.FixedUpdate();
            MouseMove();
            AnimationChanger();
            ApplyMovement();
        }
    }

}

