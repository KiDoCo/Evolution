using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
#pragma warning disable
    private float curProgress; //testing
    public float testanim;
    [SerializeField] private Text curHealthText;
    [SerializeField] private Text maxHealthText;
    [SerializeField] private int maxHealth;
    private int curHealth;
    private Animator anim;
    private AnimationClip clip;
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

    public Animator Anim
    {
        get
        {
            return anim;
        }

        set
        {
            anim = value;
        }
    }

    #endregion

    public void Inst(GameObject obj)
    {
        Debug.Log(obj.GetComponent<Animator>());
        anim = obj.GetComponent<Animator>();
    }

    public void UpdateHUD()
    {
        curHealthText.text = "" + curHealth;
        maxHealthText.text = "/" + maxHealth;
    }

    public void AnimationChanger()
    {
        Anim.Play("Take", 0 , (1.0f / 99) * testanim );
    }

    private void UpdateCooldowns() //for testing
    {
        StartCoroutine(Cooldown(5, coolDownObjects[0]));
        StartCoroutine(Cooldown(5, coolDownObjects[1]));
        StartCoroutine(Cooldown(5, coolDownObjects[2]));
    }

    IEnumerator Cooldown(float cd, GameObject cdObj) //set cooldown number and fill for UI element
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


    private void Update() 
    {
        UpdateHUD();
        AnimationChanger();
        // UpdateCooldowns();
    }
}
