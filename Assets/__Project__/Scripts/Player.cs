using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Player : MonoBehaviour
{
    [BoxGroup("Player Settings")] public float WheelSpeed;
    [BoxGroup("Player Settings")] public float TakeOffSpeed;
    [BoxGroup("Player Settings")] public float FlyingSpeed;
    [BoxGroup("Player Settings")] public float MultiplySpeedPerSecond;
    [BoxGroup("Player Settings")] public float GravitySpeedPerSecond;

}