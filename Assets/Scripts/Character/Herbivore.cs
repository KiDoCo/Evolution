using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Herbivore : Character
{

    Quaternion originZ;
    Quaternion currentZ;
    private bool Iseating;
    //objects
    public Transform myT;

    //object references
    [HideInInspector] public static Herbivore herbiv;
    [SerializeField] GameObject Camera3rd;



    public void MouseMove()
    {
        float v = verticalSpeed * Input.GetAxis("Mouse Y");
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        transform.Rotate(v, h, 0);

    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        cameraClone = Instantiate(Camera3rd);
        cameraClone.GetComponent<CameraController>().target = this.transform;
        SFXsource = transform.GetChild(3).GetComponent<AudioSource>();
        m_animator = gameObject.GetComponent<Animator>();
        UIManager.Instance.InstantiateInGameUI(this);
        canBarrellRoll = true;
        canTurn = true;
    }

    protected override void Update()
    {
        base.Update();

        if (isMoving)
        {
            m_animator.SetBool("isMoving", true);
        }
        if (!isMoving)
        {
            m_animator.SetBool("isMoving", false);
        }
        m_animator.SetBool("isEating", Iseating);

        if (Input.GetKey(KeyCode.N))
        {
            Iseating = true;
        }
        else
        {
            Iseating = false;
        }
    }

    protected override void FixedUpdate()
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
