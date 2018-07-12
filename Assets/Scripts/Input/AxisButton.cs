using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AxisButton : MonoBehaviour
{


    public void Init(string axisname, string axisdesc, string keyvalue)
    {
        GetComponentInChildren<Text>().text = axisname;
        Debug.Log("initialize");

    }
    public void Init(string axisname, string axisdesc, string keyvalue, bool negativeKey)
    {

    }
    public void ChangeKeyText(string text)
    {

    }
}
