using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputButtons : MonoBehaviour
{
    private List<Button> buttons = new List<Button>();
    private UnityAction action;
    private UnityAction action2;
    private void SetButtons()
    {
        action = () => InputManager.Instance.Button(1);
        action2 = () => InputManager.Instance.Button(-1);

    }
    private void OnEnable()
    {
        for(int i = 0; i < InputManager.Instance.InputDictionary.Count; i++)
        {
            if(i % 2 == 0)
            {
                buttons[i].onClick.AddListener(action2);
            }
            else
            {
                buttons[i].onClick.AddListener(action);
            }
        }
    }

    public void SetButtonText(string text)
    {

    }
}
