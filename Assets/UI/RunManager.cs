using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerRunData
{
    public float currentHealth;
    public int currentLevel;
    public float currentXP;
    public float xpToNextLevel;
    public int potionCount;

    public float bonusHealth;
    public float bonusStamina;
    public float bonusDamage;
}
public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Run Settings")]
    public GameManager.Difficulty currentDifficulty = GameManager.Difficulty.Easy;
    public int currentFloor = 1;
    public int maxFloors = 5;
    public bool isRunActive = false;

    [Header("Timers")]
    public float currentLevelTimer = 0f;
    public bool isTimerRunning = false;
    public List<float> levelCompletionTimes = new List<float>();

    [Header("Statistics")]
    public int totalEnemiesInRun = 0;
    public int enemiesKilledInRun = 0;
    public bool isBossDead = false;

    [Header("Run Inventory")]
    public List<ArtifactSO> equippedArtifacts = new List<ArtifactSO>();

    [Header("Saved Player Data (Between Floors)")]
    public PlayerRunData savedPlayerData;

    public static List<ArtifactSO> transferArtifactsBuffer;

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

    private void Update()
    {
        if (isRunActive && isTimerRunning && !isBossDead)
        {
            currentLevelTimer += Time.deltaTime;
        }
    }



    public void StartRun(List<ArtifactSO> startingArtifacts, int difficultyIndex)
    {

        savedPlayerData = null;

        currentFloor = 1;
        levelCompletionTimes.Clear();
        equippedArtifacts.Clear();

        currentDifficulty = (GameManager.Difficulty)difficultyIndex;

        if (startingArtifacts != null)
        {
            equippedArtifacts.AddRange(startingArtifacts);
        }

        isRunActive = true;
    }

    public void ResetRunData() 
    {

        isBossDead = false;
        enemiesKilledInRun = 0;
        totalEnemiesInRun = 0;
        currentLevelTimer = 0f;
        isTimerRunning = true;
    }

    public void CompleteLevel()
    {
        isTimerRunning = false;
        levelCompletionTimes.Add(currentLevelTimer);
    }

    public void NextFloor()
    {
        currentFloor++;
    }


    public float GetDifficultyXPMultiplier()
    {
        switch (currentDifficulty)
        {
            case GameManager.Difficulty.Medium: return 1.25f;
            case GameManager.Difficulty.Hard: return 1.5f;
            default: return 1.0f;
        }
    }

    public float GetDifficultyHealthMultiplier()
    {
        float floorMultiplier = 1f + ((currentFloor - 1) * 0.2f);
        switch (currentDifficulty)
        {
            case GameManager.Difficulty.Medium: return 1.5f * floorMultiplier;
            case GameManager.Difficulty.Hard: return 2.0f * floorMultiplier;
            default: return 1.0f * floorMultiplier;
        }
    }
}