using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveFog : MonoBehaviour
{
    public Color defaultColor;
    public float defaultDensity;

    public Color caveColor;
    public float caveDensity;

    public float speed = 0.02f; // how fast is color change
    public bool useCaveFog = false; //  is player in cave

    private Color fColor;
    private float fDensity;
    private MeshRenderer meshRenderer;

    public float FDensity
    {
        get
        {
            return fDensity;
        }
        set
        {
            fDensity = Mathf.Clamp(value, defaultDensity, caveDensity);
        }
    }

    void Start ()
    {
        meshRenderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;

        if(meshRenderer)
        {
            meshRenderer.enabled = false;
        }

        defaultColor = RenderSettings.fogColor;
        defaultDensity = RenderSettings.fogDensity;

        fColor = defaultColor;
        FDensity = defaultDensity;
    }

    private void Update()
    {
        if (useCaveFog)
        {
            if (fColor.r > caveColor.r)
            {
                fColor.r -= 0.1f * Time.deltaTime;
            }
            if (fColor.g > caveColor.g)
            {
                fColor.g -= 0.1f * Time.deltaTime;
            }
            if (fColor.b > caveColor.b)
            {
                fColor.b -= 0.1f * Time.deltaTime;
            }

            FDensity += speed * Time.deltaTime;

            RenderSettings.fogColor = fColor;
            RenderSettings.fogDensity = FDensity;
           
        }
        else
        {
            if (fColor.r < defaultColor.r)
            {
                fColor.r += 0.1f * Time.deltaTime;
            }
            if (fColor.g < defaultColor.g)
            {
                fColor.g += 0.1f * Time.deltaTime;
            }
            if (fColor.b < defaultColor.b)
            {
                fColor.b += 0.1f * Time.deltaTime;
            }

            FDensity -= speed * Time.deltaTime;

            RenderSettings.fogColor = fColor;
            RenderSettings.fogDensity = FDensity;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            useCaveFog = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            useCaveFog = false;
        }
    }
}