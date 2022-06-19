using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.NiceVibrations;
using NaughtyAttributes;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum GameState
{
    BeforeGameplay,
    Gameplay,
    LevelCompleted,
    GameOver
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance ??= FindObjectOfType<GameManager>();

    [BoxGroup("SETTINGS"), SerializeField] private int _firstLevelAfterLoop;

    [BoxGroup("GAME STATE UI"), ReadOnly] public GameState CurrentGameState;
    [BoxGroup("GAME STATE UI"), SerializeField] private GameObject _beforeGameplayUI;
    [BoxGroup("GAME STATE UI"), SerializeField] private GameObject _gameplayUI;
    [BoxGroup("GAME STATE UI"), SerializeField] private GameObject _levelCompletedUI;
    [BoxGroup("GAME STATE UI"), SerializeField] private GameObject _gameOverUI;

    [BoxGroup("TEXT SETUP"), SerializeField] private TextMeshProUGUI _levelText;
    [BoxGroup("TEXT SETUP"), SerializeField] private TextMeshProUGUI _totalGoldText;
    [BoxGroup("TEXT SETUP"), SerializeField] private TextMeshProUGUI _earnedGoldText;

    [BoxGroup("SPRITES"), SerializeField] private List<GameObject> _goldUISpriteList;
    [BoxGroup("SPRITES"), SerializeField] private GameObject _goldImage;
    [BoxGroup("SPRITES"), SerializeField] private GameObject _goldSpriteParent;

    [HideInInspector] public int CurrentGold;
    [HideInInspector] public bool IsGameEnded;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60;
        SingleEventSystem();
        SetGoldSpriteList();
        DontDestroyOnLoad(this.gameObject);
        LoadReachedLevel();
    }

    public void LoadReachedLevel()
    {
        CurrentGameState = GameState.BeforeGameplay;
        MMVibrationManager.TransientHaptic(1, 0.1f, true, this);
        SceneManager.LoadScene(PlayerPrefs.GetInt("reachedLevel", 1));
        _levelText.text = "Level " + PlayerPrefs.GetInt("fakeLevelNumber", 1).ToString();
        _levelCompletedUI.SetActive(false);
        _gameOverUI.SetActive(false);
        _beforeGameplayUI.SetActive(true);
        _gameplayUI.SetActive(true);
        _levelText.transform.parent.gameObject.SetActive(true);
        CurrentGold = 0;
        PrintGoldText();
        IsGameEnded = false;
    }

    public void LevelCompleted()
    {
        CurrentGameState = GameState.LevelCompleted;
        MMVibrationManager.TransientHaptic(1, 0.1f, true, this);
        PlayerPrefs.SetInt("TotalGold", PlayerPrefs.GetInt("TotalGold", 0) + CurrentGold);
        PlayerPrefs.SetInt("fakeLevelNumber", PlayerPrefs.GetInt("fakeLevelNumber", 1) + 1);
        StartCoroutine(SetUIMenu(_levelCompletedUI, 1f, true));
        StartCoroutine(SetUIMenu(_gameplayUI, 1f, false));
        StartCoroutine(SetUIMenu(_beforeGameplayUI, 1f, false));
        StartCoroutine(SetUIMenu(_gameOverUI, 1f, false));

        if (SceneManager.sceneCountInBuildSettings > PlayerPrefs.GetInt("reachedLevel", 1) + 1)
        {
            PlayerPrefs.SetInt("reachedLevel", PlayerPrefs.GetInt("reachedLevel", 1) + 1);
        }
        else
        {
            if (_firstLevelAfterLoop <= 0)
            {
                _firstLevelAfterLoop = 1;
            }

            if (SceneManager.sceneCountInBuildSettings <= _firstLevelAfterLoop)
            {
                PlayerPrefs.SetInt("reachedLevel", 1);
            }
            else
            {
                PlayerPrefs.SetInt("reachedLevel", _firstLevelAfterLoop);
            }
        }
    }

    public void GameOver()
    {
        CurrentGameState = GameState.GameOver;
        MMVibrationManager.TransientHaptic(1, 0.1f, true, this);
        StartCoroutine(SetUIMenu(_gameOverUI, 2f, true));
        StartCoroutine(SetUIMenu(_gameplayUI, 2f, false));
        StartCoroutine(SetUIMenu(_beforeGameplayUI, 2f, false));
        StartCoroutine(SetUIMenu(_levelCompletedUI, 2f, false));
    }

    public void Gameplay()
    {
        IsGameEnded = false;
        CurrentGameState = GameState.Gameplay;
        _levelText.transform.parent.gameObject.SetActive(false);
        _gameplayUI.SetActive(true);
        _beforeGameplayUI.SetActive(false);
        _levelCompletedUI.SetActive(false);
        _gameOverUI.SetActive(false);
    }

    private void PrintGoldText()
    {
        _earnedGoldText.text = "+" + CurrentGold.ToString();

        if (CurrentGameState == GameState.LevelCompleted || CurrentGameState == GameState.BeforeGameplay)
        {
            _totalGoldText.text = (PlayerPrefs.GetInt("TotalGold")).ToString();
        }
        else
        {
            _totalGoldText.text = (CurrentGold + PlayerPrefs.GetInt("TotalGold")).ToString();
        }
    }

    public void CollectGoldAnimation()
    {
        foreach (var gold in _goldUISpriteList)
        {
            if (!gold.activeSelf)
            {
                gold.SetActive(true);
                gold.transform.SetParent(_goldSpriteParent.transform);
                gold.transform.DOScale(_goldImage.transform.localScale * 2f, 0.5f);
                gold.transform.DOLocalMove(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    gold.transform.DOScale(Vector3.one, 0.5f);
                    gold.transform.SetParent(_goldImage.transform);
                    gold.transform.DOLocalMove(Vector3.zero, 0.5f).OnComplete(() =>
                    {
                        CurrentGold++;
                        PrintGoldText();
                        gold.transform.SetParent(_goldSpriteParent.transform);
                        gold.transform.localPosition = Vector3.zero;
                        gold.SetActive(false);
                    });
                });
                break;
            }
        }
    }

    private void SetGoldSpriteList()
    {
        if (_goldUISpriteList.Count != _goldSpriteParent.transform.childCount)
        {
            _goldUISpriteList.Clear();

            for (int i = 0; i < _goldSpriteParent.transform.childCount; i++)
            {
                _goldUISpriteList.Add(_goldSpriteParent.transform.GetChild(i).gameObject);
            }
        }
    }

    private void SingleEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(gameObject.transform);
        }
    }

    private IEnumerator SetUIMenu(GameObject menu, float time, bool trueOrFalse)
    {
        yield return new WaitForSeconds(time);
        menu.SetActive(trueOrFalse);
    }
}