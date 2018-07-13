using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public enum KeyInput { Horizontal, Vertical, Rotation, Jump, Ability, Eat }
/// <summary>
/// Handels the input and controls the changes
/// </summary>
public class InputManager : MonoBehaviour
{

    public static InputManager Instance;
    private int xboxController = 0;
    private int playstationController = 0;
    private bool waitingForKey;
    private bool coroutinerunning;
    private string tempCode;
    private Event keyevent;
    private StoredInformation storage;
    private string gameDataProjectFilePath = "/StreamingAssets/UserInputSettings.json";

    //Lists and dictionaries
    private List<AxisBase> inputAxes = new List<AxisBase>();
    [SerializeField]
    private List<AxisBase> DefaultAxes = new List<AxisBase>();

    public List<AxisBase> InputAxes
    {
        get
        {
            return inputAxes;
        }

    }

    /// <summary>
    /// Assigns a new axis and stores the information
    /// </summary>
    /// <param name="input"></param>
    /// <param name="keyvalue"></param>
    public void AssignKeyboardInput(AxisBase axis)
    {
        if (!storage.configurations.Contains(axis))
        {
            storage.configurations.Add(axis);
        }
        else
        {
            storage.configurations.Remove(storage.configurations.Find(x => x.AxisName == axis.AxisName));
            storage.configurations.Add(axis);
        }
        StorageToInput();
    }


    public void AssignControllerButtons()
    {
        if (playstationController == 1)
        {

            return;
        }

        if (xboxController == 1)
        {

            return;
        }

        Debug.Log("No controller detected!");
    }

    private void StorageToInput()
    {
        inputAxes = storage.configurations;
        SaveInput();
    }

    #region Axis Related Functions

    /// <summary>
    /// Loads data from json file 
    /// </summary>
    private void LoadInput()
    {
        string filepath = Application.dataPath + gameDataProjectFilePath;
        if (File.Exists(filepath))
        {
            string dataAsJson = File.ReadAllText(filepath);
            if (dataAsJson != null)
            {
                storage = JsonUtility.FromJson<StoredInformation>(dataAsJson);
                Debug.Log(storage.configurations.Count);
            }
        }
        else
        {
            storage = new StoredInformation();

        }

        if (storage.configurations.Count > 0) return;
        Debug.Log("Loading Def");
            storage.configurations = DefaultAxes;
    }

    /// <summary>
    /// Saves data to json file
    /// </summary>
    private void SaveInput()
    {
        string dataAsJson = JsonUtility.ToJson(storage);

        string filePath = Application.dataPath + gameDataProjectFilePath;
        File.WriteAllText(filePath, dataAsJson);
    }

    public void ResetAllAxes()
    {
        foreach (AxisBase a in inputAxes)
        {
            int pKeyValue = 0;
            pKeyValue = (int)DefaultAxes.Find(x => x.AxisName == a.AxisName).Pkey;
            a.Pkey = (KeyCode)pKeyValue;
            AssignKeyboardInput(a);
            inputAxes = storage.configurations;
        }
    }

    public void LoadAllAxes()
    {
        storage.configurations.Sort((x,y) => x.AxisName.CompareTo(y.AxisName));
        inputAxes = storage.configurations;
        Debug.Log("Load all axes");
    }

    #endregion

    #region Getters
    /// <summary>
    /// Used to get users input when key is held down
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetAxis(string name)
    {
        float v = 0;

        for (int i = 0; i < inputAxes.Count; i++)
        {
            if (inputAxes[i].AxisName == name)
            {
                v = inputAxes[i].Axis;
            }
        }
        return v;
    }
    /// <summary>
    /// Used to get the users input when button is held down
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetButton(string name)
    {
        bool retVal = false;

        for (int i = 0; i < inputAxes.Count; i++)
        {
            if (inputAxes[i].AxisName == name)
            {
                retVal = inputAxes[i].positive;
            }
        }
        return retVal;
    }
    #endregion Input getters

    #region UnityMethdods

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadInput();
        string[] names = Input.GetJoystickNames();

        if (names.Length == 19)
        {
            playstationController = 1;
            xboxController = 0;
        }
        else if (names.Length == 33)
        {
            playstationController = 0;
            xboxController = 1;
        }

        if (inputAxes.Count <= 0)
        {
            for (int i = 0; i < storage.configurations.Count; i++)
            {
                AssignKeyboardInput(storage.configurations[i]);
            }
        }
        else
        {
            for(int i = 0; i < inputAxes.Capacity; i++)
            {
                AssignKeyboardInput(inputAxes[i]);
            }
        }
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < inputAxes.Count; i++)
        {
            AxisBase a = inputAxes[i];

            a.negative = (Input.GetKey(a.Nkey));
            a.positive = (Input.GetKey(a.Pkey));

            if(a.negative)
            {
                a.TargetAxis--;
            }

            if(a.positive)
            {
                a.TargetAxis++;
            }

            if(!a.positive && !a.negative)
            {
                a.TargetAxis = 0;
            }
            
            a.Axis = Mathf.MoveTowards(a.Axis, a.TargetAxis, Time.deltaTime * a.Sensitivity);
        }
    }
    #endregion
}

#region Serialized Classes
//No touchie boios

[System.Serializable]
public class StoredInformation
{
    public List<AxisBase> configurations = new List<AxisBase>();
    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class AxisBase
{
    public string AxisName;

    public KeyCode Pkey;
    public KeyCode Nkey;
    [HideInInspector]
    public bool positive;
    [HideInInspector]
    public bool negative;
    [HideInInspector]
    public float Axis;
    private float targetAxis;
    public float Sensitivity = 3;
    public string PkeyDescription;
    public string NkeyDescription;
    [HideInInspector]
    public AxisButton NUIButton;
    [HideInInspector]
    public AxisButton PUIButton;

    public float TargetAxis
    {
        get
        {
            return targetAxis;
        }

        set
        {
            targetAxis = Mathf.Clamp(value, -1, 1);
        }
    }

}
#endregion
