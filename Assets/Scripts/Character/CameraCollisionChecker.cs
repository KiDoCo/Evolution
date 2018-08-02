using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionChecker : MonoBehaviour
{
    //[Header("Camera Properties")]
    //public float DistanceAway;                     //how far the camera is from the player.
    //public float DistanceUp;                    //how high the camera is above the player
    //public float smooth = 4.0f;                    //how smooth the camera moves into place
    //public float rotateAround = 70f;            //the angle at which you will rotate the camera (on an axis)
    //[Header("Player to follow")]
    //public Transform target;                    //the target the camera follows
    //[Header("Layer(s) to include")]
    //public LayerMask CamOcclusion;                //the layers that will be affected by collision
    //RaycastHit hit;
    //float cameraHeight = 55f;
    //float cameraPan = 0f;
    //float camRotateSpeed = 180f;
    //Vector3 camPosition;
    //Vector3 camMask;
    //Vector3 followMask;
    //// Use this for initialization
    //void Start()
    //{
    //    //the statement below automatically positions the camera behind the target.
    //    rotateAround = target.eulerAngles.y - 45f;
    //}
    //void Update()
    //{

    //}
    //// Update is called once per frame

    //void LateUpdate()
    //{
    //    //Offset of the targets transform (Since the pivot point is usually at the feet).
    //    Vector3 targetOffset = new Vector3(target.position.x, (target.position.y + 2f), target.position.z);
    //    Quaternion rotation = Quaternion.Euler(cameraHeight, rotateAround, cameraPan);
    //    Vector3 vectorMask = Vector3.one;
    //    Vector3 rotateVector = rotation * vectorMask;
    //    //this determines where both the camera and it's mask will be.
    //    //the camMask is for forcing the camera to push away from walls.
    //    camPosition = targetOffset + Vector3.up * DistanceUp - rotateVector * DistanceAway;
    //    camMask = targetOffset + Vector3.up * DistanceUp - rotateVector * DistanceAway;

    //    occludeRay(ref targetOffset);
    //    smoothCamMethod();

    //    transform.LookAt(target);

    //    #region wrap the cam orbit rotation
    //    if (rotateAround > 360)
    //    {
    //        rotateAround = 0f;
    //    }
    //    else if (rotateAround < 0f)
    //    {
    //        rotateAround = (rotateAround + 360f);
    //    }
    //    #endregion

    //    rotateAround += Input.GetAxisRaw("Horizontal") * camRotateSpeed * Time.deltaTime;
    //    DistanceUp = Mathf.Clamp(DistanceUp += Input.GetAxisRaw("Vertical"), -0.79f, 2.3f);
    //}
    //void smoothCamMethod()
    //{
    //    smooth = 4f;
    //    transform.position = Vector3.Lerp(transform.position, camPosition, Time.deltaTime * smooth);
    //}
    //void occludeRay(ref Vector3 targetFollow)
    //{
    //    #region prevent wall clipping
    //    //declare a new raycast hit.
    //    RaycastHit wallHit = new RaycastHit();
    //    //linecast from your player (targetFollow) to your cameras mask (camMask) to find collisions.
    //    if (Physics.Linecast(targetFollow, camMask, out wallHit, CamOcclusion))
    //    {
    //        //the smooth is increased so you detect geometry collisions faster.
    //        smooth = 10f;
    //        //the x and z coordinates are pushed away from the wall by hit.normal.
    //        //the y coordinate stays the same.
    //        camPosition = new Vector3(wallHit.point.x + wallHit.normal.x * 0.5f, camPosition.y, wallHit.point.z + wallHit.normal.z * 0.5f);
    //    }
    //    #endregion
    //}


    //private void Update()
    //{
    //    Vector3 desiredCameraPoint = target
    //}

    //Cast ray from camera to player to check if anything in between or if camera colliding
    //Find near clip plane points of camera(left-up, right-up, left-down, right-down, camera-position)
    //Move camera closer if something found
    //Move camera to limit shearing

    //Fade character if camera too close

    [Header("Collision varibles")]
    public float MinDistance = 0.5f;
    public float MaxDistance = 2.0f;
    public float smoothTime = 5;
    float duration = 2;
    float startTime;
    public LayerMask collisionMask;

    //private void FixPositions()
    //{
    //    //TODO: get rid of shaking during lerping camera

    //    Vector3 desiredPos = new Vector3(resetPos.x, resetPos.y, resetPos.z);
    //    float actualZ = desiredPos.z;
    //    float actualX = desiredPos.x;
    //    float actualY = desiredPos.y;

    //    CameraCollision(ref actualZ, ref actualX, ref actualY);

    //    Vector3 correctedPos = new Vector3(actualX, actualY, actualZ);
    //    Vector3 targetP = correctedPos;
    //    targetP.z = Mathf.Lerp(targetP.z, actualZ, smoothTime * Time.fixedDeltaTime);
    //    cameraOffset = targetP;
    //}

    //void CameraCollision(ref float actualZ, ref float actualX, ref float actualY)
    //{
    //    Debug.DrawLine(target.position, transform.position, Color.blue);

    //    float step = Vector3.Distance(target.position, transform.position);
    //    int stepCount = 3;
    //    float stepIncremental = step / stepCount;
    //    RaycastHit hit;
    //    Vector3 dir = transform.forward;


    //    if (Physics.Raycast(target.position, transform.position, out hit, step, collisionMask))
    //    {
    //        actualZ = -Mathf.Clamp((hit.distance * 0.9f), MinDistance, MaxDistance);
    //        //float distance = Vector3.Distance(hit.point, target.position);
    //        //actualZ = -(distance * 0.5f);

    //    }
    //    else
    //    {
    //        for (int s = 1; s < stepCount; s++)
    //        {
    //            Vector3 secondOrigin = target.position - (dir * s) * stepIncremental;

    //            for (int i = 0; i < 4; i++)
    //            {
    //                Vector3 direction = Vector3.zero;
    //                switch (i)
    //                {
    //                    case 0:
    //                        direction = transform.right;
    //                        break;
    //                    case 1:
    //                        direction = -transform.right;
    //                        break;
    //                    case 2:
    //                        direction = transform.up;
    //                        break;
    //                    case 3:
    //                        direction = -transform.up;
    //                        break;
    //                }

    //                Debug.DrawRay(secondOrigin, direction, Color.red);

    //                if (Physics.Raycast(secondOrigin, direction, out hit, 0.2f, collisionMask))
    //                {
    //                    float distance = Vector3.Distance(secondOrigin, target.position);
    //                    actualZ = -(distance * 0.5f);
    //                }
    //            }
    //        }
    //    }

    //}


    Camera camera3rd;
    public LayerMask collisionLayer;

    [HideInInspector] public bool Colliding = false;
    [HideInInspector] public Vector3[] AdjustedcameraClipPoints;
    [HideInInspector] public Vector3[] DesiredCameraClipPoints;


    public void Initialize(Camera cam)
    {
        camera3rd = cam;
        AdjustedcameraClipPoints = new Vector3[5];
        DesiredCameraClipPoints = new Vector3[5];
    }

    public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
    {
        if (!camera3rd)
            return;

        //clear the contents of intoArray
        intoArray = new Vector3[5];

        float z = camera3rd.nearClipPlane;
        float x = Mathf.Tan(camera3rd.fieldOfView / 3.41f) * z;
        float y = x / camera3rd.aspect;

        //top left
        intoArray[0] = (atRotation * new Vector3(-x, y, z) + cameraPosition); //added and rotated the point relative to camera
                                                                              //top right
        intoArray[1] = (atRotation * new Vector3(x, y, z) + cameraPosition); //added and rotated the point relative to camera
                                                                             //bottom left
        intoArray[2] = (atRotation * new Vector3(-x, -y, z) + cameraPosition); //added and rotated the point relative to camera
                                                                               //bottom right
        intoArray[3] = (atRotation * new Vector3(x, -y, z) + cameraPosition); //added and rotated the point relative to camera
                                                                              //camera position
        intoArray[4] = cameraPosition - camera3rd.transform.forward;

    }

    bool CollisionDetectedAtClipPoint(Vector3[] clipPoints, Vector3 fromPosition)
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

    public float GetAdjustedDistance(Vector3 from)
    {
        float distance = -1;

        for (int i = 0; i < DesiredCameraClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, DesiredCameraClipPoints[i] - from);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (distance == -1)
                    distance = hit.distance;
                else
                {
                    if (hit.distance < distance)
                        distance = hit.distance;
                }
            }
        }

        if (distance == -1)
        {
            return 0;
        }
        else
            return distance;
    }

    public void CheckColliding(Vector3 targetPosition)
    {
        if (CollisionDetectedAtClipPoint(DesiredCameraClipPoints, targetPosition))
        {
            Colliding = true;
        }
        else
        {
            Colliding = false;
        }
    }
}
