using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This class is used by players checkinteraction method
public class Coral : MonoBehaviour, IEatable
{
    //Food variables
    private float amountOfFood = 20;
    private int maxAmountFood = 20;
    private float foodPerSecond = 0.5f;

    //collider variables
    private const float radiusMultiplier = 1.5f;
    private Collider coralCollider;

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

    public object Iclass
    {
        get
        {
            return this;
        }

        set
        {
            
        }
    }


    /// <summary>
    /// Used for triggering eat method from player character
    /// </summary>
    /// <param name="test"></param>
    public void Interact(Test test)
    {
        if (amountOfFood > 0)
        {
            test.CmdEat(this);
        }
    }

    public void DecreaseFood()
    {
        amountOfFood -= foodPerSecond;
    }

    public Collider GetCollider()
    {
        return coralCollider;
    }


    //unity methods
    public void Awake()
    {
        coralCollider = GetComponent<Collider>();

    }

    public void Start()
    {
        Gamemanager.Instance.FoodPlaceDictionary.Add(this);
    }
}
