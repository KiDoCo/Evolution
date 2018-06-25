using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour {

    public LayerMask obstacleLayer;
    private List<Node> allNodes = new List<Node>();
    Node defNode, bestNode;

    public float speed;
    public float escapeSpeed;
    public float visionRange;
    public float maxVisionAngle;
    public float escapeTimer;
    public GameObject enemy;
    public GameObject fishPrefab;


    void Awake() 
    {
        defNode = new Node(new Vector3(), this);

        //Take all child objects -> create nodes with child position -> delete children
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            allNodes.Add(new Node(gameObject.transform.GetChild(i).position, this));
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }

        //Check neighbours for each node using Physics.Linecast
        foreach (Node n in allNodes)
        {
            foreach (Node nn in allNodes)
            {
                if (n != nn && !Physics.Linecast(n.position, nn.position, obstacleLayer))
                {
                    Debug.DrawLine(n.position, nn.position, Color.red, 100);
                    n.neighbors.Add(nn);
                }

            }
        }
    }

    void SpawnFish() //instantiate a fish in random node position
    {
        Instantiate(fishPrefab, allNodes[Random.Range(0, allNodes.Count)].position, Quaternion.identity);
    }


    public Node GetClosestNode(Vector3 startPos) //Find closest node for fish using Physics.Linecast (used when fish returns to path)
    {
        float tempDist = 100;
        foreach (Node n in allNodes)
        {
            if (!Physics.Linecast(startPos, n.position, obstacleLayer) && Vector3.Distance(startPos, n.position) < tempDist)
            {
                bestNode = n;
                tempDist = Vector3.Distance(startPos, n.position);
            }
        }
        return bestNode;
    }

    public Node GetNextNode(Vector3 startPos, Node prevNode) //Random neighbour of node
    {
        return prevNode.neighbors[Random.Range(0, prevNode.neighbors.Count)];
    }
}
