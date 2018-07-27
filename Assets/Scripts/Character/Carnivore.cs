﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Carnivore : Character
{

    [SerializeField] protected bool canMouseMove = true;

    //character stats
    public float stamina;
    private bool isEating;
    private int killCount;

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
    [SerializeField] private float damage = 1;
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

    protected override void AnimationChanger()
    {
        m_animator.SetBool("IsMoving", isMoving);
        m_animator.SetBool("IsEating", isEating);
        m_animator.SetBool("IsCharging", charging);
    }

    protected override void EndGame()
    {
        end = true;
        UIManager.Instance.MatchResultScreen(this);
    }

    public void RestoreSpeed()
    {
        defaultSpeed = 1.0f;
    }

    private void ComponentSearch()
    {
        m_animator = gameObject.GetComponent<Animator>();
        cameraClone = Instantiate(cameraClone);
        cameraClone.GetComponent<CameraController_1stPerson>().InstantiateCamera(this);
        playerCam = CameraClone.GetComponent<Camera>();
    }

    #region EatMethods

    /// <summary>
    /// checks if carnivore hits the player and starts invoking eat
    /// </summary>
    private void EatChecker()
    {
        for (int i = 0; i < InGameManager.Instance.HerbivorePrefabs.ToArray().Length; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(InGameManager.Instance.HerbivorePrefabs[i].GetComponent<Collider>().bounds))
            {
                Eat(InGameManager.Instance.HerbivorePrefabs[i].GetComponent<Herbivore>());
            }
        }
    }

    /// <summary>
    /// Damages the herbivore if player presses the eat button
    /// </summary>
    /// <param name="col"></param>
    private void Eat(Character col)
    {
        if (col.GetType() == typeof(Herbivore))
        {
            if (InputManager.Instance.GetButton("Eat"))
            {
                Herbivore vor = col as Herbivore;
                Debug.Log("mums, mums....");
                StartCoroutine(EatCoolDown());
                EatHerbivore(slowDown, vor.Health);
                vor.GetEaten(damage);
            }
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
        isMoving = InputVector.normalized.magnitude != 0 ? true : false;
        if (!hitTarget || CollisionCheck())
        {
            transform.Translate(inputVector);
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
            for (int i = 0; i < InGameManager.Instance.HerbivorePrefabs.ToArray().Length; i++)
            {
                if (GetComponent<Collider>().bounds.Intersects(InGameManager.Instance.HerbivorePrefabs[i].GetComponent<Collider>().bounds))
                {
                    HitCheck(InGameManager.Instance.HerbivorePrefabs[i].GetComponent<Herbivore>());

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
                //Kill target
                vor.GetEaten(damage * 2);
                EatHerbivore(0.5f, vor.Health);
            }
            else
            {
                Debug.Log("Hurt herbivore");
                //1 damage
                vor.GetEaten(damage);
            }
            hitTarget = true;
        }
        else if (CollisionCheck() && charging)
        {
            Debug.Log("osu");
            hitTarget = true;
            //Stun for 1.5s
        }
    }

    #endregion

    #region UnityMethods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        if (isLocalPlayer)
        {
            base.Start();
            ComponentSearch();
            UIManager.Instance.InstantiateInGameUI(this);
            EventManager.ActionAddHandler(EVENT.RoundEnd, EndGame);
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
            EatChecker();
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

    #endregion
}

