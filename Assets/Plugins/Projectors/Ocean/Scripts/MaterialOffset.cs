using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialOffset : MonoBehaviour
{
    public int materialIndex = 0;

    public string textureName = "_MainTex";
    public Vector2 uvAnimationRate = new Vector2(1.0f, 0.0f);
    public bool moveTexture;

    public string normalName = "_BumpMap";
    public Vector2 uvAnimationRateNormal = new Vector2(1.0f, 0.0f);
    public bool moveNormal;


    public float amplitude;
    public float frequency;

    public Renderer MaterialRenderer;

    Vector2 uvOffset = Vector2.zero;
    Vector2 uvOffsetNormal = Vector2.zero;


    void LateUpdate()
    {
        uvOffset += (uvAnimationRate * Time.deltaTime * (Mathf.Sin(2 * Mathf.PI * frequency * Time.time) - Mathf.Sin(2 * Mathf.PI * frequency * (Time.time - Time.deltaTime))));
        uvOffsetNormal += (uvAnimationRateNormal * Time.deltaTime * (Mathf.Sin(2 * Mathf.PI * frequency * Time.time) - Mathf.Sin(2 * Mathf.PI * frequency * (Time.time - Time.deltaTime))));


        //transform.position += amplitude * (Mathf.Sin(2 * Mathf.PI * frequency * Time.time) - Mathf.Sin(2 * Mathf.PI * frequency * (Time.time - Time.deltaTime))) * transform.forward;

        /*
        if (MaterialRenderer.enabled)
        {
            if (moveTexture)
            {
                MaterialRenderer.materials[materialIndex].SetTextureOffset(textureName, uvOffset);
            }
            if (moveNormal)
            {
                MaterialRenderer.materials[materialIndex].SetTextureOffset(normalName, uvOffsetNormal);
            }

        }
        */
    }
}
