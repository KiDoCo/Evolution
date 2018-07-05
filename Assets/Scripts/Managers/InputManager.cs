using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public enum KeyInput { Horizontal, Vertical, Rotation, Jump, Mouse }

public class InputManager : MonoBehaviour
{

    public static InputManager Instance;
    private int xboxController = 0;
    private int playstationController = 0;
    private bool waitingForKey;
    private Event keyevent;
    private KeyCode tempCode;
    private StoredInformation storage;
    private string gameDataProjectFilePath = "/StreamingAssets/data.json";

    public Dictionary<KeyCode, float> InputDictionary = new Dictionary<KeyCode, float>();

    [SerializeField] private Dictionary<KeyCode, float> DefaultInputDictionary = new Dictionary<KeyCode, float>();

    public void AssignKeyboardInput(KeyCode input, float keyvalue)
    {
        Debug.Log("Key pressed :" + input + " And Value :" + keyvalue);
        if (!InputDictionary.ContainsKey(input))
        {
            InputDictionary.Add(input, keyvalue);
        }
        else
        {
            InputDictionary.Remove(input);
            InputDictionary.Add(input, keyvalue);
        }
         if (!storage.keys.Contains(input))
          {
            storage.keys.Add(input);
            storage.values.Add(keyvalue);
          }
          else
          {
            storage.values.RemoveAt(storage.keys.FindIndex(x => x == input));
            storage.keys.Remove(input);
            storage.keys.Add(input);
            storage.values.Add(keyvalue);
          }

        SaveInput();
        tempCode = KeyCode.None;
    }
    /// <summary>
    /// Loads data from json file 
    /// </summary>
    private void LoadInput()
    {
        string filepath = Application.dataPath + gameDataProjectFilePath;
        if (File.Exists(filepath))
        {
            string dataAsJson = File.ReadAllText(filepath);
            storage = JsonUtility.FromJson<StoredInformation>(dataAsJson);
        }
        else
        {
            storage = new StoredInformation();

        }
        for(int i = 0; i < storage.keys.ToArray().Length; i++)
        {
            InputDictionary.Add(storage.keys[i], storage.values[i]);

        }
        Debug.Log(InputDictionary.ToArray().Length);
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


    /// <summary>
    /// Function for button (Value is given in editor
    /// </summary>
    /// <param name="value"></param>
    public void Button(float value)
    {
        StartCoroutine(Assign(value));
    }

    private IEnumerator WaitForKey()
    {
        while (!keyevent.isKey)
        {
            Debug.Log("Called waitforkey");
            yield return null;
        }

    }
    private IEnumerator Assign(float value)
    {
        waitingForKey = true;
        yield return WaitForKey();
        AssignKeyboardInput(tempCode, value);
    }

    public float GetKey(KeyCode key)
    {
        return InputDictionary[key];
    }

    public float GetKeyDown(KeyCode key)
    {

        return InputDictionary[key];
    }

    private void LateUpdate()
    {

    }

    //Unity Methods

    private void OnGUI()
    {
        keyevent = Event.current;

        if (keyevent.isKey && waitingForKey)
        {
            tempCode = keyevent.keyCode;
            waitingForKey = false;
        }

    }

    private void Awake()
    {
        if (DefaultInputDictionary.Count > 0) return;

        DefaultInputDictionary.Add(KeyCode.A, 1.0f);
        DefaultInputDictionary.Add(KeyCode.D, -1.0f);
        DefaultInputDictionary.Add(KeyCode.W, 1.0f);
        DefaultInputDictionary.Add(KeyCode.S, -1.0f);
        DefaultInputDictionary.Add(KeyCode.Q, 1.0f);
        DefaultInputDictionary.Add(KeyCode.E, -1.0f);
        DefaultInputDictionary.Add(KeyCode.Space, 1.0f);
        DefaultInputDictionary.Add(KeyCode.LeftControl, -1.0f);
        DefaultInputDictionary.Add(KeyCode.Mouse0, 1.0f);
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


        if (InputDictionary.Count <= 0)
        {
            Debug.Log("is empty");
            for (int i = 0; i < DefaultInputDictionary.Count; i++)
            {
                AssignKeyboardInput(DefaultInputDictionary.ElementAt(i).Key, DefaultInputDictionary.ElementAt(i).Value);
            }
        }
        else
        {

        }
    }
}

[System.Serializable]
public class StoredInformation
{
    public List<KeyCode> keys;
    public List<float> values;

    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
