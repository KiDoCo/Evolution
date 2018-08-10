using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloakAbility : MonoBehaviour
{


    private bool cloaked = false;

    private Color visibilityColor;

    public Material defaultMat;
    public Material glassMat;
    public float defSmooth = 0;
    private SkinnedMeshRenderer skin;

    private void Start()
    {
        skin = gameObject.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
        visibilityColor = skin.material.color;
    }


    private void Update() //Delete this update
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleCloak();

        }
    }

    private void ToggleCloak() //Change this to non-toggle if needed
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


    private void SetNormal() //Change material
    {
        skin.material = defaultMat;

    }
    private void SetGlass() //Change material
    {
        skin.material = glassMat;

        StartCoroutine(SetBump());
    }


    private IEnumerator SwapColor(Color goal) //Change visiblity of normal material
    {
       skin.material.SetFloat("_Glossiness", 0);
        float backUpTimer = 0;
        while (Mathf.Abs(skin.material.color.a - goal.a) > 0.05f && backUpTimer < 2)
        {

            backUpTimer += Time.deltaTime;
           skin.material.color = Color.Lerp(skin.material.color, goal, 15 * Time.deltaTime);
            yield return null;

        }
       skin.material.color = goal;
        if (cloaked) { SetGlass(); }
        else {skin.material.SetFloat("_Glossiness", defSmooth); }
    }

    private IEnumerator SetBump() //Increase distorion for glass
    {
        float backUpTimer = 0;
        float i = 0;
        while (i < 50 && backUpTimer < 2)
        {
            i = Mathf.Lerp(i, 60, Time.deltaTime * 5);
            backUpTimer += Time.deltaTime;
           skin.material.SetFloat("_BumpAmt", i);
            yield return null;

        }

    }

    private IEnumerator RemoveGlass() //Decrease distortion for glass
    {
        float backUpTimer = 0;
        float i = 50;
        while (i > 8 && backUpTimer < 2)
        {
            i = Mathf.Lerp(i, 0, Time.deltaTime * 5);
            backUpTimer += Time.deltaTime;
           skin.material.SetFloat("_BumpAmt", i);
            yield return null;

        }
        SetNormal();
       skin.material.color = visibilityColor;
        visibilityColor.a = 1;
        StartCoroutine(SwapColor(visibilityColor));
    }
}
