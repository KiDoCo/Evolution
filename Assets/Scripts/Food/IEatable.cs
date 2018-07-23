using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface which is implemented to food sources
/// </summary>
public interface IEatable 
{
    int MaxAmountFood { get;  set; }
    float AmountOfFood { get;  set; }
    float FoodPerSecond { get; set; }
    float CoolDownTime { get; set; }
    bool Eaten { get; set; }
    bool IsEatening { get; set; }
    float GetAmount();
    void Awake();
    Collider GetCollider();
    AudioSource Source();
    object GetInstance { get; set; } 
}
