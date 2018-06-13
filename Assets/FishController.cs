using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : MonoBehaviour {

    public PathManager pathManager;
    private Node node;

    public LayerMask threatMask;
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
        RaycastHit hit;
        switch (curState)
        {
            case States.roam:
                if (Vector3.Distance(gameObject.transform.position, node.position) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, node.position, pathManager.speed * Time.deltaTime);
                    Physics.BoxCast(mCollider.bounds.center, boxScale * 2, transform.forward, out hit, transform.rotation, 1, threatMask);
                    if (hit.transform)
                    {
                        CheckVisible(hit.transform);
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
                Physics.BoxCast(mCollider.bounds.center, boxScale * 1.5f, transform.forward, out hit, transform.rotation, 1, obstacleMask);
                if (hit.transform)
                {
                    transform.Rotate(Vector3.up*200*Time.deltaTime);
                }
                /*
                if (Physics.SphereCast(transform.position, 1.5f, transform.forward,out hit, 1, obstacleMask))
                {
                    transform.Rotate(Vector3.up*200*Time.deltaTime);
                }*/
                Debug.DrawRay(transform.position, transform.forward*1.5f, Color.green);
                break;
        }
    }

    private void CheckVisible(Transform threat)
    {
        if (!Physics.Linecast(transform.position, threat.position, obstacleMask))
        {
            transform.LookAt(threat.position);
            transform.Rotate(Vector3.up * 180);
            curState = States.chased;
            Invoke("SetRoam", 10);
        }
    }

    private void SetRoam()
    {
        node = pathManager.GetClosestNode(transform.position);
        curState = States.roam;
    }
}
