using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloakAbility : MonoBehaviour {
   

    bool cloaked = false;

    Color visibilityColor;
   
    //Uses 2 materials to swap between when cloak is used
    public Material defaultMat;
    public Material glassMat;
    public float defSmooth = 0;

    void Start ()
    {
        visibilityColor = gameObject.GetComponent<MeshRenderer>().material.color;
    }

	
	void Update () //Delete this update
    {

		if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleCloak();

        }
    }

    void ToggleCloak() //Change this to non-toggle if needed
    {
        Debug.Log("toggle cloak");
        cloaked = !cloaked;
        if (cloaked)
        {
            visibilityColor.a = 0;
            StartCoroutine(SwapColor(visibilityColor));

        }
        else
        {
            StartCoroutine(RemoveGlass());
        }
    }


    void SetNormal() //Change material
    {
        gameObject.GetComponent<MeshRenderer>().material = defaultMat;
        
    }
    void SetGlass() //Change material
    {
        gameObject.GetComponent<MeshRenderer>().material = glassMat;
        
        StartCoroutine(SetBump());
    }


    IEnumerator SwapColor(Color goal) //Change visiblity of normal material
    {
        gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);
        float backUpTimer = 0;
        while (Mathf.Abs(gameObject.GetComponent<MeshRenderer>().material.color.a - goal.a) > 0.05f && backUpTimer < 2)
        {

            backUpTimer += Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(gameObject.GetComponent<MeshRenderer>().material.color, goal, 15 * Time.deltaTime);
            yield return null;

        }
        gameObject.GetComponent<MeshRenderer>().material.color = goal;
        if (cloaked) { SetGlass(); }
        else { gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", defSmooth); }
    }

    IEnumerator SetBump() //Increase distorion for glass
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

    IEnumerator RemoveGlass() //Decrease distortion for glass
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
