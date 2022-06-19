using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [BoxGroup("Player Settings")] public float TakeOffSpeed;
    [BoxGroup("Player Settings")] public float MultiplySpeedPerSecond;
    [BoxGroup("Player Settings")] public float ScoreDecreaseSpeed = 0.1f;

    [HideInInspector] public float WheelSpeed;
    [HideInInspector] public float FlyingSpeed;
    [HideInInspector] public float GravitySpeedPerSecond;
    [HideInInspector] public int CheckpointScore;

    [BoxGroup("SETUP")] public ParticleSystem CrashParticle;
    [BoxGroup("SETUP")] public ParticleSystem CheckpointParticle;
    [BoxGroup("SETUP")] public TextMeshProUGUI ScoreText;
    [BoxGroup("SETUP")] public TextMeshProUGUI WarningText;
    [BoxGroup("SETUP")] public GameObject PlayerUI;
    [BoxGroup("SETUP")] public List<GameObject> CheckpointList;
    [BoxGroup("SETUP"), SerializeField] private GameObject _aircraftModel;
    [BoxGroup("SETUP"), SerializeField] private TextMeshProUGUI _speedText;
    [BoxGroup("SETUP"), SerializeField] private PlayerController _playerController;
    [BoxGroup("SETUP"), SerializeField] private FXDemoController _fxDemoController;
    private int _warningTimer;

    private void Start()
    {
        _warningTimer = 10;
        StartCoroutine(DecreaseScorePerSecond());
        StartCoroutine(ControlWarningText());
        _fxDemoController.Snow();
    }

    private void Update()
    {
        WriteTexts();
        ControlCheckpoints();
    }

    private void WriteTexts()
    {
        switch (_playerController.ControllerType)
        {
            case PlayerController.ControllerTypes.OnWheels:
                _speedText.text = "Speed: " + (int)(WheelSpeed / 50);
                ScoreText.text = "Score: " + CheckpointScore;
                break;
            case PlayerController.ControllerTypes.Flying:
                _speedText.text = "Speed: " + (int)(FlyingSpeed / 50);
                ScoreText.text = "Score: " + CheckpointScore;
                break;
            case PlayerController.ControllerTypes.Autopilot:
                _speedText.text = "Speed: " + (int)(FlyingSpeed / 50);
                ScoreText.text = "Score: " + CheckpointScore;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator DecreaseScorePerSecond()
    {
        while (true)
        {
            if (CheckpointScore <= 0)
            {
                yield return null;
            }
            else
            {
                CheckpointScore--;
                yield return new WaitForSeconds(ScoreDecreaseSpeed);
            }
        }
    }

    private void ControlCheckpoints()
    {
        if (CheckpointList.Count <= 0 && !GameManager.Instance.IsGameEnded)
        {
            GameManager.Instance.IsGameEnded = true;
            _playerController.ControllerType = PlayerController.ControllerTypes.Autopilot;
            _playerController.GameEnded();
            PlayerUI.SetActive(false);
            transform.DOLookAt(new Vector3(-2.8f, 0, 100), 3f);
            transform.DOMove(new Vector3(-2.8f, 0, 100), 5f).OnComplete(() => GameManager.Instance.LevelCompleted());
        }
    }

    private IEnumerator ControlWarningText()
    {
        while (true)
        {
            if (_warningTimer <= 0 && !GameManager.Instance.IsGameEnded)
            {
                GameManager.Instance.IsGameEnded = true;
                gameObject.tag = "Untagged";
                _aircraftModel.gameObject.SetActive(false);
                _playerController.ControllerType = PlayerController.ControllerTypes.Autopilot;
                _playerController.GameEnded();
                CrashParticle.Play();
                CrashParticle.GetComponent<AudioSource>().Play();
                PlayerUI.SetActive(false);
                GameManager.Instance.GameOver();
                yield break;
            }

            if (CheckpointList.Count > 0 && (CheckpointList[0].transform.position - transform.position).sqrMagnitude > 200000)
            {
                WarningText.gameObject.SetActive(true);
                _warningTimer--;
                WarningText.text = "Mission will fail in " + _warningTimer;
            }
            else
            {
                _warningTimer = 10;
                WarningText.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1f);
        }
    }
}