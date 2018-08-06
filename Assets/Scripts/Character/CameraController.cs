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

    [Header("Collision varibles")]
    public float MinDistance = 0.5f;
    public float MaxDistance = 2.0f;
    public float smoothTime;
    float duration = 2;
    public LayerMask CollisionMask;
    private Camera colCamera;
    private Vector3[] desiredClipPoints = new Vector3[5];
    private Vector3[] adjustedClipPoints = new Vector3[5];
    private float distance;
    private bool colliding;
    float actualZ;

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

        distance = Vector3.Distance(transform.position, target.position);
        actualZ = cameraOffset.z;

        colCamera = transform.GetComponent<Camera>();
        CameraClipPlanePoints(distance, ref desiredClipPoints);
    }


    void FixedUpdate()
    {
        if (target == null) Debug.LogError("Camera needs a target");

        if (pivotpoint == null) Debug.LogError("Camera needs a pivotpoint to look at");

        distance = Vector3.Distance(transform.position, target.position);
        CameraClipPlanePoints(distance, ref adjustedClipPoints);
        CheckColliding(target.position);


        SetDampening();
        ControlCamera();
        SmoothFollow();
        SmoothRotate(); //rotation method1
        //FollowRot(); //alternative rotation method
        Stabilize();
        Restrict();
    }

    public void InstantiateCamera(Character test)
    {
        Target = test.transform;
    }

    public struct ClipPanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }

    public ClipPanePoints CameraClipPlanePoints(float distance, ref Vector3[] intoArray)
    {
        intoArray = new Vector3[5];

        ClipPanePoints clipPlanePoints = new ClipPanePoints();
        Transform transform = colCamera.transform;
        Vector3 pos = transform.position;

        float halfFOV = (colCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float aspect = colCamera.aspect;

        float height = Mathf.Tan(halfFOV) * distance;
        float width = height * aspect;

        //LowerRight point
        clipPlanePoints.LowerRight = pos + transform.forward * distance;
        clipPlanePoints.LowerRight += transform.right * width;
        clipPlanePoints.LowerRight -= transform.up * height;
        intoArray[0] = clipPlanePoints.LowerRight;

        //LowerLeft point
        clipPlanePoints.LowerLeft = pos + transform.forward * distance;
        clipPlanePoints.LowerLeft -= transform.right * width;
        clipPlanePoints.LowerLeft -= transform.up * height;
        intoArray[1] = clipPlanePoints.LowerLeft;

        //UpperRight point
        clipPlanePoints.UpperRight = pos + transform.forward * distance;
        clipPlanePoints.UpperRight += transform.right * width;
        clipPlanePoints.UpperRight += transform.up * height;
        intoArray[2] = clipPlanePoints.UpperRight;    

        //UpperLeft point
        clipPlanePoints.UpperLeft = pos + transform.forward * distance;
        clipPlanePoints.UpperLeft -= transform.right * width;
        clipPlanePoints.UpperLeft += transform.up * height;
        intoArray[3] = clipPlanePoints.UpperLeft;

        //Camera position point
        intoArray[4] = transform.position;

        return clipPlanePoints;
    }

    private bool CollisionDetectedAtClipPoint(Vector3[] clipPoints, Vector3 fromPosition)
    {
        print("clippointt lenght: " + clipPoints.Length);
        for (int i = 0; i < clipPoints.Length; i++)
        {
            print("krooh");
            RaycastHit hit;
            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
            float Distance = Vector3.Distance(clipPoints[i], fromPosition);
            if (Physics.Raycast(ray, out hit, Distance, CollisionMask))            
            {
                print("collided");
                return true;
            }
        }
        print("didn't collide");
        return false;
    }

    private float GetAdjustedDistance(Vector3 from)
    {
        for (int i = 0; i < desiredClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, desiredClipPoints[i] - from);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, CollisionMask))
            {
                if (distance == -1)
                    distance = hit.distance;
                else
                {
                    if (hit.distance < distance)
                    {
                        distance = hit.distance;
                        actualZ = (hit.point.z + hit.normal.z * 0.5f);
                    }
                }
            }
        }

        if (distance == -1)
        {
            return 0.2f;
        }
        else
            return distance;
    }

    public void CheckColliding(Vector3 targetPosition)
    {
        if (CollisionDetectedAtClipPoint(adjustedClipPoints, targetPosition))
        {
            colliding = true;
        }
        else
        {
            colliding = false;
        }
    }

    private void FixPositions()
    {
        //TODO: get rid of shaking during lerping camera

        Vector3 desiredPos = new Vector3(cameraOffset.x, cameraOffset.y, cameraOffset.z);
        float actualZ = desiredPos.z;
        float actualX = desiredPos.x;
        float actualY = desiredPos.y;

        CameraCollision(ref actualZ, ref actualX, ref actualY);

        Vector3 correctedPos = new Vector3(actualX, actualY, actualZ);
        cameraOffset = correctedPos;
    }

    void CameraCollision(ref float actualZ, ref float actualX, ref float actualY)
    {
        //Debug.DrawLine(target.position, transform.position, Color.blue);

        //float step = Mathf.Abs(actualZ);
        float step = Vector3.Distance(target.position, transform.position);
        int stepCount = 3;
        float stepIncremental = step / stepCount;
        RaycastHit hit;
        Vector3 dir = transform.forward;


        if (Physics.Linecast(target.position, -target.forward, out hit, CollisionMask))
        {
            actualZ = -Mathf.Clamp((hit.distance * 0.9f), MinDistance, MaxDistance);
            //float distance = Vector3.Distance(hit.point, target.position);
            //actualZ = -(distance * 0.3f);
        }
        else
        {
            for (int s = 1; s < stepCount + 1; s++)
            {
                Vector3 secondOrigin = target.position - (dir * s) * stepIncremental;

                for (int i = 0; i < 5; i++)
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

                    if (Physics.Raycast(secondOrigin, direction, out hit, CollisionMask))
                    {

                        float distance = Vector3.Distance(secondOrigin, target.position);
                        actualZ = -(distance * 0.3f);

                        //actualX = -(hit.point.x + hit.normal.x * 0.5f);
                        //actualZ = -(hit.point.z + hit.normal.z * 0.5f);                       

                    }
                }
            }
        }
    }


    protected void Restrict()
    {
        //limit up and down rotation        
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
        Vector3 toPos;

        if (colliding)
        {
            //TODO: move camera away from collision

            toPos = target.position + (target.rotation * cameraOffset);
        }
        else
        {
            toPos = target.position + (target.rotation * cameraOffset);
        }


     
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
