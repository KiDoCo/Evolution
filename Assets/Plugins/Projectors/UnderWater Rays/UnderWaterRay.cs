using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderWaterRay : MonoBehaviour
{
    private Renderer WaterRenderer;

    private Vector3 startSize;
    private Vector3 startPosition;
    
    public enum Speed { slow, fast  }
    public Speed speed;

    public bool loopAll = false;    //  if true. Plays all states in order
    public enum State { appear, loop, disappear}
    public State state;

    [Range(0, 15f)]
    public int duration = 8; // if loopAll selected. How long to complete each of states
    private float currentStateTime;

    private float posY;     //  Current position Y
    private float scaleZ;   //  Current Scale Z

    [Range(0, 20f)]
    public float targetScale = 10f;   // How long
    
    [Range(0, 0.1f)]
    public float minTransparency = 0.001f;
    [Range(0, 0.2f)]
    public float maxTransparency = 0.05f;
    private float transparency; //  Current Transparency

    [Range(0, 0.001f)]
    public float fadeSpeed = 0.0001f;   //  Speed to disappear
    [Range(1, 3.0f)]
    public float loopSpeed = 1f;   //  Looping Speed


    void Start ()
    {
        transparency = 0f;

        startSize = transform.localScale;
        startPosition = transform.position;
    
        WaterRenderer = GetComponent(typeof(Renderer)) as Renderer;
        if (WaterRenderer == null)
        {
            Debug.LogWarning("UnderWaterRay: No Renderer found on this gameObject", this);
            enabled = false;
        }

        posY = startPosition.y;
        scaleZ = transform.localScale.z;
    }


	void Update ()
    {
        if (loopAll)
        {
            currentStateTime += 1 * Time.deltaTime;

            if (currentStateTime >= duration * 2)//Start>>Half
            {
                state = State.loop;
            }
            if (currentStateTime >= duration * 3)//Half>>End
            {
                state = State.disappear;
            }
            if (currentStateTime >= duration * 4)//End>>Start   -   reset
            {
                currentStateTime = 0f;
                state = State.appear;

                transparency = 0;

                transform.position = startPosition;
                transform.localScale = startSize;
            }
        }

        if (state == State.appear)
        {
            if (transform.localScale.z <= targetScale) //  LIMIT
            {
                if (speed == Speed.slow)
                {
                    posY -= 3.0f * Time.deltaTime;
                    scaleZ += 0.6f * Time.deltaTime;
                }
                else if (speed == Speed.fast)
                {
                    posY -= 6.0f * Time.deltaTime;
                    scaleZ += 1.2f * Time.deltaTime;
                }    
                if (transparency < maxTransparency) //if (transparency < 0.19f)
                {
                    transparency += 0.0001f;
                }
            
                Color newColor = WaterRenderer.material.GetColor("_TintColor");
                WaterRenderer.material.SetColor("_TintColor", new Vector4(newColor.r, newColor.g, newColor.b, transparency));

                transform.position = new Vector3(transform.position.x, posY, transform.position.z); //  Move Down
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scaleZ); //  Scale Up
            }
        }

        if (state == State.loop)
        {
            Color a = WaterRenderer.material.GetColor("_TintColor"); a.a = maxTransparency / 2;  //  a.a = 0.015f;
            Color b = WaterRenderer.material.GetColor("_TintColor"); b.a = maxTransparency;  //  b.a = 0.03f;

            Color newColor = Color.Lerp(a,b, Mathf.PingPong(Time.time, loopSpeed)); // Lerping color between a & b     //  1.25f
            WaterRenderer.material.SetColor("_TintColor", newColor);
        }

        if (state == State.disappear)
        {
            if (transform.localScale.z >= 0) //  LIMIT
            {
                if (speed == Speed.slow)
                {
                    posY += 3.0f * Time.deltaTime;
                    scaleZ -= 0.6f * Time.deltaTime;
                }
                else if (speed == Speed.fast)
                {
                    posY += 6.0f * Time.deltaTime;
                    scaleZ -= 1.2f * Time.deltaTime;
                }
                if (transparency > 0.001f)
                {
                    transparency -= fadeSpeed;  //  transparency -= 0.0001f;
                }
                
                Color newColor = WaterRenderer.material.GetColor("_TintColor");
                WaterRenderer.material.SetColor("_TintColor", new Vector4(newColor.r, newColor.g, newColor.b, transparency));

                transform.position = new Vector3(transform.position.x, posY, transform.position.z); //  Move Up
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scaleZ); //  Scale Down
            }
        }
    }

}