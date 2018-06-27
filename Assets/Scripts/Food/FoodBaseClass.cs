using System.Collections;
using UnityEngine;

/// <summary>
/// This class takes care of the functionality of a food source
/// </summary>
public class FoodBaseClass : MonoBehaviour, IEatable
{

    //Food variables
    private int         maxAmountFood     = 20;
    private float       amountOfFood      = 20;
    private float       foodPerSecond     = 0.5f;
    private const float regenerationTimer = 2.0f;
    private const float sizeMultiplier    = 1.5f;
    private bool        isEatening        = false;
    private bool        eaten;

    //collider variables
    private const float radiusMultiplier  = 1.5f;
    private Collider    coralCollider;
    private BoxCollider box;
    private AudioSource source;


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
            amountOfFood = value;
        }
    }

    public float GetAmount()
    {
        if (amountOfFood > 0)
        {
            return foodPerSecond * Time.deltaTime;
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
    /// Used for triggering eat method from player character
    /// </summary>
    /// <param name="test"></param>
    public void Interact(move test)
    {
        if (amountOfFood > 0)
        {
            test.CmdEat(this);
        }
    }

    /// <summary>
    /// Decreases food from source
    /// </summary>
    public void DecreaseFood()
    {
        Debug.Log(Eaten);
        StartCoroutine(EatChecker());
        amountOfFood -= foodPerSecond * Time.deltaTime;
    }

    /// <summary>
    /// Increases food to source
    /// </summary>
    private void IncreaseFood()
    {
        if (amountOfFood < MaxAmountFood && !Eaten)
        {
            amountOfFood += (foodPerSecond / 2 )* Time.deltaTime;
        }
    }

    /// <summary>
    /// Changes the size depending on the food amount
    /// </summary>
    public void SizeChanger()
    {
        if(AmountFood <= MaxAmountFood)
        {
        transform.GetChild(0).transform.localScale = new Vector3(AmountFood / MaxAmountFood, AmountFood / MaxAmountFood, AmountFood / MaxAmountFood);
        box.size = new Vector3(2.0f, amountOfFood * sizeMultiplier, 2.0f);
        box.center = new Vector3(0.0f, (amountOfFood * sizeMultiplier) / 2, 0.0f);

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

    //unity methods
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
    }

    public void Update()
    {
        SizeChanger();
    }

}
