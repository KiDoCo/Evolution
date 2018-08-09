using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

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

        InvokeRepeating("TransmitRotation", 0f, 1 / sendRate);
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, syncRot, Time.deltaTime * lerpRate);
        }
    }

    //Send info
    [Command]
    void CmdProvideRotationToServer(Quaternion rot)
    {
        syncRot = rot;
    }

    [ClientCallback]
    void TransmitRotation()
    {
        if (isLocalPlayer && Quaternion.Angle(transform.rotation, lastRot) > threshold)
        {
            CmdProvideRotationToServer(transform.rotation);
            lastRot = transform.rotation;
        }
    }

    [Client]
    void SyncRotationValues(Quaternion latestRot)
    {
        syncRot = latestRot;
        syncRotList.Add(syncRot);
    }
}