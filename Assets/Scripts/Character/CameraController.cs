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
    private Vector3 velocity = Vector3.one;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
    [SerializeField] private float distanceDamp = 10f;
    [SerializeField] private float distanceDampValue = 0.4f;
    [SerializeField] private float distanceDampValueB = 0.02f; //used in reverse movement
    [SerializeField] private float distanceDampValueV = 0.02f; //used in vertical movement
    [SerializeField] private float RotationSpeed = 2f; //camera rotation speed
    public float resetLerpValue = 5; // camera reset speed

    #region Orbit

    public float xRotation = -20;
    public float yRotation = -180;
    public float maxXRotation = 25;
    public float minXRotation = -85;
    public float vOrbitSmooth = 150;
    public float hOrbitSmooth = 150;
    #endregion

    //FOV
    public float FOVValue = 60f;
    [HideInInspector] public float m_FieldOfView = 60f;

    //reset point
    Vector3 resetPos = Vector3.zero;

    //Followrot(); values
    Vector3 targetPos = Vector3.zero;
    [SerializeField] protected float lookSmooth = 100f;

    [Header("Collision varibles")]
    public float MinDistance = 0.5f;
    public float MaxDistance = 2.0f;
    public LayerMask CollisionMask;
    private Camera colCamera;
    private Vector3[] desiredClipPoints = new Vector3[5];
    private Vector3[] adjustedClipPoints = new Vector3[5];
    private float distance;
    private bool colliding = false;
    private float minCollisionDistance;
    private float desiredDist;


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

    public void InstantiateCamera(Character test)
    {
        Target = test.transform;
    }

    public struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
        public Vector3 MiddleRight;
        public Vector3 MiddleLeft;
        public Vector3 MiddleUp;
        public Vector3 MiddleDown;
    }

    /// <summary>
    /// calculates clippoints for 4 corners of nearClipPlane
    /// also saves position of camera as one clippoint
    /// </summary>
    public ClipPlanePoints CameraClipPlanePoints(float distance, ref Vector3[] intoArray)
    {
        intoArray = new Vector3[5];

        ClipPlanePoints clipPlanePoints = new ClipPlanePoints();
        Transform transform = colCamera.transform;
        Vector3 pos = transform.position;

        float halfFOV = (colCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad; // half Of cameras field of view changed from degree to radians
        float aspect = colCamera.aspect;

        float height = Mathf.Tan(halfFOV) * (distance); //height of nearClipPlane
        float width = (height * aspect); //width of nearClipPlane

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

        //Camera's position point
        intoArray[4] = pos;

        return clipPlanePoints;
    }

    /// <summary>
    /// go through all clipPoints and cast ray from target to each of them
    /// return true if something hit at one of the clippoints
    /// </summary>
    private bool CollisionDetectedAtClipPoint(Vector3[] clipPoints, Vector3 fromPosition)
    {
        print("clippoints lenght: " + clipPoints.Length);

        for (int i = 0; i < clipPoints.Length; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
            float Distance = Vector3.Distance(clipPoints[i], fromPosition);

            if (Physics.Raycast(ray, out hit, Distance, CollisionMask)) //ray is cast to ach of clipPoints to see if they are occluded
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// adjust the distance to closest position where camera collides
    /// returns shortest distance from target to collision point
    /// </summary>
    private float GetAdjustedDistance(Vector3 from)
    {
        for (int i = 0; i < adjustedClipPoints.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(from, adjustedClipPoints[i] - from, out hit, distance, CollisionMask))
            {
                if (hit.distance < distance)
                {
                    minCollisionDistance = hit.distance;
                }
            }
        }

        //check that distance is not less than -1
        if (minCollisionDistance <= -1)
        {
            minCollisionDistance = 0f;
            return minCollisionDistance;
        }
        else
            return minCollisionDistance;
    }

    //used to check if colliding or not by using targets position and cameras adjustedClipPoints
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

    void OrbitTarget() //restict camera angle
    {
        xRotation += -Input.GetAxisRaw("Mouse X") * vOrbitSmooth * Time.deltaTime;
        yRotation += -Input.GetAxisRaw("Mouse Y") * hOrbitSmooth * Time.deltaTime;

        if (xRotation > maxXRotation)
        {
            xRotation = maxXRotation;
        }
        if (xRotation < minXRotation)
        {
            xRotation = minXRotation;
        }
    }

    /// <summary>
    /// Checks if target is backing up or changing altitude, then changes camera values
    /// </summary>
    void SetDampening()
    {
        if (target.GetComponent<Herbivore>().InputVector.normalized.y < 0)
        {
            distanceDamp = distanceDampValueB;
        }
        else if (target.GetComponent<Herbivore>().InputVector.normalized.z != 0)
        {
            distanceDamp = distanceDampValueV;
        }
        else
            distanceDamp = distanceDampValue;
    }

    void SmoothFollow() // follow every frame
    {
        distance = minCollisionDistance - Mathf.Abs(0 + offset.x) * 2; //reduce absolute of cameraOffset.z from shortest collision point to get distance
        distance = Mathf.Clamp(distance, MinDistance, MaxDistance); //Clamp distance between minDistance and maxDistance

        print("distance: " + distance);

        Vector3 toPos;
        toPos = target.position - (target.forward * distance);
        transform.position = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
    }

    void SmoothRotate()
    {
        // rotation, same up direction
        transform.LookAt(pivotpoint, target.up);
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
            Quaternion camTurnAngleH = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * RotationSpeed, Vector3.up); //laske horisontaalinen liike
            Quaternion camTurnAngleV = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * RotationSpeed, Vector3.right);// laske vertikaalinen liike

            //laske kulmat yhteen

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

            Vector3 resetLoc = Vector3.Lerp(offset, resetPos, resetLerpValue * Time.deltaTime);
            //Vector3 resetLoc = Vector3.SmoothDamp(offset, startOffset, ref velocity, distanceDamp);
            offset = resetLoc;

            //offset = startOffset; //direct reset
        }
    }

    void FollowRot() //camera is looking position of the target with the offset!
    {
        targetPos = target.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position, target.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookSmooth * Time.deltaTime);
    }

    private void LateUpdate()
    {
        //change distance to player to max distance if not colliding
        if (!colliding)
        {
            if (minCollisionDistance < desiredDist)
            {
                minCollisionDistance += Time.deltaTime * 10f;
            }
            if (minCollisionDistance > desiredDist)
            {
                minCollisionDistance = desiredDist;
            }
        }
    }

    private void Start()
    {
        resetPos = offset;
        m_FieldOfView = FOVValue;  
        distanceDamp = distanceDampValue;
        transform.position = target.position + (target.rotation * offset);

        distance = Vector3.Distance(transform.position, target.position * offset.z); 
        desiredDist = distance;

        colCamera = transform.GetComponent<Camera>();
        CameraClipPlanePoints(distance, ref desiredClipPoints);
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        //testiä varten
        if (Input.GetKey(KeyCode.R))
        {
            ResetCamera();
        }
        SetDampening();
        ControlCamera();
        SmoothFollow();
        FollowRot();
        Stabilize();
        Restrict();
    }

}