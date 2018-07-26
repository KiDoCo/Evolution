using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionChecker : MonoBehaviour
{
    public Transform target;

    public float targetHeight = 1.7f;
    public float distance = 5.0f;

    public float maxDistance = 20;
    public float minDistance = .6f;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    public int yMinLimit = -80;
    public int yMaxLimit = 80;

    public int zoomRate = 40;

    public float rotationDampening = 3.0f;
    public float zoomDampening = 5.0f;

    private float x = 0.0f;
    private float y = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private float correctedDistance;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.x;
        y = angles.y;

        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;

    }

    /**
     * Camera logic on LateUpdate to only update after all character movement logic has been handled.
     */
    void LateUpdate()
    {
        // Don't do anything if target is not defined
        if (!target)
            return;

        // If either mouse buttons are down, let the mouse govern camera position
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        // otherwise, ease behind the target if any of the directional keys are pressed
        else if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            float targetRotationAngle = target.eulerAngles.y;
            float currentRotationAngle = transform.eulerAngles.y;
            x = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // set camera rotation
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        // calculate the desired distance
        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        correctedDistance = desiredDistance;

        // calculate desired camera position
        Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + new Vector3(0, -targetHeight, 0));

        // check for collision using the true target's desired registration point as set by user using height
        RaycastHit collisionHit;
        Vector3 trueTargetPosition = new Vector3(target.position.x, target.position.y + targetHeight, target.position.z);

        // if there was a collision, correct the camera position and calculate the corrected distance
        bool isCorrected = false;
        if (Physics.Linecast(trueTargetPosition, position, out collisionHit))
        {
            position = collisionHit.point;
            correctedDistance = Vector3.Distance(trueTargetPosition, position);
            isCorrected = true;
        }

        // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
        currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening) : correctedDistance;

        // recalculate position based on the new currentDistance
        position = target.position - (rotation * Vector3.forward * currentDistance + new Vector3(0, -targetHeight, 0));

        transform.rotation = rotation;
        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }


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
