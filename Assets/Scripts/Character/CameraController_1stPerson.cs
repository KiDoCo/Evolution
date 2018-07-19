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

    }

    public void FixedUpdate()
    {

        if (target == null) return;

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
