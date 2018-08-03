using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FishController : NetworkBehaviour
{
    public Node node;
    private LayerMask obstacleMask;
    private Vector3 lookDir;
    private Vector3 reference;
    private enum States { roam, chased };
    States curState = States.roam;



    private void CheckVisible() //Check monster visiblity with Physics.Linecast and the angle with Vector3.Angle -> chased if visible
    {
        if (!Physics.Linecast(transform.position, PathManager.Instance.enemy.transform.position, obstacleMask) &&
            Vector3.Angle(PathManager.Instance.enemy.transform.position - transform.position, transform.forward) < PathManager.Instance.maxVisionAngle)
        {
            transform.LookAt(PathManager.Instance.enemy.transform.position);
            transform.Rotate(Vector3.up * 180);
            curState = States.chased;
            Invoke("SetRoam", PathManager.Instance.escapeTimer);
        }
    }

    [ClientRpc]
    private void RpcMovement(Vector3 pos)
    {
        transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * PathManager.Instance.speed);
    }

    [ClientRpc]
    private void RpcLook(Vector3 pos)
    {
        transform.LookAt(pos);
    }

    private void CheckObstacles() //Rotate away from obstacles
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

    private void SetRoam() //Check if monster is within given distance and return to path if it isn't
    {
        if (Vector3.Distance(transform.position, PathManager.Instance.enemy.transform.position) < PathManager.Instance.visionRange)
        {
            Invoke("SetRoam", PathManager.Instance.escapeTimer);
            return;
        }
        node = PathManager.Instance.GetClosestNode(transform.position);
        curState = States.roam;
    }



    void Start()
    {
        node = PathManager.Instance.GetClosestNode(transform.position);
        obstacleMask = PathManager.Instance.obstacleLayer;

    }

    void Update()
    {

        if (isServer)
        {
            if (curState == States.roam)
            {
                //Wander randomly between nodes
                if (Vector3.Distance(gameObject.transform.position, node.position) > 0)
                {
                    RpcMovement(node.position);

                    if (Vector3.Distance(transform.position, PathManager.Instance.enemy.transform.position) < PathManager.Instance.visionRange)
                    {
                        CheckVisible();
                    }
                }
                else
                {
                    node = PathManager.Instance.GetNextNode(transform.position, node);
                    lookDir = node.position;
                    RpcLook(lookDir);
                }
            }
            else if (curState == States.chased)
            {
                //Move forward for a given time and avoid obstacles
                RpcMovement(-PathManager.Instance.enemy.transform.position);
                CheckObstacles();
                Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.green);

            }
        }
    }
}
