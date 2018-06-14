using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloakAbility : MonoBehaviour {
    
    bool cloaked = false;

    Color visibilityColor;

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
            visibilityColor.a = 0.25f;
            StartCoroutine(SwapColor(visibilityColor));
        }
        else
        {
            visibilityColor.a = 1;
            StartCoroutine(SwapColor(visibilityColor));
        }
    }

    IEnumerator SwapColor(Color goal)
    {
        float backUpTimer = 0;
        while (Mathf.Abs(gameObject.GetComponent<MeshRenderer>().material.color.a - goal.a) > 0.05f && backUpTimer < 2)
        {

            backUpTimer += Time.deltaTime;
            gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(gameObject.GetComponent<MeshRenderer>().material.color, goal, 4 * Time.deltaTime);
            yield return null;

        }
        gameObject.GetComponent<MeshRenderer>().material.color = goal;
    }
}
