using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FXDemoController : MonoBehaviour
{
    public GameObject StormFX;
    public GameObject BlizzardFX;
    public GameObject SnowFX;
    public GameObject AudioFX;

    public void Storm()
    {
        TurnOffFX();
        StormFX.SetActive(true);
        AudioFX.SetActive(true);
    }

    public void Blizzard()
    {
        TurnOffFX();
        BlizzardFX.SetActive(true);
        AudioFX.SetActive(true);
    }

    public void Snow()
    {
        TurnOffFX();
        SnowFX.SetActive(true);
        AudioFX.SetActive(true);
    }

    public void TurnOffFX()
    {
        StormFX.SetActive(false);
        BlizzardFX.SetActive(false);
        SnowFX.SetActive(false);
        AudioFX.SetActive(false);
    }
}