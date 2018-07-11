using UnityEngine;
using UnityEngine.Networking;

public class NetworkSmoothTransform : NetworkTransform {

    public override void OnStartServer()
    {
        clientMoveCallback3D = SmoothTransform;
    }

    public bool SmoothTransform(ref Vector3 position, ref Vector3 velocity, ref Quaternion rotation)
    {
        Debug.Log(position + "/t" + velocity + "/t" + rotation);
        return true;
    }

}
