using System.Collections;
using UnityEngine;

/// <summary>
/// This class takes care of the functionality of a food source
/// </summary>
public class FoodBaseClass : MonoBehaviour, IEatable
{

    //Food variables
    private int         maxAmountFood     = 10;
    private float       amountOfFood      = 10;
    private float       foodPerSecond     = 4.0f;
    private const float regenerationTimer = 2.0f;
    private const float sizeMultiplier    = 1.5f;
    private const float DefaultSize       = 2.0f;
    private float       cooldownTime;      
    private bool        isEatening        = false;
    private bool        eaten;

    //collider variables
    private const float radiusMultiplier  = 1.5f;
    private float       vectoroffset      = 0.55f;
    private Collider    coralCollider;
    private BoxCollider box;
    private AudioSource source;
    private Vector3     originalPos;


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

    public float AmountFood
    {
        get
        {
            return amountOfFood;
        }
        set
        {
            amountOfFood = Mathf.Clamp(value,0,maxAmountFood);
        }
    }

    public float GetAmount()
    {
        if (amountOfFood > 0)
        {
            return (foodPerSecond / 4) * Time.deltaTime;
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
            return foodPerSecond;
        }
        set
        {
            foodPerSecond = value;
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
            cooldownTime = Mathf.Clamp(value, 0 , 5);
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

    //Methods

    /// <summary>
    /// Decreases food from source
    /// </summary>
    public void DecreaseFood()
    {
        StartCoroutine(EatChecker());
        AmountFood -= foodPerSecond * Time.deltaTime;
        CoolDownTime = 5.0f;
    }

    /// <summary>
    /// Increases food to source
    /// </summary>
    private void IncreaseFood()
    {
        if (amountOfFood < MaxAmountFood && !Eaten && CoolDownTime <= 0)
        {
            amountOfFood += (foodPerSecond / 4) * Time.deltaTime;
        }
    }

    /// <summary>
    /// Changes the size depending on the food amount
    /// </summary>
    public void SizeChanger()
    {
        if(AmountFood <= MaxAmountFood && amountOfFood > 0)
        {
            MathFunctions.BoxScaleObject(transform, transform.GetChild(0), originalPos, AmountFood / MaxAmountFood);
        }
    }

    /// <summary>
    /// Checks if this object is being eaten
    /// </summary>
    /// <returns></returns>
    IEnumerator EatChecker()
    {
        Eaten = true;
        yield return new WaitForSeconds(1.0f);
        Eaten = false;
    }

    //Unity methods
    public void Awake()
    {
        coralCollider = GetComponent<Collider>();
        box = coralCollider.GetComponent<BoxCollider>();
        source = GetComponent<AudioSource>();
        Gamemanager.Instance.FoodPlaceList.Add(this);
    }

    public void Start()
    {
        EventManager.ActionAddHandler(EVENT.Increase, IncreaseFood);
        originalPos = transform.GetChild(0).localPosition;
    }

    public void Update()
    {
        CoolDownTime -= Time.deltaTime;
        SizeChanger();
    }
}
