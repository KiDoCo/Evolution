using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour {

    public PathManager pathManager;
    private Node node;

    public LayerMask threatMask; //remove this
    private LayerMask obstacleMask;

    private enum States {roam, chased};
    States curState = States.roam;

    Collider mCollider;
    Vector3 boxScale;

    void Start()
    {
        node = pathManager.GetClosestNode(transform.position);
        mCollider = gameObject.GetComponent<Collider>();
        boxScale = transform.localScale;
        boxScale.z *= 2;
        obstacleMask = pathManager.obstacleLayer;
    }

    void Update()
    {
        switch (curState)
        {
            case States.roam:
                if (Vector3.Distance(gameObject.transform.position, node.position) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, node.position, pathManager.speed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, pathManager.enemy.transform.position) < pathManager.visionRange)
                    {
                        CheckVisible();
                    }
                }
                else
                {
                    node = pathManager.GetNextNode(transform.position, node);
                    transform.LookAt(node.position);
                }
                break;
            case States.chased:
                transform.Translate(Vector3.forward * pathManager.escapeSpeed * Time.deltaTime);
                CheckObstacles();
                /*
                if (Physics.SphereCast(transform.position, 1.5f, transform.forward,out hit, 1, obstacleMask))
                {
                    transform.Rotate(Vector3.up*200*Time.deltaTime);
                }*/
                Debug.DrawRay(transform.position, transform.forward*1.5f, Color.green);
                break;
        }
    }

    private void CheckVisible()
    {
        Debug.Log(Vector3.Angle(transform.right, pathManager.enemy.transform.position));
        if (!Physics.Linecast(transform.position, pathManager.enemy.transform.position, obstacleMask) && 
            Vector3.Angle(pathManager.enemy.transform.position - transform.position, transform.forward) < 45)
        {
            transform.LookAt(pathManager.enemy.transform.position);
            transform.Rotate(Vector3.up * 180);
            curState = States.chased;
            Invoke("SetRoam", 10);
        }
    }

    private void CheckObstacles()
    {
        if (Physics.Raycast(transform.position + transform.right * 0.5f, transform.forward, 1, obstacleMask))
        {
            transform.Rotate(-Vector3.up * 200 * Time.deltaTime);
        }
        else if (Physics.Raycast(transform.position - transform.right * 0.5f, transform.forward, 1, obstacleMask))
        {
            transform.Rotate(Vector3.up * 200 * Time.deltaTime);
        }
        /*
        if (transform.forward.z - h.point.z > 0) //!!!!!!!!
        {
            transform.Rotate(Vector3.up * 200 * Time.deltaTime);
        }
        else
        {
            transform.Rotate(-Vector3.up * 200 * Time.deltaTime);
        }*/
    }

    private void SetRoam()
    {
        node = pathManager.GetClosestNode(transform.position);
        curState = States.roam;
    }
}
