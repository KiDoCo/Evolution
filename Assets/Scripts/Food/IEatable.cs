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
    float CoolDownTime { get; set; }
    bool Eaten { get; set; }
    bool IsEatening { get; set; }
    float GetAmount();
    void SizeChanger();
    void Awake();
    void DecreaseFood();
    Collider GetCollider();
    AudioSource Source();
}
