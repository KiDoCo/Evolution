using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    private static Transform target;
    [SerializeField] Vector3 defaultDistance = new Vector3(0f, 2f, -10f);
    [SerializeField] float distanceDamp = 10f;
    [SerializeField] float rotationalDamp = 10f;
    public Vector3 velocity = Vector3.one;
    public bool freeCamera;
    public static CameraController cam;

    Transform myT;


    // CAMERA CONTROLS
    public float speedH = 2.0f;
    public float speedV = 2.0f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Awake()
    {
        //lisää singleton
        cam = this;
        myT = transform;
    }


    void FixedUpdate()
    {
       SmoothFollow();
       
       
        Stabilize();
        ControlCamera();

        //Vector3 toPos = target.position + (target.rotation * defaultDistance);
        //Vector3 curPos = Vector3.Lerp(myT.position, toPos, distanceDamp * Time.deltaTime);
        // myT.position = curPos;

        // Quaternion toRot = Quaternion.LookRotation(target.position - myT.position, target.up);
        // Quaternion curRot = Quaternion.Slerp(myT.rotation, toRot, rotationalDamp * Time.deltaTime);
        //myT.rotation = curRot;
    }
    void SmoothFollow()
    {
        Vector3 toPos = target.position + (target.rotation * defaultDistance);

        Vector3 curPos = Vector3.SmoothDamp(myT.position, toPos, ref velocity, distanceDamp);
        myT.position = curPos;
        if (!freeCamera)
        {
            //rotation, same up direction
            myT.LookAt(target, target.up);
        }
        
    }

    public  void InstantiateCamera(move test)
    {
        target = test.transform;
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
    void FreeCamera()
    {
        if (freeCamera)
        {
            Debug.Log("FREE CAMERA!");
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }
}
