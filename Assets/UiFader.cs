using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiFader : MonoBehaviour {

    Image FadeShape;
    //ParticleSystem bubbler;
    GameObject MenuGrid;
    //ParticleSystem.ShapeModule BubblerMover;

	void Start ()
    {
        MenuGrid = transform.Find("MainMenuGrid").gameObject;
        //bubbler = GetComponent<ParticleSystem>();
        //BubblerMover = bubbler.shape;
        FadeShape = GetComponent<Image>();
        FadeShape.fillAmount = 1;
	}

    public void StartFader()
    {
        StartCoroutine(Fader());
        //bubbler.Play();
    }

    private IEnumerator Fader()
    {
        while (FadeShape.fillAmount != 0)
        {
            yield return new WaitForSeconds(0.00001f);
            //BubblerMover.position += new Vector3(0.06f, 0, 0); 
            FadeShape.fillAmount -= 0.01f;
        }
        MenuGrid.SetActive(false);
        //bubbler.Stop();
    }
}
