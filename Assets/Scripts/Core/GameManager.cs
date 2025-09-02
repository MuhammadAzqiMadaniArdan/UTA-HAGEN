using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public GameState currentState = GameState.Playing;
    public float greedLevel = 0f;
    public float suspicionLevel = 0f;
    public int currentStoryProgress = 0;

    [Header("Game Settings")]
    public float maxGreedLevel = 100f;
    public float maxSuspicionLevel = 100f;
    public List<EndingData> availableEndings = new List<EndingData>();

    [Header("Corruption System")]
    public float totalCorruption = 0f; // total uang yang sudah dikorupsi
    public event System.Action<float> OnCorruptionChanged;

    public static GameManager Instance { get; private set; }

    // Events
    public System.Action<float> OnGreedLevelChanged;
    public System.Action<float> OnSuspicionLevelChanged;
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<EndingType> OnGameEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        greedLevel = 0f;
        suspicionLevel = 0f;
        currentStoryProgress = 0;
        ChangeGameState(GameState.Playing);
    }

    public void AddGreed(float amount)
    {
        greedLevel = Mathf.Clamp(greedLevel + amount, 0f, maxGreedLevel);
        OnGreedLevelChanged?.Invoke(greedLevel);

        CheckGameEndConditions();
    }

    public void AddSuspicion(float amount)
    {
        suspicionLevel = Mathf.Clamp(suspicionLevel + amount, 0f, maxSuspicionLevel);
        OnSuspicionLevelChanged?.Invoke(suspicionLevel);

        CheckGameEndConditions();
    }

    public void AddCorruption(float amount)
    {
        totalCorruption += amount;
        OnCorruptionChanged?.Invoke(totalCorruption);
    }
    public void AdvanceStory()
    {
        currentStoryProgress++;
        CheckGameEndConditions();
    }

    private void CheckGameEndConditions()
    {
        EndingType endingType = DetermineEnding();
        if (endingType != EndingType.None)
        {
            EndGame(endingType);
        }
    }

    private EndingType DetermineEnding()
    {
        // Logic untuk menentukan ending berdasarkan greed level, suspicion level, dan story progress
        if (suspicionLevel >= maxSuspicionLevel)
            return EndingType.Caught;

        if (greedLevel >= maxGreedLevel)
            return EndingType.TooGreedy;

        if (currentStoryProgress >= 10) // Contoh: 10 chapter
        {
            if (greedLevel < 30f && suspicionLevel < 30f)
                return EndingType.GoodEnding;
            else if (greedLevel < 60f && suspicionLevel < 60f)
                return EndingType.NeutralEnding;
            else
                return EndingType.BadEnding;
        }

        return EndingType.None;
    }

    private void EndGame(EndingType endingType)
    {
        ChangeGameState(GameState.GameOver);
        OnGameEnded?.Invoke(endingType);

        // Unlock ending baru untuk next playthrough
        UnlockEnding(endingType);
    }

    private void UnlockEnding(EndingType endingType)
    {
        // Save unlocked ending to persistent data
        PlayerPrefs.SetInt($"Ending_{endingType}", 1);
        PlayerPrefs.Save();
    }

    public void ChangeGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }

    public void RestartGame()
    {
        InitializeGame();
    }

    public float GetGreed()
    {
        return greedLevel;
    }

    public float GetSuspicion()
    {
        return suspicionLevel;
    }

    public int GetStoryProgress()
    {
        return currentStoryProgress;
    }

    public GameState GetCurrentGameState()
    {
        return currentState;
    }
}

public enum GameState
{
    Playing,
    Paused,
    Dialog,
    Event,
    GameOver
}

public enum EndingType
{
    None,
    GoodEnding,
    NeutralEnding,
    BadEnding,
    Caught,
    TooGreedy
}

[System.Serializable]
public class EndingData
{
    public EndingType endingType;
    public string endingName;
    public string endingDescription;
    public bool isUnlocked;
}
