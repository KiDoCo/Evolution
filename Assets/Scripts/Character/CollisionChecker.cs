using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    private Rigidbody rBod;
    private Vector3 moveDirection;
    private Vector3 surfaceNormal;
    private Vector3 colDirection;
    private CapsuleCollider col;
    private bool canMove = true;

    private void Awake()
    {
        rBod = GetComponent<Rigidbody>();
        col = GetComponentInChildren<CapsuleCollider>();
    }


    private void Update()
    {
        float horizontalMovement = 0;
        float verticalMovement = 0;

        if (CanMove(transform.right * Input.GetAxis("Horizontal")))
        {
            horizontalMovement = Input.GetAxisRaw("Horizontal");
        }
        if (CanMove(transform.forward * Input.GetAxis("Vertical")))
        {
            verticalMovement = Input.GetAxisRaw("Vertical");
        }

        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;

        Move(moveDirection);
    }

    //private void FixedUpdate()
    //{
    //    //Vector3 temp = Vector3.Cross(surfaceNormal, moveDirection);
    //    //moveDirection = Vector3.Cross(temp, surfaceNormal);
    //    moveDirection = (moveDirection - (Vector3.Dot(moveDirection, surfaceNormal)) * surfaceNormal).normalized;

    //    Move(moveDirection);

    //            else
    //    {
    //        moveDirection = colPoint + (moveDirection - colPoint) - surfaceNormal* Vector3.Dot(transform.forward - colPoint, surfaceNormal);
    //transform.Translate(moveDirection* Speed * Time.deltaTime);
    //isMoving = true;
    //    }
//}

bool CanMove(Vector3 dir)
    {
        float distanceToPoints = col.height / 2 - col.radius;

        Vector3 point1 = transform.position + col.center + Vector3.up * distanceToPoints;
        Vector3 point2 = transform.position + col.center - Vector3.up * distanceToPoints;

        float radius = col.radius * 0.95f;
        float castDistance = 0.5f;

        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, radius, dir, castDistance);

        foreach (RaycastHit objectHit in hits)
        {

            // Red = from origin to point
            Debug.DrawLine(dir, objectHit.point, Color.red);
            // Blue = normal on hit point.
            Debug.DrawRay(objectHit.point, objectHit.normal, Color.blue);



            if (objectHit.transform.tag == "Ground")
            {
                return false;
            }
        }

        return true;
    }

    private void Move(Vector3 dir)
    {
        transform.Translate(dir * 5 * Time.deltaTime);
    }

}
