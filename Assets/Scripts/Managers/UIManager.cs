using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable]
public class InspectorObject
{
    [SerializeField] private string name = null;
    [SerializeField] private GameObject obj = null;

    public string Name
    {
        get
        {
            return name;
        }
    }

    public GameObject Object
    {
        get
        {
            return obj;
        }
    }

    // --------------------------

    // Enables only one of the objects in InspectorObject list and disables others
    public static void switchGameObject(List<InspectorObject> list, string name)
    {
        bool found = false;

        foreach (InspectorObject o in list)
        {
            if (o.Name == name)
            {
                found = true;
                o.Object.SetActive(true);
            }
            else
            {
                o.Object.SetActive(false);
            }
        }

        if (!found)
        {
            Debug.Log("UIManager, switchUIWindow: " + name + " gameObject not found!");
        }
    }

    public static void enableGameObject(List<InspectorObject> list, string name, bool enabled = true)
    {
        list.Find(x => x.Name == name).Object.SetActive(enabled);
    }
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject hud = null;
    [SerializeField] private GameObject pauseMenu = null;

    //Match UI
	public void UpdateMatchUI(Character source)
    {
        HUDController.instance.MaxHealth   = (int)source.Maxhealth;
        HUDController.instance.CurProgress = source.Experience;
        HUDController.instance.CurHealth   = (int)source.Health;
    }

    public void InstantiateInGameUI()
    {
        //Instantiate(hud);
        Instantiate(pauseMenu);
    }

    public void UpdateCharacterExperience()
    {

    }

    private void DisplayTime()
    {
        // Gamemanager.Instance.MatchTimer / 60;
    }

//-------------------------------------------------------------------------------------
    //Main Menu UI

    private void InstantiateMainMenuUI()
    {

    }

    private void MatchResultScreen(Character source)
    {

    }

    private void Awake()
    {
        Instance = this;
    }

    //unity methods
    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.Instance.UI.SetActive(!PauseMenu.Instance.UI.activeSelf);

            if (PauseMenu.Instance.UI.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
