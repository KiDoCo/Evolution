using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject hud = null;
    [SerializeField] private GameObject helix;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject matchResultScreen;
    [SerializeField] private GameObject helixCamera;
    
    public static void switchGameObject(GameObject[] list, GameObject obj)
    {
        foreach (GameObject o in list)
        {
            if (o == obj)
                o.SetActive(true);
            else
                o.SetActive(false);
        }
    }


    private void DisplayTime()
    {
        // Gamemanager.Instance.MatchTimer / 60;
    }

    private void InstantiateMainMenuUI()
    {

    }

    #region MatchUI


    private void MatchResultScreen(Character source)
    {

    }
    public void InstantiateInGameUI(Character source)
    {
        Instantiate(hud);
        Instantiate(pauseMenu);

        if (source.GetType() == typeof(Herbivore))
        {
            GameObject clone = Instantiate(helix, GameObject.Find("HelixLocation").transform.position, Quaternion.identity);
            HUDController.Instance.Inst(clone);
            GameObject cameraClone = Instantiate(helixCamera, GameObject.Find("CameraLocation").transform.position, Quaternion.identity);
            cameraClone.transform.rotation = Quaternion.Euler(90, 180, 0);
        }
    }
    //Match UI
    public void UpdateMatchUI(Herbivore source)
    {
        HUDController.Instance.MaxHealth = (int)source.Maxhealth;
        HUDController.Instance.CurProgress = source.Experience;
        HUDController.Instance.CurHealth = (int)source.Health;
    }
    #endregion

    //unity methods

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == NetworkGameManager.Instance.playScene)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu.Instance.UI.SetActive(!PauseMenu.Instance.UI.activeSelf);

                if (PauseMenu.Instance.UI.activeSelf)
                {
                    HideCursor(false);
                }
                else
                {
                    HideCursor(true);
                }
            }
        }
    }

    public void HideCursor(bool hide)
    {
        Cursor.lockState = hide ? CursorLockMode.Locked : CursorLockMode.None; 
        Cursor.visible = !hide;
    }
}
