using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This class takes care of the functionality of a food source
/// </summary>
public class FoodBaseClass : NetworkBehaviour, IEatable
{

    //Food variables
    private int maxAmountFood = 10;
    [SyncVar(hook = "CmdOnDecreaseFood")]
    private float amountOfFood = 10;
    private float foodPerSecond = 4.0f;
    private const float regenerationTimer = 2.0f;
    private const float sizeMultiplier = 1.5f;
    private const float DefaultSize = 2.0f;
    private float cooldownTime;
    private bool isEatening = false;
    private bool eaten;

    //collider variables
    private const float radiusMultiplier = 1.5f;
    private Collider coralCollider;
    private AudioSource source;
    private Vector3 originalPos;

    #region Getters&setters
    //interface properties
    public int MaxAmountFood
    {
        get
        {
            return maxAmountFood;
        }

        set
        {
            maxAmountFood = value;
        }
    }

    public float AmountOfFood
    {
        get
        {
            return amountOfFood;
        }
        set
        {
            amountOfFood = Mathf.Clamp(value, 0, maxAmountFood);
        }
    }

    public float GetAmount()
    {
        if (amountOfFood > 0)
        {
            return (FoodPerSecond / 4) * Time.deltaTime;
        }
        else
        {
            return 0;
        }
    }

    float IEatable.FoodPerSecond
    {
        get
        {
            return FoodPerSecond;
        }
        set
        {
            FoodPerSecond = value;
        }
    }

    public bool IsEatening
    {
        get
        {
            return isEatening;
        }

        set
        {
            isEatening = value;
        }
    }

    public bool Eaten
    {
        get
        {
            return eaten;
        }

        set
        {
            eaten = value;
        }
    }

    public float CoolDownTime
    {
        get
        {
            return cooldownTime;
        }

        set
        {
            cooldownTime = Mathf.Clamp(value, 0, 5);
        }
    }

    public object GetInstance
    {
        get
        {
            return this;
        }

        set
        {
        }
    }

    public float FoodPerSecond
    {
        get
        {
            return foodPerSecond;
        }

        set
        {
            foodPerSecond = value;
        }
    }

    public Collider GetCollider()
    {
        return coralCollider;
    }

    public AudioSource Source()
    {
        return source;
    }

    #endregion

    public void DecreaseFood(float food)
    {
        AmountOfFood -= food;
    }

    public void CmdOnDecreaseFood(float food)
    {
        Debug.Log("ASDF");
        AmountOfFood = food;
    }


    /// <summary>
    /// Increases food to source
    /// </summary>
    private void IncreaseFood()
    {
        if (amountOfFood < MaxAmountFood && !Eaten && CoolDownTime <= 0)
        {
            amountOfFood += (FoodPerSecond / 4) * Time.deltaTime;
        }
    }

    /// <summary>
    /// Changes the size depending on the food amount
    /// </summary>
    public void SizeChanger()
    {
        if (AmountOfFood <= MaxAmountFood && amountOfFood > 0)
        {
            BoxScaleObject(transform, transform.GetChild(0), originalPos, AmountOfFood / MaxAmountFood);
        }

    }
    /// <summary>
    /// Calculates the scaling of an box object which is a child 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="transform"></param>
    /// <param name="position"></param>
    /// <param name="amount"></param>
    public void BoxScaleObject(Transform parent, Transform transform, Vector3 position, float amount)
    {
        Vector3 scale = new Vector3(amount, amount, amount);
        transform.localScale = Vector3.Lerp(scale, transform.localScale, Time.deltaTime / 1000);
        transform.position = parent.position + (position * scale.y);
    }

    /// <summary>
    /// Checks if this object is being eaten
    /// </summary>
    /// <returns></returns>
    public IEnumerator EatChecker()
    {
        Eaten = true;
        yield return new WaitForSeconds(1.0f);
        Eaten = false;
    }

    //Unity methods
    public void Awake()
    {
        coralCollider = GetComponent<Collider>();
        source = GetComponent<AudioSource>();
        Gamemanager.Instance.FoodPlaceList.Add(gameObject);
    }

    public void Start()
    {
        EventManager.ActionAddHandler(EVENT.Increase, IncreaseFood);
        originalPos = transform.GetChild(0).localPosition;
    }

    public void Update()
    {
        SizeChanger();

        CoolDownTime -= Time.deltaTime;
    }
}
