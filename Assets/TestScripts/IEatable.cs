using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEatable 
{
    int MaxAmountFood { get;  set; }
    float AmountFood { get;  set; }
    float FoodPerSecond { get; set; }
    void Awake();
    void Interact(Test test);
    void DecreaseFood();
    Collider GetCollider();
    object Iclass { get; set; }
}
