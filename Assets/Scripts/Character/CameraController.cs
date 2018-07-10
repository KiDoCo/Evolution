using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{



    [SerializeField] public Transform target;

    //values
    [SerializeField] Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] float distanceDamp = 10f;
    [SerializeField] float distanceDampValue = 0.4f;
    [SerializeField] float distanceDampValueB = 0.02f; //used in reverse movement
    [SerializeField] float distanceDampValueV = 0.02f; //used in vertical movement
    [SerializeField] float rotationalDamp = 10f;
    [SerializeField] float rotationalDampValue = 5f;
    [SerializeField] Vector3 velocity = Vector3.one;
    [SerializeField] float RotationsSpeed = 2f;

    public bool FreeCamera;
    public Camera Camera3rd;
    public bool reset;

    //Script references
    [HideInInspector] public Herbivore Herbivorescript;
    [HideInInspector] public static CameraController cam;

    //FOV
    public float FOVValue = 60f;
    [HideInInspector] public float m_FieldOfView = 60f;


    //reset point
    Vector3 startOffset = Vector3.zero;
#pragma warning restore
    public Transform Target
    {
        get
        {
            return target;
        }

        set
        {
            target = value;
        }
    }

    private void Start()
    {

        cam = this;
        startOffset = offset;
        m_FieldOfView = FOVValue;  // set camera Field of view to fixed value in editor
        distanceDamp = distanceDampValue;
        rotationalDamp = rotationalDampValue;


    }

    void FixedUpdate()
    {
        if (target == null) return;

        SetDampening();
        ControlCamera();
        if (Input.GetKey(KeyCode.R))
        {
            ResetCamera();
        }
        FollowRot();
        Stabilize();
       


    }
    public void InstantiateCamera(Character test)
    {
        Target = test.transform;
    }

    /// <summary>
    /// Sets the camera target to fixed point
    /// </summary>
    /// <param name="source"></param>
    public void CameraPlaceOnDeath(Character test)
    {
        test.CameraClone.GetComponent<CameraController>().Target = Gamemanager.Instance.DeathCameraPlace.transform;
    }

    void SmoothFollow()// follow every frame
    {
        Vector3 toPos = target.position + (target.rotation * offset);

        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;

        //rotation, same up direction
        transform.LookAt(target, target.up);

    }


    void FollowRot()//Alternative camera follow
    {

        Vector3 toPos = target.position + (target.rotation * offset);

        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;

        Quaternion toRot = Quaternion.LookRotation(target.position - transform.position, target.up);
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, rotationalDamp * Time.deltaTime);
        transform.rotation = curRot;
    }


    void Stabilize()
    {

        float z = transform.eulerAngles.z;
        //Debug.Log(z);
        transform.Rotate(0, 0, -z);

    }

    void ResetCamera()
    {
        
            offset = startOffset;
        
        
    }

    void ControlCamera()// Camera input
    {

        if (Input.GetMouseButton(2))
        {
            //FreeCamera = !FreeCamera;
            FreeCamera = true;
        }
        else
        {
            FreeCamera = false;
        }

        if (FreeCamera)
        {
            Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * RotationsSpeed, Vector3.up);
            offset = camTurnAngle * offset;
        }
        else if (!FreeCamera)
        {
            ResetCamera();
        }
    }

    /// <summary>
    /// Checks if target is backing up or changing altitude, then changes camera values
    /// </summary>
    void SetDampening()
    {
        if (target.GetComponent<Herbivore>().isReversing)
        {
            distanceDamp = distanceDampValueB;
        }
        else if (target.GetComponent<Herbivore>().isMovingVertical)
        {
            distanceDamp = distanceDampValueV;
        }

        else
            distanceDamp = distanceDampValue;
        
    }


}
