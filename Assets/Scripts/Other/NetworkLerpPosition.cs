using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class NetworkLerpPosition : NetworkBehaviour
{
    [SyncVar(hook = "SyncPositionValues")] private Vector3 syncPos;

    public float lerpRate = 4;            //alkp 15
    public float normalLerpRate = 8;      //alkp 16
    public float fasterLerpRate = 16;     //alkp 27

    [SerializeField] float sendRate = 9f;

    private Vector3 lastPos;
    public float threshold = 0.1f;         //alkp 0.5f

    //Latency stuff
    private List<Vector3> syncPosList = new List<Vector3>();
    public float closeEnough = 0.05f;      //alkp 0.11f

    void Start()
    {
        lerpRate = normalLerpRate;

        InvokeRepeating("TransmitPosition", 0f, 1 / sendRate);  //  1
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * lerpRate);
        }
    }

    [ClientCallback]
    void TransmitPosition()  //  2
    {
        if (isLocalPlayer && Vector3.Distance(transform.position, lastPos) > threshold)
        {
            CmdProvidePositionToServer(transform.position);
            lastPos = transform.position;
        }
    }

    //Send info
    [Command]
    void CmdProvidePositionToServer(Vector3 pos)  //  3
    {
        syncPos = pos;
    }

    [Client]
    void SyncPositionValues(Vector3 latestPos)  //  4
    {
        syncPos = latestPos;
        syncPosList.Add(syncPos);
    }
}