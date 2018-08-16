using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerCaveFog : MonoBehaviour
{
    public bool useFog;

    public Color defaultColor;
    public float defaultDensity;

    public Color caveColor;     //  23, 25, 46      //  ColorCode  -   17172E
    public float caveDensity = 0.085f;

    public float speed = 0.02f; // how fast is color change

    private Color fColor;
    private float fDensity;

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
        useFog = false;

        defaultColor = RenderSettings.fogColor;
        defaultDensity = RenderSettings.fogDensity;

        fColor = defaultColor;
        FDensity = defaultDensity;
    }

	void Update ()
    {
        if(transform.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            if (useFog)
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
	}

    public void changeFog (bool use)
    {
        if (use)
        {
            useFog = true;
        }
        else
        {
            useFog = false;
        }
    }
}