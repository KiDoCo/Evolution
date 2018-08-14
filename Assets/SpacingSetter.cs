using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpacingSetter : MonoBehaviour {

	public void Update()
    {
       float textWidth = GetComponentInParent<Text>().preferredWidth;

        GetComponent<HorizontalLayoutGroup>().spacing = textWidth;
	}
}
