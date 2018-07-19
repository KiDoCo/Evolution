using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionChecker : MonoBehaviour
{
    //Cast ray from camera to player to check if anything in between or if camera colliding
    //Find near clip plane points of camera (left-up, right-up, left-down, right-down, camera-position)
    //Move camera closer if something found
    //Move camera to limit shearing

    //Fade character if camera too close

    public LayerMask collisionLayer;

    [HideInInspector] public bool Colliding = false;
    [HideInInspector] public Vector3[] AdjustedcameraClipPoints;
    [HideInInspector] public Vector3[] DesiredCameraClipPoints;

    Camera camera;

    public void Initialize(Camera cam)
    {
        camera = cam;
        AdjustedcameraClipPoints = new Vector3[5];
        DesiredCameraClipPoints = new Vector3[5];
    }

    public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
    {
        if (!camera)
            return;

        //clear the contents of intoArray
        intoArray = new Vector3[5];

        float z = camera.nearClipPlane;
        float x = Mathf.Tan(camera.fieldOfView / 3.41f) * z;
        float y = x / camera.aspect;

        //top left
        intoArray[0] = (atRotation * new Vector3(-x, y, z) + cameraPosition); //added and rotated the point relative to camera
        //top right
        intoArray[1] = (atRotation * new Vector3(x, y, z) + cameraPosition); //added and rotated the point relative to camera
        //bottom left
        intoArray[2] = (atRotation * new Vector3(-x, -y, z) + cameraPosition); //added and rotated the point relative to camera
        //bottom right
        intoArray[3] = (atRotation * new Vector3(x, -y, z) + cameraPosition); //added and rotated the point relative to camera
        //camera position
        intoArray[4] = cameraPosition - camera.transform.forward;

    }

    bool CollisionDetectedAtClipPoint(Vector3[] clipPoints, Vector3 fromPosition)
    {
        for(int i = 0; i < clipPoints.Length; i++)
        {
            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
            float distance = Vector3.Distance(clipPoints[i], fromPosition);
            if(Physics.Raycast(ray, distance, collisionLayer))
            {
                return true;
            }
        }

        return false;
    }

    public float GetAdjustedDistance(Vector3 from)
    {
        float distance = -1;

        for(int i=0; i < DesiredCameraClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, DesiredCameraClipPoints[i] - from);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
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
        if(CollisionDetectedAtClipPoint(DesiredCameraClipPoints, targetPosition))
        {
           Colliding = true;
        }
        else
        {
           Colliding = false;
        }
    }
}
