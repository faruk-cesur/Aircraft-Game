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

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
            Storm();
        if (Input.GetKeyUp(KeyCode.T))
            Blizzard();
        if (Input.GetKeyUp(KeyCode.Y))
            Snow();
        if (Input.GetKeyUp(KeyCode.U))
            TurnOffFX();
    }

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