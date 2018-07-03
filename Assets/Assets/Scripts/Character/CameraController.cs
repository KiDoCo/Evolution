using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;

    //values
   
    [SerializeField] protected Vector3 cameraPos = new Vector3(0f, 2f, -10f);
    
    Vector3 resetPoint = Vector3.zero;
    [SerializeField] protected float distanceDamp = 10f;
    [SerializeField] protected float rotationalDamp = 10f;
    [SerializeField] protected Vector3 velocity = Vector3.one;
    [SerializeField] protected float rotationsSpeed = 2f;
    [SerializeField] protected float m_FieldOfView = 60f;

    public Camera FollowCam;
    public bool FreeCamera;
        
    //Script references
    [HideInInspector] public Herbivore Herbivorescript;
    [HideInInspector] public static CameraController Cam;

    
    
         

    private void Start()
    {
        //lisää singleton
        Cam = this;
        //save reset point
        resetPoint = cameraPos;
        FollowCam.fieldOfView = m_FieldOfView;
        
    }

    void FixedUpdate()
    {

        ControlCamera();
        CameraFOV();
        if (Input.GetKey(KeyCode.R))
        {
            ResetCamera();
        }       
        FollowRot();
        //SmoothFollow();
        Stabilize();
        
    }

    /// <summary>
    /// Follow target every frame with Lookat -method
    /// </summary>
    void SmoothFollow()
    {
        Vector3 toPos = target.position + (target.rotation * cameraPos);
        //add smoothness
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;
       
            //rotation, same up direction
            transform.LookAt(target, target.up);
               
    }


    /// <summary>
    /// Alternative camera follow calculating rotations
    /// </summary>
    void FollowRot()
    {

        Vector3 toPos = target.position + (target.rotation * cameraPos);
        
        //calculate smoothness
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        
        //add smoothvalues to transform
        transform.position = curPos;

        //calculate rotations
        Quaternion toRot = Quaternion.LookRotation(target.position - transform.position, target.up);
        //add smoothness
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, rotationalDamp * Time.deltaTime);
        transform.rotation = curRot;
        

    }

    /// <summary>
    /// Reset z rotation every fame
    /// </summary>
    void Stabilize()  
    {

        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);

    }
    

    /// <summary>
    /// CAMERA INPUT
    /// </summary>
    void ControlCamera()
    {
        
        if (Input.GetMouseButton(2)) 
        {
           
            FreeCamera = true;
        }
       else
        {
            FreeCamera = false;            
        }

        //MOVE CAMERA WITH MOUSE
        if (FreeCamera) 
        {
            Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationsSpeed, Vector3.up);
            cameraPos = camTurnAngle * cameraPos;
        }
        else if (!FreeCamera)
        {
            ResetCamera();
        }
    }

    public void ConstructCamera(Character source)
    {
        target = source.transform;
    }

    void ResetCamera()
    {
        cameraPos = resetPoint;
    }

    /// <summary>
    /// changing camera FOV with F key (in testing)
    /// </summary>
    void CameraFOV()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            m_FieldOfView = 80f;
            FollowCam.fieldOfView = m_FieldOfView;
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            m_FieldOfView = 60f;
            FollowCam.fieldOfView = m_FieldOfView;
        }

       
    }

}
