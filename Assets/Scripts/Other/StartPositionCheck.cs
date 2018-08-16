using UnityEngine;

public class StartPositionCheck : MonoBehaviour
{
    public bool isEmpty;
    public float updateSpeed = 1f;
    public float triggerBoxSize = 1f;

    private void Awake()
    {
        Checking();
        InvokeRepeating("Checking", 0.1f, updateSpeed);
    }
    
    private void Checking()
    {
        Collider[] tempC = null;

        tempC = Physics.OverlapBox(transform.position, new Vector3(triggerBoxSize, triggerBoxSize, triggerBoxSize), Quaternion.identity, ~(LayerMask.NameToLayer("Herbivore") | LayerMask.NameToLayer("Carnivore")));

        if (tempC.Length != 0)
        {
            isEmpty = false;
        }
        else
        {
            isEmpty = true;
        }
    }

    private void OnDrawGizmos() //  Debugging
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawWireCube(transform.position, new Vector3(triggerBoxSize * 2, triggerBoxSize * 2, triggerBoxSize * 2));
    }
}