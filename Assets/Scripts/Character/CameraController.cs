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
    public bool reset;

    //values
    Vector3 velocity = Vector3.one;
    [SerializeField] Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] float distanceDamp = 10f;
    [SerializeField] float distanceDampValue = 0.4f;
    [SerializeField] float distanceDampValueB = 0.02f; //used in reverse movement
    [SerializeField] float distanceDampValueV = 0.02f; //used in vertical movement
    [SerializeField] float RotationsSpeed = 2f; //camera rotation speed
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
    Vector3 startOffset = Vector3.zero;


    //Followrot2(); values
    Vector3 targetPos = Vector3.zero;
    public float lookSmooth = 100f;
    public Vector3 targetPosOffset = new Vector3(0, 1, 0);

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

        if (target == null)
        {
            Debug.Log("Camera needs a target");
            return;
        }
    public void InstantiateCamera(Character test)
    {
        Target = test.transform;
    }

    void GetInput()
    {
        Input.GetAxisRaw("Mouse X");
        Input.GetAxisRaw("Mouse Y");
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
        test.CameraClone.GetComponent<CameraController>().Target = InGameManager.Instance.DeathCameraPlace.transform;
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
        Vector3 toPos = target.position + (target.rotation * offset);

        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;

    }

    void SmoothRotate()
    {
        // rotation, same up direction
        transform.LookAt(pivotpoint, target.up);
    }

    void FollowRot() //camera is looking position of the target with the offset!
    {
        targetPos = target.position + targetPosOffset; 
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position, target.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookSmooth * Time.deltaTime);
    }

    void Stabilize()
    {
        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
    }

    void ResetCamera()
    {
        if (!FreeCamera) 
        {

            Vector3 resetLoc = Vector3.Lerp(offset, startOffset, resetLerpValue * Time.deltaTime);
            //Vector3 resetLoc = Vector3.SmoothDamp(offset, startOffset, ref velocity, distanceDamp);
            offset = resetLoc;

            //offset = startOffset; //direct reset
        }
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

            Quaternion camTurnAngleH = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * RotationsSpeed, Vector3.up); //laske horisontaalinen liike



            Quaternion camTurnAngleV = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * RotationsSpeed, Vector3.right);// laske vertikaalinen liike


            //laske kulmat yhteen
            Quaternion sum = camTurnAngleH * camTurnAngleV;
            //Quaternion sum = camTurnAngleH * camTurnAngleV; // normal sum


            //lisää summa cameran offset arvoon
            offset = sum * offset;

        }
        else if (!FreeCamera)
        {
            ResetCamera();
        }
    }

    private void Start()
    {
        GetInput();
        startOffset = offset;
        m_FieldOfView = FOVValue;  // set camera Field of view to fixed value in editor
        distanceDamp = distanceDampValue;

    }

    void FixedUpdate()
    {
        if (target == null) Debug.LogError("Camera needs a target");

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

    [System.Serializable]
    public class CollisionHandler
    {
        public LayerMask collisionLayer;
        [HideInInspector] public bool colliding = false;
        [HideInInspector] public Vector3[] adjustedCameraClipPoints;
        [HideInInspector] public Vector3[] desiredCameraClipPoints;
        Camera camera;

        public void Initialize(Camera cam)
        {
            camera = cam;
            adjustedCameraClipPoints = new Vector3[5];
            desiredCameraClipPoints = new Vector3[5];
        }


        //get clip points and pass the values into array (Quaternion = cameras rotation)
        public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
        {
            if (!camera) return;

            //clear the contents of intoArray
            intoArray = new Vector3[5];

            //find clippoint coordinates with relation to our camera position
            float z = camera.nearClipPlane;

            // "cushion" between cameras position and collision
            float x = Mathf.Tan(camera.fieldOfView / 3.41f) * z;
            float y = x / camera.aspect;


            //top left 
            intoArray[0] = (atRotation) * new Vector3(-x, y, z) + cameraPosition; //added and rotated the point relative to camera

            // top right
            intoArray[1] = (atRotation) * new Vector3(x, y, z) + cameraPosition;

            // bottom left
            intoArray[2] = (atRotation) * new Vector3(-x, -y, z) + cameraPosition;

            // bottom right
            intoArray[3] = (atRotation) * new Vector3(x, -y, z) + cameraPosition;

            //camera's position
            intoArray[4] = cameraPosition - camera.transform.forward;
        }

        //check collisions with raycasts
        bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
        {
            for (int i = 0; i < clipPoints.Length; i++)
            {
                Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
                float distance = Vector3.Distance(clipPoints[i], fromPosition);
                if (Physics.Raycast(ray, distance, collisionLayer))
                {
                    return true;

                }

            }
            return false;
        }

        //finds out the distance from target and how far to move
        public float GetAdjustedDistanceWithRayFrom(Vector3 from)
        {
            float distance = -1;
            if (distance == -1)
                return 0;
            else
                return distance;
        }
        //checks if colliding and if needed to move
        public void CheckColliding(Vector3 targetPosition)
        {

        }
    }
}
