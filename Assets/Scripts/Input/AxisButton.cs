using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AxisButton : MonoBehaviour
{
    private bool negative;
    private string axisN;
    private string keyV;
    public Text AxisDescText;
    public Text CurrentBindKey;

    public void Init(string axisname, string axisdesc, string keyvalue)
    {
        ChangeKeyText(axisname);
        axisN = axisname;
        keyV = keyvalue;
        ChangeKeyText(keyvalue);
        AxisDescText.text = axisdesc;
    }
    public void Init(string axisname, string axisdesc, string keyvalue, bool negativeKey)
    {
        ChangeKeyText(axisname);
        axisN = axisname;
        keyV = keyvalue;
        ChangeKeyText(keyvalue);
        AxisDescText.text = axisdesc;
        negative = negativeKey;
    }
    public void ChangeKeyText(string axisname)
    {
        string temp;
        char[] a = { 'A', 'a', 'l', 'p', 'h', ' ' };
        if(axisname.Contains("Alpha"))
        {
        temp = axisname.TrimStart(a);
        CurrentBindKey.text = temp;
        }
        else
        {
            CurrentBindKey.text = axisname;
        }
        
    }

    public void OpenDialog()
    {
        ControlsMenu.Instance.OpenRebindButtonDialog(axisN, negative);
    }
}
