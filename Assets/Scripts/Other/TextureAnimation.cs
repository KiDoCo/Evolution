using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureAnimation : MonoBehaviour
{
    public enum AnimMode    {   forwards,backwards,random   }

    public Texture[] textures;
    public float fps = 15;

    public AnimMode animMode = AnimMode.forwards;

    private int frameNr = 0;
    public Renderer AnimRenderer;

    void Start()
    {
        AnimRenderer = GetComponent(typeof(Renderer)) as Renderer;

        if (AnimRenderer == null)
        {
            Debug.LogWarning("TextureAnimation: No Renderer found on this gameObject", this);
            enabled = false;
        }
        
        StartCoroutine("switchTexture");
    }
    
    IEnumerator switchTexture()
    {
        while (true)
        {
            AnimRenderer.material.mainTexture = textures[frameNr];

            yield return new WaitForSeconds(1.0f / fps);
            switch (animMode)
            {
                case AnimMode.forwards: frameNr++; if (frameNr >= textures.Length) frameNr = 0; break;
                case AnimMode.backwards: frameNr--; if (frameNr < 0) frameNr = textures.Length - 1; break;
                case AnimMode.random: frameNr = Random.Range(0, textures.Length); break;
            }
        }
    }
}
