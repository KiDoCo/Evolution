﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PathManager : NetworkBehaviour
{
#pragma warning disable
    public static PathManager Instance;

    public LayerMask obstacleLayer;
    private List<Node> allNodes = new List<Node>();
    private Node defNode, bestNode;
    private int fishAmount;
    private float timer;
    private const float spawnInterval = 4.0f;
    public float speed;
    public float escapeSpeed;
    public float visionRange;
    public float maxVisionAngle;
    public float escapeTimer;
    private bool loaded;
    public GameObject enemy;
    public GameObject fishPrefab;
    private GameObject locationContainer;


#pragma warning restore

    /// <summary>
    /// Assigns nodes to the locationContainer child positions
    /// </summary>
    [ServerCallback]
    public void NodeAssign()
    {
        locationContainer = GameObject.Find("LocationContainer");
        Transform[] array = locationContainer.GetComponentsInChildren<Transform>();

        foreach (Transform a in array)
        {
            if (a.transform.parent != null)
            {
                a.SetParent(gameObject.transform);
            }
        }
        defNode = new Node(new Vector3(), this);

        //Take all child objects -> create nodes with child position -> delete children
        for (int i = 0; i < transform.childCount; i++)
        {
            allNodes.Add(new Node(transform.GetChild(i).position, this));
            Destroy(transform.GetChild(i).gameObject);
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

    /// <summary>
    /// Instantiates a fish to random node
    /// </summary>
    [ServerCallback]
    public void SpawnFish() 
    {
        if (fishAmount >= 1 || timer >= 0) return;
        GameObject clone = Instantiate(fishPrefab, allNodes[Random.Range(0, allNodes.Count)].position, Quaternion.identity);
        NetworkServer.Spawn(clone);
        fishAmount++;
        timer = spawnInterval;
    }

    public void DecreaseFishAmount()
    {
        fishAmount--;
    }

    public Node GetNextNode(Vector3 startPos, Node prevNode) //Random neighbour of node
    {
        return prevNode.neighbors[Random.Range(0, prevNode.neighbors.Count)];
    }

    /// <summary>
    /// Finds the closest node from current node when fish has reached it
    /// </summary>
    /// <param name="startPos"></param>
    /// <returns></returns>
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

    private void Update()
    {
        timer -= Time.deltaTime;
    }

    private void Awake()
    {
        Instance = this;

        EventManager.ActionAddHandler(EVENT.AINodeSpawn, NodeAssign);
        EventManager.ActionAddHandler(EVENT.Increase, SpawnFish);
    }

    private void OnDestroy()
    {
        EventManager.ActionDeleteHandler(EVENT.AINodeSpawn);
        EventManager.ActionDeleteHandler(EVENT.Increase);
    }
}
