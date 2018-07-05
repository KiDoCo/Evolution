using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
#pragma warning disable
    private Transform                target;
    [SerializeField] private Vector3 defaultDistance = new Vector3(0f, 2f, -10f);
    [SerializeField] private float   distanceDamp    = 10f;
    [SerializeField] private float   rotationalDamp  = 10f;
    public Vector3                   velocity        = Vector3.one;
    public bool                      freeCamera;
    private Transform                myT;

    // CAMERA CONTROLS
    public float  speedH = 2.0f;
    public float  speedV = 2.0f;
    private float yaw    = 0.0f;
    private float pitch  = 0.0f;
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

    void SmoothFollow()
    {
        Vector3 toPos = Target.position + (Target.rotation * defaultDistance);

        Vector3 curPos = Vector3.SmoothDamp(myT.position, toPos, ref velocity, distanceDamp);
        myT.position = curPos;
        if (!freeCamera)
        {
            //rotation, same up direction
            myT.LookAt(Target, Target.up);
        }
        
    }

    public  void InstantiateCamera(Character test)
    {
        Target = test.transform;
    }
       
    void Stabilize()
    {

        float z = transform.eulerAngles.z;
        //Debug.Log(z);
        transform.Rotate(0, 0, -z);
    }

    void ControlCamera()
    {
        if (Input.GetMouseButtonDown(2))
        {
            freeCamera = true;
        }
        if (Input.GetMouseButtonUp(2))
        {
            freeCamera = false;
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

    public void FreeCamera()
    {
        if (freeCamera)
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

    void Awake()
    {
        myT = transform;
    }

    void FixedUpdate()
    {
        if (target == null) return;
       SmoothFollow();
        Stabilize();
        ControlCamera();
    }
}
