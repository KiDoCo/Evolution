using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface which is implemented to food sources
/// </summary>
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
    void Interact(move test);
    void DecreaseFood();
    Collider GetCollider();
    AudioSource Source();
}
