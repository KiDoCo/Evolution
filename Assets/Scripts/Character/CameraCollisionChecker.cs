using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionChecker : MonoBehaviour
{
    float castDistance;
    public LayerMask collisionMask;
    RaycastHit[] hits = new RaycastHit[10];
    int hitCount;
    CapsuleCollider col;
    float Height;

    private void Start()
    {
        col = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        Vector3 direction = transform.forward;

        float distanceToPoints = col.height / 2 - col.radius;
        Vector3 point1 = transform.position + col.center + Vector3.up * distanceToPoints;
        Vector3 point2 = transform.position + col.center - Vector3.up * distanceToPoints;
        float radius = col.radius;
        Height = col.height;


        hitCount = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, hits, 1f, collisionMask, QueryTriggerInteraction.Ignore);

        for ()
    }

    //private void Update()
    //{
    //    Vector3 desiredCameraPoint = target
    //}

    //Cast ray from camera to player to check if anything in between or if camera colliding
    //Find near clip plane points of camera (left-up, right-up, left-down, right-down, camera-position)
    //Move camera closer if something found
    //Move camera to limit shearing

    //Fade character if camera too close


    //    Camera camera3rd;
    //    public LayerMask collisionLayer;

    //    [HideInInspector] public bool Colliding = false;
    //    [HideInInspector] public Vector3[] AdjustedcameraClipPoints;
    //    [HideInInspector] public Vector3[] DesiredCameraClipPoints;


    //    public void Initialize(Camera cam)
    //    {
    //        camera3rd = cam;
    //        AdjustedcameraClipPoints = new Vector3[5];
    //        DesiredCameraClipPoints = new Vector3[5];
    //    }

    //    public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
    //    {
    //        if (!camera3rd)
    //            return;

    //        //clear the contents of intoArray
    //        intoArray = new Vector3[5];

    //        float z = camera3rd.nearClipPlane;
    //        float x = Mathf.Tan(camera3rd.fieldOfView / 3.41f) * z;
    //        float y = x / camera3rd.aspect;

    //        //top left
    //        intoArray[0] = (atRotation * new Vector3(-x, y, z) + cameraPosition); //added and rotated the point relative to camera
    //        //top right
    //        intoArray[1] = (atRotation * new Vector3(x, y, z) + cameraPosition); //added and rotated the point relative to camera
    //        //bottom left
    //        intoArray[2] = (atRotation * new Vector3(-x, -y, z) + cameraPosition); //added and rotated the point relative to camera
    //        //bottom right
    //        intoArray[3] = (atRotation * new Vector3(x, -y, z) + cameraPosition); //added and rotated the point relative to camera
    //        //camera position
    //        intoArray[4] = cameraPosition - camera3rd.transform.forward;

    //    }

    //    bool CollisionDetectedAtClipPoint(Vector3[] clipPoints, Vector3 fromPosition)
    //    {
    //        for (int i = 0; i < clipPoints.Length; i++)
    //        {
    //            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
    //            float distance = Vector3.Distance(clipPoints[i], fromPosition);
    //            if (Physics.Raycast(ray, distance, collisionLayer))
    //            {
    //                return true;
    //            }
    //        }

    //        return false;
    //    }

    //    public float GetAdjustedDistance(Vector3 from)
    //    {
    //        float distance = -1;

    //        for (int i = 0; i < DesiredCameraClipPoints.Length; i++)
    //        {
    //            Ray ray = new Ray(from, DesiredCameraClipPoints[i] - from);
    //            RaycastHit hit;
    //            if (Physics.Raycast(ray, out hit))
    //            {
    //                if (distance == -1)
    //                    distance = hit.distance;
    //                else
    //                {
    //                    if (hit.distance < distance)
    //                        distance = hit.distance;
    //                }
    //            }
    //        }

    //        if (distance == -1)
    //        {
    //            return 0;
    //        }
    //        else
    //            return distance;
    //    }

    //    public void CheckColliding(Vector3 targetPosition)
    //    {
    //        if (CollisionDetectedAtClipPoint(DesiredCameraClipPoints, targetPosition))
    //        {
    //            Colliding = true;
    //        }
    //        else
    //        {
    //            Colliding = false;
    //        }
    //    }
}
