using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{

    //objects
    public Transform target;
    public Transform pivotpoint;
    public Camera Camera3rd;
    Transform camTrans;
    LayerMask ground;

    //bools
    public bool FreeCamera;

    //values
    Vector3 velocity = Vector3.one;
    [SerializeField] Vector3 cameraPos = new Vector3(0f, 0.2f, -2f);
    private float distanceDamp = 10f;
    [SerializeField] protected float distanceDampValue = 0.4f;
    [SerializeField] protected float reverseDistanceDamp = 0.02f; //used in reverse movement
    [SerializeField] protected float verticalDistanceDamp = 0.02f; //used in vertical movement
    private float rotationalDamp = 10f;
    [SerializeField] protected float rotationalDampValue = 5f;
    [SerializeField] protected float rotationSpeed = 2f; //camera rotation speed
    [SerializeField] protected float resetLerpValue = 5; // camera reset speed

    //FOV
    public float FOVValue = 60f;
    [HideInInspector] public float m_FieldOfView = 60f;

    public float DefZ;
    private float curZ;
    public float ZSpeed = 19;

    //reset point
    Vector3 resetPos = Vector3.zero;

    //Followrot(); values
    Vector3 targetPos = Vector3.zero;
    [SerializeField] protected float lookSmooth = 100f;
    [SerializeField] protected Vector3 pivotPos = new Vector3(0, 0, 0);

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
        camTrans = this.transform;
        curZ = DefZ;
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
            distanceDamp = reverseDistanceDamp;
        }
        else if (target.GetComponent<Herbivore>().isMovingVertical)
        {
            distanceDamp = verticalDistanceDamp;
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
        targetPos = target.position + pivotPos; //välissä pivotpoint
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position, target.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookSmooth * Time.deltaTime);
    }

    void Stabilize()
    {
        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
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

            Quaternion camTurnAngleH = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, Vector3.up); //laske horisontaalinen liike
            Quaternion camTurnAngleV = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeed, Vector3.right);// laske vertikaalinen liike

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
    void ResetCamera()
    {
        if (!FreeCamera)
        {
            Vector3 resetLoc = Vector3.Lerp(cameraPos, resetPos, resetLerpValue * Time.deltaTime);
            cameraPos = resetLoc;
        }
    }

    private void HandlePivotPosition()
    {
        float targetZ = DefZ;

        CameraCollision(DefZ, ref targetZ);

        curZ = Mathf.Lerp(curZ, targetZ, Time.deltaTime * ZSpeed);
        Vector3 temp = Vector3.zero;
        temp.z = curZ;
        camTrans.localPosition = temp;
    }

    private void CameraCollision(float targetZ, ref float actualZ)
    {
        float step = Mathf.Abs(targetZ);
        int stepCount = 2;
        float stepIncrement = step / stepCount;

        RaycastHit hit;
        Vector3 origin = pivotpoint.position;
        Vector3 direction = -pivotpoint.position;

        if (Physics.Raycast(origin, direction, out hit, step, ground))
        {
            float distance = Vector3.Distance(hit.point, origin);
            actualZ = -(distance / 2);
        }
        else
        {
            for(int s = 0; s< stepCount; s++)
            {
                for(int i =0; i<4; i++)
                {
                    Vector3 dir = Vector3.zero;
                    Vector3 secondOrigin = origin + (direction * s) * stepIncrement;

                    switch(i)
                    {
                        case 0:
                            dir = camTrans.right;
                            break;
                        case 1:
                            dir = -camTrans.right;
                            break;
                        case 2:
                            dir = camTrans.up;
                            break;
                        case 3:
                            dir = -camTrans.up;
                            break;
                    }

                    if(Physics.Raycast(secondOrigin, dir, out hit, 0.5f, ground))
                    {
                        float distance = Vector3.Distance(secondOrigin, origin);
                        actualZ = -(distance / 2);
                        if (actualZ < 0.2f)
                            actualZ = 0;
                    }
                }
            }
        }
    }



}
