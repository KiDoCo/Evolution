using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{

    //objects
    public Transform target;
    public Transform pivotpoint;
    public Camera Camera3rd;
   
    //bools
    public bool FreeCamera;

    //values
    Vector3 velocity = Vector3.one;
    [SerializeField] Vector3 cameraPos = new Vector3(0f, 0.2f, -2f);
    private float distanceDamp = 10f;
    [SerializeField] protected float distanceDampValue = 0.4f;
    [SerializeField] protected float reverseDistanceDamp = 0.02f; //used in reverse movement
    [SerializeField] protected float verticalDistanceDamp = 0.02f; //used in vertical movement
    private float rotationalDamp = 10f;
    [SerializeField] protected float rotationalDampValue = 5f;
    [SerializeField] protected float rotationSpeed = 2f; //camera rotation speed
    [SerializeField] protected float resetLerpValue = 5; // camera reset speed

    //FOV
    public float FOVValue = 60f;
    [HideInInspector] public float m_FieldOfView = 60f;

    //reset point
    Vector3 resetPos = Vector3.zero;

    //Followrot(); values
    Vector3 targetPos = Vector3.zero;
    [SerializeField] protected float lookSmooth = 100f;
    [SerializeField] protected Vector3 pivotPos = new Vector3(0, 0, 0);


    [Header("Collision variables")]
    public bool addDefaultAsNormal;
    public float moveSpeed = 5;
    public string activeStateID;
    private float turnSmoothing = 0.1f;
    Vector3 targetPosition;
    [HideInInspector] Vector3 targetPositionOffset;
    float x;
    float y;
    float lookAngle;
    float tiltAngle;
    float offsetX;
    float offsetY;
    float smoothX = 0;
    float smoothY = 0;
    float smoothXVelocity = 0;
    float smoothYVelocity = 0;
    public float minAngle = 35;
    public float maxAngle = 35;
    Transform camTrans;
    public LayerMask player;
    float destination;
    

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
        resetPos = cameraPos;
        m_FieldOfView = FOVValue;  // set camera Field of view to fixed value in editor
        distanceDamp = distanceDampValue;
        rotationalDamp = rotationalDampValue;
    }

    //public void SetCamtrans(Camera cam)
    //{
    //    camTrans = cam.transform;
    //    print("setting camTrans");
    //    FixPositions();
    //}


    void FixedUpdate()
    {
        if (target == null) Debug.LogError("Camera needs a target");

        if (pivotpoint == null) Debug.LogError("Camera needs a pivotpoint to look at");

        FixPositions();
        SetDampening();
        ControlCamera();
        SmoothFollow();
        //SmoothRotate(); //rotation method1
        FollowRot(); //alternative rotation method
        Stabilize();
        Restrict();
    }

    public void InstantiateCamera(Character test)
    {
        Target = test.transform;
    }

    private void FixPositions()
    {
        //print("here");
        //float targetZ = camTrans.localPosition.z;
        //float actualZ = targetZ;

        CameraCollision();

        //Vector3 targetP = camTrans.localPosition;
        //targetP.z = Mathf.Lerp(targetP.z, actualZ, Time.deltaTime * 5);
        //transform.position = targetP;

        //float targetFov = m_FieldOfView;

        //if (targetFov < 1)
        //{
        //    targetFov = 2;
        //}
        //Camera3rd.fieldOfView = Mathf.Lerp(Camera3rd.fieldOfView, targetFov, Time.deltaTime * 5);
    }

    void CameraCollision()
    {
        Vector3 adjustedPosition = Vector3.zero;

        Debug.DrawLine(target.position, transform.position, Color.blue);

        float step = Mathf.Abs(Vector3.Distance(target.position, transform.position));
        int stepCount = 4;
        float stepIncremental = step / stepCount;

        Vector3 dir = transform.forward;

        for (int s = 0; s < stepCount + 1; s++)
        {
            RaycastHit hit;
            Vector3 secondOrigin = target.position - (dir * s) * stepIncremental;

            for (int i = 0; i < 4; i++)
            {
                Vector3 direction = Vector3.zero;
                switch (i)
                {
                    case 0:
                        direction = transform.right;
                        break;
                    case 1:
                        direction = -transform.right;
                        break;
                    case 2:
                        direction = transform.up;
                        break;
                    case 3:
                        direction = -transform.up;
                        break;
                }

                Debug.DrawRay(secondOrigin, direction, Color.red);

                if (Physics.Raycast(secondOrigin, direction, out hit, 1))
                {
                    float distance = Vector3.Distance(secondOrigin, target.position);
                    adjustedPosition.z = -(distance / 2);
                }
            }


        }
    }


    protected void Restrict()
    {
        if (transform.rotation.x > 15)
        {
            float x = transform.eulerAngles.x;
            transform.Rotate(-x, 0, 0);
        }

        if (transform.rotation.x < -15)
        {
            float x = transform.eulerAngles.x;
            transform.Rotate(-x, 0, 0);
        }
    }

    /// <summary>
    /// Sets the camera target to fixed point
    /// </summary>
    /// <param name="source"></param>
    public void CameraPlaceOnDeath(Character test)
    {
        test.CameraClone.GetComponent<CameraController>().Target = Gamemanager.Instance.DeathCameraPlace.transform;
    }

    /// <summary>
    /// Checks if target is backing up or changing altitude, then changes camera values
    /// </summary>
    void SetDampening()
    {
        if (target.GetComponent<Herbivore>().isReversing)
        {
            distanceDamp = reverseDistanceDamp;
        }
        else if (target.GetComponent<Herbivore>().isMovingVertical)
        {
            distanceDamp = verticalDistanceDamp;
        }
        else
            distanceDamp = distanceDampValue;
    }

    void SmoothFollow()// follow every frame
    {
        Vector3 toPos = target.position + (target.rotation * cameraPos);
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;
    }
    void SmoothRotate()
    {
        // rotation, same up direction
        transform.LookAt(pivotpoint, target.up);
    }


    void FollowRot() //camera is looking position of the target with the offset! no pivotpoint needed GOOD METHOD
    {
        targetPos = target.position + pivotPos; //välissä pivotpoint
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position, target.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookSmooth * Time.deltaTime);
    }

    void HandleOffsets()
    {
        if(offsetX !=0)
        {
            offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
        }
        if (offsetY != 0)
        {
            offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
        }
    }

    void Stabilize()
    {
        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
    }


    void ControlCamera()// Camera input 
    {

        if (Input.GetMouseButton(2))
        {
            FreeCamera = true;
        }
        else
        {
            FreeCamera = false;
        }

        if (FreeCamera)
        {

            Quaternion camTurnAngleH = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, Vector3.up); //laske horisontaalinen liike
            Quaternion camTurnAngleV = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeed, Vector3.right);// laske vertikaalinen liike

            //laske kulmat yhteen
            Quaternion sum = camTurnAngleH * camTurnAngleV;

            //lisää summa cameran cameraPos arvoon
            cameraPos = sum * cameraPos;

        }
        else if (!FreeCamera)
        {
            ResetCamera();
        }
    }
    void ResetCamera()
    {
        if (!FreeCamera)
        {
            Vector3 resetLoc = Vector3.Lerp(cameraPos, resetPos, resetLerpValue * Time.deltaTime);
            cameraPos = resetLoc;
        }
    }
}
