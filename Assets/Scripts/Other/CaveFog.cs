using UnityEngine;

public class CaveFog : MonoBehaviour
{
    public bool useCaveFog = false; //  is player in cave
    private MeshRenderer meshRenderer;

    void Start ()
    {
        meshRenderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;

        if(meshRenderer)
        {
            meshRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            useCaveFog = true;
            other.transform.GetComponent<NetworkPlayerCaveFog>().changeFog(useCaveFog);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            useCaveFog = false;
            other.transform.GetComponent<NetworkPlayerCaveFog>().changeFog(useCaveFog);
        }
    }
}