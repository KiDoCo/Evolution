using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloakAbility : MonoBehaviour {
    
    bool cloaked = false;

    Color visibilityColor;

    public Material defaultMat;
    public Material glassMat;

    void Start ()
    {
        visibilityColor = gameObject.GetComponent<MeshRenderer>().material.color;
    }

	
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleCloak();

        }
    }

    void ToggleCloak()
    {
        Debug.Log("toggle cloak");
        cloaked = !cloaked;
        if (cloaked)
        {
            visibilityColor.a = 0;
            StartCoroutine(SwapColor(visibilityColor));
            //Find out a way to disable the material reflections
            //gameObject.GetComponent<MeshRenderer>().material.SetFloat("_SpecularHighlights", 0f);

        }
        else
        {
            StartCoroutine(RemoveGlass());
        }
    }


    void SetNormal()
    {
        gameObject.GetComponent<MeshRenderer>().material = defaultMat;
        
    }
    void SetGlass()
    {
        gameObject.GetComponent<MeshRenderer>().material = glassMat;
        
        StartCoroutine(SetBump());
    }


    IEnumerator SwapColor(Color goal)
    {
        float backUpTimer = 0;
        while (Mathf.Abs(gameObject.GetComponent<MeshRenderer>().material.color.a - goal.a) > 0.05f && backUpTimer < 2)
        {

            backUpTimer += Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(gameObject.GetComponent<MeshRenderer>().material.color, goal, 12 * Time.deltaTime);
            yield return null;

        }
        gameObject.GetComponent<MeshRenderer>().material.color = goal;
        if (cloaked) { SetGlass(); }
    }

    IEnumerator SetBump()
    {
        float backUpTimer = 0;
        float i = 0;
        while (i < 50 && backUpTimer < 2)
        {
            i = Mathf.Lerp(i, 60, Time.deltaTime * 5);
            backUpTimer += Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_BumpAmt", i);
            yield return null;

        }
    }

    IEnumerator RemoveGlass()
    {
        float backUpTimer = 0;
        float i = 50;
        while (i > 8 && backUpTimer < 2)
        {
            i = Mathf.Lerp(i, 0, Time.deltaTime * 5);
            backUpTimer += Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_BumpAmt", i);
            yield return null;

        }
        SetNormal();
        gameObject.GetComponent<MeshRenderer>().material.color = visibilityColor;
        visibilityColor.a = 1;
        StartCoroutine(SwapColor(visibilityColor));
    }
}
