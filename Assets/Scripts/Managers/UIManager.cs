using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject hud = null;
    [SerializeField] private GameObject helix;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject matchResultScreen;

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
            hud.GetComponent<HUDController>().Inst(clone);
        }
    }
    //Match UI
    public void UpdateMatchUI(Character source)
    {
        hud.GetComponent<HUDController>().MaxHealth = (int)source.Maxhealth;
        hud.GetComponent<HUDController>().CurProgress = source.Experience;
        hud.GetComponent<HUDController>().CurHealth = (int)source.Health;
    }
    #endregion

    //unity methods

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.Instance.UI.SetActive(!PauseMenu.Instance.UI.activeSelf);
        }
    }
}
