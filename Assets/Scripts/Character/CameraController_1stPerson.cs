using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController_1stPerson : MonoBehaviour
{



    //[SerializeField] Transform target;

    public Transform target;

    //values
    [SerializeField] Vector3 cameraPos = new Vector3(0f, -0.2f, -1f);
    [SerializeField] protected float distanceDamp = 10f;
    [SerializeField] protected float rotationalDamp = 10f;
    Vector3 velocity = Vector3.one;

    //fixed damp values (Set Dampening käyttää näitä)
    [SerializeField] protected float distanceDampValue = 0.025f;
    [SerializeField] protected float rotationalDampValue = 10f;
    
    [SerializeField] protected float strafeDamp = 0.2f;
    [SerializeField] protected float verticalDamp = 0.2f;
    [SerializeField] protected float rotationYDamp = 12f;
    [SerializeField] protected float fastRotationDamp = 1000;
    
    
    [HideInInspector] public float m_FieldOfView = 60f;
    public float FOVValue = 30f;

    
    public Camera Camera1st;

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
    Vector3 lookAtTarget;
    private float desiredDist;
    [SerializeField] Vector3 cameraOffset = Vector3.zero;


    //camera reset point
    Vector3 startcameraPos = Vector3.zero;


#pragma warning restore
    Transform Target
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

    protected void Start()
    {

        distanceDamp = distanceDampValue; // reset damping values to fixed values
        rotationalDamp = rotationalDampValue; // --"--
       

        m_FieldOfView = FOVValue; // set camera Field of view to fixed value
        startcameraPos = cameraPos; //set camera default location
        if (target == null) Debug.LogError("Missing target to look at");
        Camera1st.fieldOfView = 60f;
        Camera1st.cullingMask = (1 << 0 | 1<<15); //hide everything but default and ground layers, NEEDED to hide carnivore from camera, carnivore is in different layer;


        distance = Vector3.Distance(transform.position, target.position * cameraOffset.z);
        desiredDist = distance;

        colCamera = transform.GetComponent<Camera>();
        CameraClipPlanePoints(distance, ref desiredClipPoints);
    }

    public void FixedUpdate()
    {

        if (target == null) return;


        CameraClipPlanePoints(GetAdjustedDistance(target.position), ref adjustedClipPoints);
        CheckColliding(target.position);

        SetDampening(); //this has to be first
        FollowRot();
        Stabilize();
        CameraFOV();
        

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

    public struct ClipPanePoints
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

    //find clippoints and save them to arrays
    public ClipPanePoints CameraClipPlanePoints(float distance, ref Vector3[] intoArray)
    {
        intoArray = new Vector3[5];

        ClipPanePoints clipPlanePoints = new ClipPanePoints();
        Transform transform = colCamera.transform;
        Vector3 pos = transform.position;

        float halfFOV = (colCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float aspect = colCamera.aspect;

        float height = Mathf.Tan(halfFOV) * (distance);
        float width = (height * aspect);
        float tmp = colCamera.nearClipPlane;

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

    //detect collisions at clippoints
    private bool CollisionDetectedAtClipPoint(Vector3[] clipPoints, Vector3 fromPosition)
    {
        for (int i = 0; i < clipPoints.Length; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
            float Distance = Vector3.Distance(clipPoints[i], fromPosition);

            if (Physics.Raycast(ray, out hit, Distance, CollisionMask))
            {
                return true;
            }
        }
        return false;
    }

    //adjust the distance to closest position where camera collides
    private float GetAdjustedDistance(Vector3 from)
    {
        for (int i = 0; i < adjustedClipPoints.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(from, adjustedClipPoints[i] - from, out hit, distance, CollisionMask))
            {

                if (distance == -1)
                {
                    minCollisionDistance = hit.distance;
                }

                else
                {
                    if (hit.distance < distance)
                    {
                        minCollisionDistance = hit.distance;
                    }
                }
            }
        }

        if (minCollisionDistance == -1)
        {
            minCollisionDistance = 0f;
            return minCollisionDistance;
        }
        else
            return minCollisionDistance;
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

    /// <summary>
    /// Follows target movement and rotation smoothly (using distanceDamp and rotationalDamp values, cameraPos is camera distance from target)
    /// </summary>
    public void FollowRot()
    {
        //movement
        Vector3 toPos = target.position - (target.rotation * cameraPos);
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;

        //Rotation
        Quaternion toRot = Quaternion.LookRotation( transform.position - target.position, target.up); //HUOM!!!! vaihda  tämän rivin koodissa transform.positionin ja target positionin +-merkit, jos cameraPos:in z asetetaan uudestaan editorissa positiiviseen arvoon :O
       
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, rotationalDamp * Time.deltaTime);
        transform.rotation = curRot;
        
    }
   

    /// <summary>
    /// Needed to stabilize camera
    /// </summary>
    public void Stabilize()
    {

        float z = transform.eulerAngles.z;
        //Debug.Log(z);
        transform.Rotate(0, 0, -z);

    }

   
    public void ResetCamera() //back to original fixed cameraPos point
    {
        cameraPos = startcameraPos;
    }

    /// <summary>
    /// changing camera FOV with F key (in testing)
    /// </summary>
    public void CameraFOV()
    {
        if (Input.GetKeyDown(KeyCode.F)) //|| target.GetComponent<Carnivore2>().isDashing) // if carnie is charging, FOV increases to make "cool" effect
        {
            Debug.Log("FOV");
            m_FieldOfView = 90f;
            Camera1st.fieldOfView = m_FieldOfView;
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            m_FieldOfView = FOVValue; //reset camera FOV default
            Camera1st.fieldOfView = m_FieldOfView;
        }


    }

    /// <summary>
    /// Camera follows more stricktly when doing these things
    /// </summary>
    public void SetDampening() 
    {
        if (target.GetComponent<Carnivoremove>().isStrafing || target.GetComponent<Carnivore>().isStrafing) 
        {
            distanceDamp = strafeDamp;
            
        }
        else if (target.GetComponent<Carnivoremove>().isMovingVertical || target.GetComponent<Carnivore>().isMovingVertical)
        {
            distanceDamp = verticalDamp;
        }
        else if (target.GetComponent<Carnivore>().isReversing)
        {
            distanceDamp = distanceDampValue;
        }
        else if (target.GetComponent<Carnivore>().isMovingForward)
        {
            distanceDamp = distanceDampValue;
        }
       // else if (target.GetComponent<Carnivoremove>().rotationY)
       // {
       //     rotationalDamp = rotationYDamp;
      //  }
        else 
            distanceDamp = distanceDampValue;

    }

}
