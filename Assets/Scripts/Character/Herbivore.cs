using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;


public class Herbivore : Character
{

    Quaternion originZ;
    Quaternion currentZ;
    //objects
    public Transform myT;

    //object references
    [HideInInspector] public static Herbivore herbiv;
    [SerializeField] new GameObject  CameraClone;
    [SerializeField] GameObject Camera3rd;
    public bool mouseInput;

    Quaternion targetRotation;
    public Quaternion TargetRotation
    {
        get { return targetRotation; }
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


    /// <summary>
    /// Checks for interaction when player enters the corals bounding box
    /// </summary>
    protected virtual void CmdInteractionChecker()
    {
        for (int i = 0; Gamemanager.Instance.FoodPlaceList.Count > i; i++)
        {
            if (GetComponent<Collider>().bounds.Intersects(Gamemanager.Instance.FoodPlaceList[i].GetComponent<FoodBaseClass>().GetCollider().bounds))
            {
                Eat(Gamemanager.Instance.FoodPlaceList[i]);
            }
        }
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
        }    }
    protected override void Update()
    {
        if (isLocalPlayer)
        {
            base.Update();
            CmdInteractionChecker();

            UIManager.Instance.UpdateMatchUI(this);
        }
    }

    protected override void FixedUpdate()
    {
if(!isLocalPlayer)return;
        {
            base.FixedUpdate();
            if (!CameraClone.GetComponent<CameraController>().FreeCamera)
            {
                MouseMove();
            }
            Restrict();

            if (!rolling)
            {
                Stabilize();
            }
        }
    }

    public void GetEaten(float dmg)
    {
        TakeDamage(dmg);
    }}
