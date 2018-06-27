using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
#pragma warning disable
    private PathManager pathManager;

    public Vector3 position; //use this instead of transform
    public List<Node> neighbors = new List<Node>(); //visible nodes
    public float gCost, hCost, fCost; //not used ATM
#pragma warning restore

    public Node(Vector3 _pos, PathManager _path)
    {
        position = _pos;
        pathManager = _path;
    }

    public void CalculateCosts(Vector3 startPoint, Vector3 goalPoint)
    {
        gCost = Vector3.Distance(position, startPoint);
        hCost = Vector3.Distance(position, goalPoint);
        fCost = gCost + hCost;
    }


}
