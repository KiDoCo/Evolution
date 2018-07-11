using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkFood : NetworkBehaviour {

    [SerializeField] private float decreaseSpeed = 0.01f;
    [SerializeField] private float minSize = 0.01f;

    private float foodSize
    {
        get
        {
            return transform.localScale.x;
        }
        set
        {
            RpcChangeSize(value);
        }
    }

    private IEnumerator DecreaseSize()
    {
        for (float s = foodSize; s >= minSize; s -= decreaseSpeed)
        {
            foodSize = s;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)       // All size changes are done from the host side
        {
            StartCoroutine("DecreaseSize");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isServer)
        {
            StopCoroutine("DecreaseSize");
        }
    }

    /// <summary> Changes size in other clients (host -> client) </summary>
    [ClientRpc]
    private void RpcChangeSize(float size)
    {
        transform.localScale = new Vector3(size, size, size);
    }
}
