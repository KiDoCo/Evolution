using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebindButton : MonoBehaviour
{
    public UnityEngine.UI.Text axisText;

    public void Init(string n)
    {
        axisText.text = n;
    }
}
