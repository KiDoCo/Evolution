using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController2 : MonoBehaviour {

    
    public Transform target;
    
        public float lookSmooth = 100f;
        public Vector3 offsetFromTarget = new Vector3(0, 6, -9);
    public Vector3 targetPosOffset = new Vector3(0, 1, 0);
    public float distanceFromTarget = -1;

        public float xTilt = 10; //looking downward target


    public float zoomSmooth = 10;
    public float maxZoom = -2;
    public float minZoom = -15;


    //orbitsettings

        public float xRotation = -20;
        public float yRotation = -180;
        public float maxXRotation = 25;
        public float minXRotation = -85;
        public float vOrbitSmooth = 150;
        public float hOrbitSmooth = 150;
  
    //inputsettings
        public string ORBIT_HORIZONTAL_SNAP = "OrbitHorizontalSnap";
        public string ORBIT_HORIZONTAL = "OrbitHorizontal";
        public string ORBIT_VERTICAL = "OrbitVertical";
        public string ZOOM = "Mouse ScrollWheel";
  

    Vector3 destination = Vector3.zero;
    Vector3 targetPos = Vector3.zero;
    Herbivore herbiv; 
    float rotateVel = 0;

    float vOrbitInput, hOrbitInput, zoomInput, hOrbitSnapInput;
    
    [SerializeField] Vector3 velocity = Vector3.one;
    [SerializeField] float distanceDamp = 0.02f;

    void Start ()
    {
        SetCameraTarget(target);
        //makes sure camera is behind our target
        targetPos = target.position + targetPosOffset;
        destination = Quaternion.Euler(xRotation, yRotation, 0) * -Vector3.forward * distanceFromTarget;
        destination += targetPos;
        transform.position = destination;

    }
	
    void SetCameraTarget(Transform t)
    {
        target = t;
        if (target != null)
        {
            if (target.GetComponent<Herbivore>())
            {
                herbiv = target.GetComponent<Herbivore>();
            }
            else
                Debug.LogError("the camera's target needs a herbivore");
        }
        else
            Debug.LogError("You camera needs a target.");
    }


    void Update()
    {
        GetInput();
        OrbitTarget();
        ZoomInOnTarget();
    }
    

    private void FixedUpdate()
    {
        //moving
        //MoveToTarget();
        //FollowPos(); //vanha tapa toimii lookattarget2:n kanssa
        MoveToTarget2();


        //rotating
        //LookAtTarget();
        LookAtTarget2();
       // OrbitTarget();
    }

    void GetInput()
    {
        vOrbitInput = Input.GetAxisRaw(ORBIT_VERTICAL);
        hOrbitInput = Input.GetAxisRaw(ORBIT_HORIZONTAL);
        hOrbitSnapInput = Input.GetAxisRaw(ORBIT_HORIZONTAL_SNAP);
        zoomInput = Input.GetAxisRaw(ZOOM);
    }

    void OrbitTarget()
    {
        if(hOrbitSnapInput >0)
        {
            yRotation = -180;
        }
        xRotation += -vOrbitInput * vOrbitSmooth * Time.deltaTime;
        yRotation += -hOrbitInput * hOrbitSmooth * Time.deltaTime;

        if (xRotation > maxXRotation)
        {
            xRotation = maxXRotation;
        }
        if (xRotation < minXRotation)
        {
            xRotation = minXRotation;
        }
    }
    void ZoomInOnTarget()
    {
        distanceFromTarget += zoomInput * zoomSmooth * Time.deltaTime;
        if (distanceFromTarget > maxZoom)
        {
            distanceFromTarget = maxZoom;
        }
        if (distanceFromTarget < minZoom)
        {
            distanceFromTarget = minZoom;
        }
    }


    void MoveToTarget()
    {
        destination = herbiv.TargetRotation * offsetFromTarget;
        destination += target.position;
        transform.position = destination;
    }

    void MoveToTarget2() //uusi tapa, orbit kamera kontrollit käyttää tätä
    {

        //huom! viittaa Serializable -luokkiin sen nimellä, esim. position.targetPosOffset;
        targetPos = target.position + targetPosOffset; //tämä on "pivotpoint"
        Vector3 toPos = targetPos + (target.rotation * offsetFromTarget); //itse lisätty rivi vanhasta, ota huomioon target rotaatio
        //laske destination
        destination = Quaternion.Euler(xRotation, yRotation, 0) * -Vector3.back * distanceFromTarget;
       // destination += targetPos;
        destination += toPos;
        Vector3 smoothy = Vector3.SmoothDamp(transform.position, destination, ref velocity, distanceDamp);
        //mene destinationiin
        transform.position = smoothy;
    }

    void FollowPos() //vanhasta projektista
    {
        Vector3 toPos = target.position + (target.rotation * offsetFromTarget);
        //laske destination, eli targetin paikka
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref velocity, distanceDamp);
        //mene destinationiin
        transform.position = curPos;

    }
    


    void LookAtTarget()
    {
        float eulerYAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, lookSmooth);
        float eulerXAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, target.eulerAngles.x, ref rotateVel, lookSmooth);
        transform.rotation = Quaternion.Euler(eulerXAngle, eulerYAngle, 0);
        //transform.rotation = Quaternion.Euler(transform.eulerAngles.x, eulerYAngle, 0); = ilman x rotaatiota
    }

   void LookAtTarget2() //camera is looking position of the target with the offset! no pivotpoint needed GOOD METHOD
    {
        targetPos = target.position + targetPosOffset; //välissä pivotpoint
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position, target.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookSmooth * Time.deltaTime);
    }
    void LookAtTarget3()//old rotation
    {
        Quaternion toRot = Quaternion.LookRotation(target.position - transform.position, target.up);
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, lookSmooth * Time.deltaTime);
        transform.rotation = curRot;
    }

 
}
