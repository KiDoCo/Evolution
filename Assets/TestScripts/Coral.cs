﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This class is used by players checkinteraction method
public class Coral : MonoBehaviour, IEatable
{
    //Food variables
    private float amountOfFood = 20;
    private int maxAmountFood = 20;
    private float foodPerSecond = 0.5f;
    private bool isEatening = false;
    private const float regenerationTimer = 2.0f;
    private bool eaten;
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

    public float GetAmount()
    {
        if (amountOfFood > 0)
        {
            return foodPerSecond;
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

    //Methods

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
        StartCoroutine(EatChecker());
        amountOfFood -= foodPerSecond;
    }

    private void IncreaseFood()
    {
        if (amountOfFood < MaxAmountFood && !Eaten)
        {
            IsEatening = true;
            amountOfFood += foodPerSecond / 2;
        }
    }

    public void SizeChanger()
    {
        transform.GetChild(0).transform.localScale = new Vector3(AmountFood / MaxAmountFood, AmountFood / MaxAmountFood, AmountFood / MaxAmountFood);
    }

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
    }

    public void Start()
    {
        Gamemanager.Instance.FoodPlaceDictionary.Add(this);
        EventManager.ActionAddHandler(EVENT.Increase, IncreaseFood);
    }

    public void Update()
    {
        SizeChanger();
    }
}
