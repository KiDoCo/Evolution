using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController_1stPerson : MonoBehaviour
{
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


    public const float FOVValue = 75.0f;


    private Camera Camera1st;




    //camera reset point
    Vector3 startcameraPos = Vector3.zero;


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
        Camera1st = GetComponent<Camera>();
        distanceDamp = distanceDampValue; // reset damping values to fixed values
        rotationalDamp = rotationalDampValue; // --"--


        startcameraPos = cameraPos; //set camera default location
        
        Camera1st.fieldOfView = 60f;
        //hide everything but default layer, NEEDED to hide carnivore from camera, carnivore is in different layer;
        Camera1st.cullingMask = 1 << 0; 

    }

    public void FixedUpdate()
    {

        SetDampening(); //this has to be first
        FollowRot();
        Stabilize();


    }

    public void InstantiateCamera(Character test)
    {
        //if (test.isLocalPlayer)
        {
            Target = test.transform;
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
        //HUOM!!!! vaihda  tämän rivin koodissa transform.positionin ja target positionin +-merkit, jos cameraPos:in z asetetaan uudestaan editorissa positiiviseen arvoon :O
        Quaternion toRot = Quaternion.LookRotation(transform.position - target.position + new Vector3(0,-0.2f,0), target.up); 

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
    /// Camera follows more stricktly when doing these things
    /// </summary>
    public void SetDampening()
    {
        if (target.GetComponent<Carnivore>().InputVector.x != 0)
        {
            distanceDamp = strafeDamp;

        }

        else if (target.GetComponent<Carnivore>().InputVector.y != 0)
        {
            distanceDamp = verticalDamp;
        }

        else if (target.GetComponent<Carnivore>().InputVector.y < 0 || target.GetComponent<Carnivore>().InputVector.y > 0)
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
