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
    [SerializeField] Vector3 cameraPos = new Vector3(0f, 2f, -10f);
    private float distanceDamp = 10f;
    [SerializeField] float distanceDampValue = 0.4f;
    [SerializeField] float distanceDampValueB = 0.02f; //used in reverse movement
    [SerializeField] float distanceDampValueV = 0.02f; //used in vertical movement
    private float rotationalDamp = 10f;
    [SerializeField] float rotationalDampValue = 5f;
    [SerializeField] float RotationsSpeed = 2f; //camera rotation speed
    public float resetLerpValue = 5; // camera reset speed

  
    //FOV
    public float FOVValue = 60f;
    [HideInInspector] public float m_FieldOfView = 60f;


    //reset point
    Vector3 resetPos = Vector3.zero;


    //Followrot(); values
    Vector3 targetPos = Vector3.zero;
    public float lookSmooth = 100f;
    public Vector3 pivotPoint = new Vector3(0, 1, 0);

    


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

    void FixedUpdate()
    {
        if (target == null) Debug.LogError("Camera needs a target");

        if (pivotpoint == null) Debug.LogError("Camera needs a pivotpoint to look at");
        
      
        
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
            distanceDamp = distanceDampValueB;
        }
        else if (target.GetComponent<Herbivore>().isMovingVertical)
        {
            distanceDamp = distanceDampValueV;
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
        targetPos = target.position + pivotPoint; //välissä pivotpoint
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
            Vector3 resetLoc = Vector3.Lerp(cameraPos, resetPos, resetLerpValue * Time.deltaTime);
            cameraPos = resetLoc;
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

            //lisää summa cameran cameraPos arvoon
            cameraPos = sum * cameraPos;
          
        }
        else if (!FreeCamera)
        {
            ResetCamera();
        }
    }
}
