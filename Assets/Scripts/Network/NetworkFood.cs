using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkFood : NetworkBehaviour {

    [SerializeField] private float decreaseSpeed = 0.01f;
    [SerializeField] private float minSize = 0.01f;

    private bool eating = false;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CmdEatState(true);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            CmdEatState(false);
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

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (eating)
            StartCoroutine("DecreaseSize");
        else
            StopCoroutine("DecreaseSize");
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        StopCoroutine("DecreaseSize");
    }

    /// <summary> Changes size in other clients (host -> client) </summary>
    [ClientRpc]
    private void RpcChangeSize(float size)
    {
        transform.localScale = new Vector3(size, size, size);
    }

    [Command]
    private void CmdEatState(bool eatState)
    {
        eating = eatState;
    }
}
