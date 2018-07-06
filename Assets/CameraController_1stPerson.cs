using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController_1stPerson : MonoBehaviour
{



    [SerializeField] Transform target;

    //values
    [SerializeField] Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] float distanceDamp = 10f;
    [SerializeField] float rotationalDamp = 10f;
    [SerializeField] Vector3 velocity = Vector3.one;
    
    [HideInInspector] public float m_FieldOfView = 60f;
    public float FOVValue = 30f;

    
    public Camera Camera1stCamera;

    //Script references
    [HideInInspector] public static CameraController_1stPerson cam1;
    [HideInInspector] public Carnivore2 Carnivorescript;

    //camera reset point
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

        cam1 = this;
        m_FieldOfView = FOVValue; // set camera Field of view to fixed value
        startOffset = offset; //set camera default location
        if (target == null) Debug.Log("Missing look target");
        Camera1stCamera.fieldOfView = 60f;
        Camera1stCamera.cullingMask = 1 << 0; //hide everything but default layer, NEEDED to hide carnivore from camera, carnivore is in different layer;

    }

    void FixedUpdate()
    {

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
    /// Follows target movement and rotation smoothly (using distanceDamp and rotationalDamp values, offset is camera distance from target)
    /// </summary>
    void FollowRot()
    {
        //movement
        Vector3 toPos = target.position - (target.rotation * offset);
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        transform.position = curPos;

        //Rotation
        Quaternion toRot = Quaternion.LookRotation(- transform.position +  target.position, target.up);
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, rotationalDamp * Time.deltaTime);
        transform.rotation = curRot;
        
    }

    /// <summary>
    /// Needed to stabilize camera
    /// </summary>
    void Stabilize()
    {

        float z = transform.eulerAngles.z;
        //Debug.Log(z);
        transform.Rotate(0, 0, -z);

    }

   
    void ResetCamera() //back to original fixed offset point
    {
        offset = startOffset;
    }

    /// <summary>
    /// changing camera FOV with F key (in testing)
    /// </summary>
    void CameraFOV()
    {
        if (Input.GetKeyDown(KeyCode.F) || Carnivore2.carniv.IsCharging) // if carnie is charging, FOV increases to make "cool" effect
        {
            Debug.Log("FOV");
            m_FieldOfView = 90f;
            Camera1stCamera.fieldOfView = m_FieldOfView;
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            m_FieldOfView = FOVValue; //reset camera FOV default
            Camera1stCamera.fieldOfView = m_FieldOfView;
        }


    }


}
