using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//interface used by character interaction
public interface IEatable 
{
    int MaxAmountFood { get;  set; }
    float AmountFood { get;  set; }
    float FoodPerSecond { get; set; }
    float GetAmount();
    bool Eaten { get; set; }
    bool IsEatening { get; set; }
    void SizeChanger();
    void Awake();
    void Interact(Test test);
    void DecreaseFood();
    Collider GetCollider();
}
