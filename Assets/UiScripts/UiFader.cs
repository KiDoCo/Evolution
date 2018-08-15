using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiFader : MonoBehaviour {

    Image FadeShape;

    [SerializeField]
    float FilledAmount;

    [SerializeField]
    float UnfilledAmount;

	void Start ()
    {
        FadeShape = GetComponent<Image>();
        FadeShape.fillAmount = FilledAmount;
	}

    private void OnEnable()
    {
        FadeShape.fillAmount = FilledAmount;
    }

    private void OnDisable()
    {
        StartCoroutine(Fader());
    }

    private IEnumerator Fader()
    {
        while (FadeShape.fillAmount >= UnfilledAmount)
        {
            yield return new WaitForSeconds(0.00001f);
            FadeShape.fillAmount -= 0.01f;
        }
    }
}
