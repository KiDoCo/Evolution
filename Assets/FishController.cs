using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour {

    public PathManager pathManager;
    private Node node;
    private LayerMask obstacleMask;

    private enum States {roam, chased};
    States curState = States.roam;

    Collider mCollider;
    Vector3 boxScale;

    void Start()
    {
        pathManager = GameObject.Find("FishPathManager").GetComponent<PathManager>();
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
                Debug.DrawRay(transform.position, transform.forward*1.5f, Color.green);
                break;
        }
    }

    private void CheckVisible()
    {
        if (!Physics.Linecast(transform.position, pathManager.enemy.transform.position, obstacleMask) && 
            Vector3.Angle(pathManager.enemy.transform.position - transform.position, transform.forward) < pathManager.maxVisionAngle)
        {
            transform.LookAt(pathManager.enemy.transform.position);
            transform.Rotate(Vector3.up * 180);
            curState = States.chased;
            Invoke("SetRoam", pathManager.escapeTimer);
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
    }

    private void SetRoam()
    {
        if (Vector3.Distance(transform.position, pathManager.enemy.transform.position) < pathManager.visionRange)
        {
            Invoke("SetRoam", pathManager.escapeTimer);
            return;
        }
        node = pathManager.GetClosestNode(transform.position);
        curState = States.roam;
    }
}
