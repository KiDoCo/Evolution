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
    [SerializeField] Vector3 cameraOffset = new Vector3(0f, 0.2f, -2f);
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
        resetPos = cameraOffset;
        m_FieldOfView = FOVValue;  // set camera Field of view to fixed value in editor
        distanceDamp = distanceDampValue;
        rotationalDamp = rotationalDampValue;
    }


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
        Vector3 desiredPos = new Vector3(cameraOffset.x, cameraOffset.y + 0.2f, cameraOffset.z);
        float actualZ = desiredPos.z;
        float actualX = desiredPos.x;
        float actualY = desiredPos.y;

        CameraCollision(ref actualZ, ref actualX, ref actualY);

        //Vector3 correctedPos = new Vector3(actualX, actualY - 0.2f, actualZ);
        //cameraOffset = correctedPos;
        Vector3 targetP = cameraOffset;
        targetP.z = Mathf.Lerp(targetP.z, actualZ, Time.deltaTime * 5);
        targetP.x = Mathf.Lerp(targetP.x, actualX, Time.deltaTime * 5);
        targetP.y = Mathf.Lerp(targetP.y, actualY, Time.deltaTime * 5);
        cameraOffset = targetP;
        //velocity = targetP;

        float targetFov = m_FieldOfView;

        if (targetFov < 1)
        {
            targetFov = 1;
        }
        Camera3rd.fieldOfView = Mathf.Lerp(Camera3rd.fieldOfView, targetFov, Time.deltaTime * 5);
    }

    void CameraCollision(ref float actualZ, ref float actualX, ref float actualY)
    {
        Debug.DrawLine(target.position, transform.position, Color.blue);

        float step = Vector3.Distance(target.position, transform.position);
        int stepCount = 2;
        float stepIncremental = step / stepCount;
        RaycastHit hit;
        Vector3 dir = transform.forward;

        if (Physics.SphereCast(target.position,0.5f, dir, out hit, step))
        {
            float distance = Vector3.Distance(hit.point, target.position);
            actualZ = -(distance * 0.5f);
        }

        for (int s = 0; s < stepCount + 1; s++)
        {
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
                    float distanceZ = Vector3.Distance(secondOrigin, target.position);
                    actualZ = -(distanceZ * 0.5f);

                    float distanceX = Vector3.Distance(secondOrigin, hit.point);
                    actualX = -(distanceX * 0.5f);

                    //float distanceY = Vector3.Distance(secondOrigin, hit.point);
                    //actualY = -(distanceY * 0.5f);

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
        Vector3 toPos = target.position + (target.rotation * cameraOffset);
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
            cameraOffset = sum * cameraOffset;

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
            Vector3 resetLoc = Vector3.Lerp(cameraOffset, resetPos, resetLerpValue * Time.deltaTime);
            cameraOffset = resetLoc;
        }
    }
}
