using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public static ControlsMenu Instance;

    [SerializeField]
    public List<AxisBase> Axes = new List<AxisBase>();

    private Dictionary<string, int> defaultaxes = new Dictionary<string, int>();

    public GameObject AxisPrefab;
    public Transform AxesGrid;
    public RebindButton RebButton;

    public bool rebinding;
    private bool negativeKey;
    private AxisBase targetAxis;

    bool initOnce;

    public void Init()
    {
        if(!initOnce)
        {
            initOnce = true;
        }

        RebButton.gameObject.SetActive(false);

        InputManager.Instance.LoadAllAxes();
        CreateAxisButtons();
    }

    private void CreateAxisButtons()
    {
        foreach (AxisBase a in Axes)
        {
            GameObject p = Instantiate(AxisPrefab);
            p.transform.SetParent(AxesGrid);
            AxisButton pB = p.GetComponent<AxisButton>();

            pB.Init(a.AxisName, a.PkeyDescription, a.Pkey.ToString());
            a.PUIButton = pB;

            if(a.Nkey != KeyCode.None)
            {
                GameObject n = Instantiate(AxisPrefab);
                n.transform.SetParent(AxesGrid);
                AxisButton nB = n.GetComponent<AxisButton>();

                nB.Init(a.AxisName, a.NkeyDescription, a.Nkey.ToString());
                a.NUIButton = nB;
            }
        }
    }

    public void ResetAllAxes()
    {
        foreach(AxisBase a in Axes)
        {
            int pKeyValue = 0;
            defaultaxes.TryGetValue(a.AxisName + "pKey", out pKeyValue);
            a.Pkey = (KeyCode)pKeyValue;
            InputManager.Instance.AssignKeyboardInput(a);
        }
    }

    public void ChangeInputKey(string name, KeyCode newKey,bool negative = false)
    {
        AxisBase a = ReturnAxis(name);

        if(a==null)
        {
            Debug.Log(name + " dun goofd");
            return;
        }

        if(negative)
        {
            a.Nkey = newKey;
            a.NUIButton.ChangeKeyText(a.Nkey.ToString());
        }
        else
        {
            a.Pkey = newKey;
            a.PUIButton.ChangeKeyText(a.Pkey.ToString());
        }
        InputManager.Instance.AssignKeyboardInput(a);
    }

    private AxisBase ReturnAxis(string name)
    {
        AxisBase retVal = null;

        for(int i  = 0; i < Axes.Count; i++)
        {
            if(name == Axes[i].AxisName)
            {
                retVal = Axes[i];
            }
        }
        return retVal; 
    }

    public void OpenRebindButtonDialog(string axisname, bool negative)
    {
        targetAxis = ReturnAxis(axisname);
        rebinding = true;
        RebButton.Init(axisname);
        RebButton.gameObject.SetActive(true);
        negativeKey = negative;

        
    }

    private void CloseRebinding()
    {
        rebinding = false;
        RebButton.gameObject.SetActive(false);
        AxesGrid.gameObject.SetActive(true);

    }

    public void CancelRebinding()
    {
        CloseRebinding();
    }

    private void Awake()
    {
        Instance = this;
    }

    public void FixedUpdate()
    {
        for(int i = 0; i < Axes.Count; i++)
        {
            AxisBase a = Axes[i];

            a.negative = (Input.GetKey(a.Nkey));
            a.positive = (Input.GetKey(a.Pkey));

            if(a.negative)
            {
                a.TargetAxis = -1;
            }
            else if(a.positive)
            {
                a.TargetAxis = 1;
            }
            else
            {
                a.TargetAxis = 0;
            }

            a.Axis = Mathf.MoveTowards(a.Axis, a.TargetAxis, Time.deltaTime * a.Sensitivity);
        }
    }

//extreme spaghetti
    private void OnGUI()
    {
        if(rebinding)
        {
            AxesGrid.gameObject.SetActive(false);

            Event e = Event.current;

            if(e != null)
            {
                if(e.isKey && !e.isMouse)
                {
                    if(e.keyCode != KeyCode.None)
                    {
                        ChangeInputKey(targetAxis.AxisName, e.keyCode, negativeKey);
                        CloseRebinding();
                        
                    }
                }

                if(e.isMouse && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    KeyCode targetKey = KeyCode.None;

                    switch(e.button)
                    {
                        case 0:
                            targetKey = KeyCode.Mouse0;
                            break;
                        case 1:
                            targetKey = KeyCode.Mouse1;
                            break;
                        default:
                            break;
                    }

                    if(targetKey != KeyCode.None)
                    {
                        ChangeInputKey(targetAxis.AxisName, targetKey, negativeKey);
                        CloseRebinding();
                    }
                }
            }
        }
    }
}
