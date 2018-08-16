using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class NetworkLerpRotation : NetworkBehaviour
{
    [SyncVar(hook = "SyncRotationValues")] private Quaternion syncRot;

    public float lerpRate = 4;            //alkp 15
    public float normalLerpRate = 8;      //alkp 16
    public float fasterLerpRate = 16;     //alkp 27

    [SerializeField] float sendRate = 9f;

    private Quaternion lastRot;
    public float threshold = 0.1f;         //alkp 0.5f

    //Latency stuff
    private List<Quaternion> syncRotList = new List<Quaternion>();
    public float closeEnough = 0.05f;      //alkp 0.11f

    void Start()
    {
        lerpRate = normalLerpRate;

        InvokeRepeating("TransmitRotation", 0f, 1 / sendRate);  //  1
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, syncRot, Time.deltaTime * lerpRate);
        }
    }

    [ClientCallback]
    void TransmitRotation()  //  2
    {
        if (isLocalPlayer && Quaternion.Angle(transform.rotation, lastRot) > threshold)
        {
            CmdProvideRotationToServer(transform.rotation);
            lastRot = transform.rotation;
        }
    }

    //Send info
    [Command]
    void CmdProvideRotationToServer(Quaternion rot)  //  3
    {
        syncRot = rot;
    }

    [Client]
    void SyncRotationValues(Quaternion latestRot)  //  4
    {
        syncRot = latestRot;
        syncRotList.Add(syncRot);
    }
}