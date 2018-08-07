using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance;
#pragma warning disable
    private float curProgress;

    #region Texts

    //General
    [SerializeField] private Text curHealthText;
    [SerializeField] private Text maxHealthText;
    [SerializeField] private Text matchTime;
    [SerializeField] private Text inGameMatchTime;

    //herbivores
    [SerializeField] private Text deathCount;
    [SerializeField] private Text expCount;
    [SerializeField] private Text SurTime;

    //Carnivores
    [SerializeField] private Text carnivoreRank;

    #endregion

    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject HresultScreen;
    [SerializeField] private GameObject CresultScreen;
    [SerializeField] private GameObject ResScreen;
    private Animator helixAnim;
    private Animator predatorMouthAnim;
    private AnimationClip clip;
    private PredatorRanks rank;
    [SerializeField] private int maxHealth;
    private int curHealth;

    //Cooldowns
    [SerializeField] private int abilityCount;
    [SerializeField] private List<GameObject> coolDownObjects;
#pragma warning restore

    #region getters&setters

    public Text CurHealthText
    {
        get
        {
            return curHealthText;
        }

        set
        {
            curHealthText = value;
        }
    }

    public Text MaxHealthText
    {
        get
        {
            return maxHealthText;
        }

        set
        {
            maxHealthText = value;
        }
    }

    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }

        set
        {
            maxHealth = value;
        }
    }

    public int CurHealth
    {
        get
        {
            return curHealth;
        }

        set
        {
            curHealth = value;
        }
    }

    public float CurProgress
    {
        get
        {
            return curProgress;
        }

        set
        {
            curProgress = value;
        }
    }

    public Animator HelixAnim
    {
        get
        {
            return helixAnim;
        }

        set
        {
            helixAnim = value;
        }
    }

    public Animator PredatorMouthAnim
    {
        get
        {
            return predatorMouthAnim;
        }

        set
        {
            predatorMouthAnim = value;
        }
    }

    #endregion

    /// <summary>
    /// Instantiates the helix
    /// </summary>
    /// <param name="obj"></param>
    public void InstantiateHelix(GameObject obj)
    {
        HelixAnim = obj.GetComponent<Animator>();
    }

    public void InstantiateTeeth(GameObject obj)
    {
        PredatorMouthAnim = obj.transform.GetChild(1).GetComponent<Animator>();
    }

    /// <summary>
    /// Updates the hud for the player
    /// </summary>
    public void UpdateHUD()
    {
        int Atime = (int)InGameManager.Instance.MatchTimer / 60;
        int Btime = (int)InGameManager.Instance.MatchTimer % 60;
        curHealthText.text = "" + curHealth;
        maxHealthText.text = "/" + maxHealth;
        inGameMatchTime.text = Atime + " : " + Btime.ToString("00");
    }

    /// <summary>
    /// Changes the helix animation
    /// </summary>
    public void AnimationChanger()
    {
        if (HelixAnim != null)
            HelixAnim.Play("Take", 0, (1.0f / 99) * curProgress);
    }

    /// <summary>
    /// Updates the player Cooldown UI
    /// </summary>
    private void UpdateCooldowns() 
    {
        StartCoroutine(Cooldown(5, coolDownObjects[0]));
        StartCoroutine(Cooldown(5, coolDownObjects[1]));
        StartCoroutine(Cooldown(5, coolDownObjects[2]));
    }

    /// <summary>
    /// Prints out the result screen for the player
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="death"></param>
    /// <param name="time"></param>
    /// <param name="surtime"></param>
    /// <param name="herbivore"></param>
    public void ResultScreen(float exp, int death, float time,float surtime, bool herbivore)
    {
        ingameUI.SetActive(false);
        ResScreen.SetActive(true);

        if (!herbivore)
        {
            if(death == 0)
            {
                rank = PredatorRanks.Pacifish; 
            }
            /*else if(?) // if only one herbivore survives, add logic to this
            {
                rank = PredatorRanks.FishersPrey;
            }*/
            else if (GameObject.Find("Herbivore") == null)
            {
                rank = PredatorRanks.ApexPredator;
            }
            else
            {
                rank = PredatorRanks.Predator;
            }
        }

        int temp = (int)exp;
        int rTime = (int)(InGameManager.Instance.StartingMatchTimer - time);
        int Atime = rTime / 60;
        int Btime = rTime % 60;
        matchTime.text = Atime + " : " + Btime.ToString("00");

        if (herbivore)
        {
            int xTemp = (int)surtime;
            int yTime = xTemp / 60;
            int zTime = xTemp % 60;
            HresultScreen.SetActive(true);
            deathCount.text = death.ToString();
            SurTime.text = yTime + " : " + zTime.ToString("00");
            expCount.text = temp.ToString();
        }
        else
        {
            CresultScreen.SetActive(true);
            deathCount.text = death.ToString();
            carnivoreRank.text = AddSpacesToSentence(rank.ToString());
        }
    }

    /// <summary>
    /// Adds spaces between capital letters
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string AddSpacesToSentence(string text)
    {
        
        if (string.IsNullOrEmpty(text))
            return "";
        System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }

    /// <summary>
    /// Sets the cooldown number and gives the info for UI elements
    /// </summary>
    /// <param name="cd"></param>
    /// <param name="cdObj"></param>
    /// <returns></returns>
    IEnumerator Cooldown(float cd, GameObject cdObj)
    {
        float temp = 0;
        while (temp < cd)
        {
            temp += Time.deltaTime;
            cdObj.GetComponentInChildren<Text>().text = ((int)(cd - temp)).ToString();
            cdObj.transform.GetChild(0).GetComponent<Image>().fillAmount = 1 - temp / cd;
            yield return null;
        }
        cdObj.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
        cdObj.GetComponentInChildren<Text>().text = "";
    }


    private void Awake()
    {
        Instance = this;
        HresultScreen.SetActive(false);
        CresultScreen.SetActive(false);
    }

    private void Update()
    {
        UpdateHUD();
        AnimationChanger();
        // UpdateCooldowns();
    }
}
