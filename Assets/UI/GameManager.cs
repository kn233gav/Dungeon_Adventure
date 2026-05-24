using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public HUDManager hudManager;
    public LevelUpUI levelUpUIInstance;
    public EndGameUI endGameUI;

    // Це посилання на дані персонажів
    public List<CharacterStatsSO> allCharactersData;

    // --- REDIRECTS TO RUN MANAGER ---
    public int currentFloor => RunManager.Instance.currentFloor;
    public int maxFloors => RunManager.Instance.maxFloors;
    public bool isRunActive => RunManager.Instance.isRunActive;
    public Difficulty currentDifficulty => RunManager.Instance.currentDifficulty;
    public List<ArtifactSO> equippedArtifacts => RunManager.Instance.equippedArtifacts;

    public static List<ArtifactSO> transferArtifactsBuffer
    {
        get => RunManager.transferArtifactsBuffer;
        set => RunManager.transferArtifactsBuffer = value;
    }

    public enum Difficulty { Easy, Medium, Hard }

    private string levelUnlocksLog = "";

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
        if (RunManager.Instance == null)
        {
            GameObject runMgrObj = new GameObject("RunManager");
            runMgrObj.AddComponent<RunManager>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Підписуємось на подію смерті, якщо PlayerStatsManager є на сцені
        PlayerStatsManager player = FindFirstObjectByType<PlayerStatsManager>();
        if (player != null) player.OnDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerStatsManager player = FindFirstObjectByType<PlayerStatsManager>();
        if (player != null) player.OnDeath -= OnPlayerDeath;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferencesInNewScene();

        // Перепідписуємось на події гравця в новій сцені
        PlayerStatsManager player = FindFirstObjectByType<PlayerStatsManager>();
        if (player != null) player.OnDeath += OnPlayerDeath;
    }

    public void RefreshReferencesInNewScene()
    {
        hudManager = FindFirstObjectByType<HUDManager>();
        endGameUI = FindFirstObjectByType<EndGameUI>(FindObjectsInactive.Include);
        levelUpUIInstance = FindFirstObjectByType<LevelUpUI>(FindObjectsInactive.Include);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (RunManager.Instance.isRunActive && RunManager.Instance.isTimerRunning && !RunManager.Instance.isBossDead)
        {
            if (hudManager != null) hudManager.UpdateTimerUI(RunManager.Instance.currentLevelTimer);
        }
    }

    // --- СТАРТ ТА ПЕРЕХІД ---

    public void StartNewRun()
    {
        // При старті нового забігу видаляємо старий сейв, якщо він був
        GlobalProgressionManager.Instance.DeleteActiveRun();

        RunManager.Instance.savedPlayerData = null;

        int diffIndex = PlayerPrefs.GetInt("SelectedDifficulty", 0);
        RunManager.Instance.StartRun(transferArtifactsBuffer, diffIndex);

        GlobalProgressionManager.Instance.saveData.totalRunsPlayed++;
        GlobalProgressionManager.Instance.SaveProgress();

        LoadLevelScene();
    }

    public void LoadNextLevel()
    {
        // 1. Отримуємо дані гравця перед виходом
        PlayerStatsManager playerStats = FindFirstObjectByType<PlayerStatsManager>();
        if (playerStats != null)
        {
            RunManager.Instance.savedPlayerData = playerStats.GetSaveData();
        }

        // 2. Піднімаємо поверх
        RunManager.Instance.NextFloor();

        // 3. ЗБЕРІГАЄМО ЗАБІГ (Чекпоінт)
        // Тепер, якщо гравець вийде, він почне з початку ЦЬОГО НОВОГО поверху
        if (GlobalProgressionManager.Instance != null && RunManager.Instance.savedPlayerData != null)
        {
            GlobalProgressionManager.Instance.SaveActiveRun(
                RunManager.Instance.currentFloor,
                (int)RunManager.Instance.currentDifficulty,
                RunManager.Instance.savedPlayerData,
                RunManager.Instance.equippedArtifacts
            );
        }

        LoadLevelScene();
    }

    private void LoadLevelScene()
    {
        Time.timeScale = 1f;
        RunManager.Instance.ResetRunData();
        levelUnlocksLog = "";
        SceneManager.LoadScene("Game");
    }

    // --- ОБРОБКА СМЕРТІ ---
    private void OnPlayerDeath()
    {
        // Якщо гравець помер - видаляємо сейв, це ж Roguelike :)
        GlobalProgressionManager.Instance.DeleteActiveRun();
        Debug.Log("Гравець помер. Сейв видалено.");

        // Тут виклик UI програшу (якщо є)
        // if (endGameUI != null) endGameUI.ShowLoseScreen();
    }

    // --- SPAWN LOGIC ---
    public void SpawnPlayerAt(Transform point)
    {
        RefreshReferencesInNewScene();
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        // Якщо завантажили гру, то індекс персонажа міг би бути збережений в ActiveRunData,
        // але поки беремо з PlayerPrefs (або дефолтний).
        if (selectedIndex >= allCharactersData.Count) selectedIndex = 0;

        CharacterStatsSO selectedCharData = allCharactersData[selectedIndex];
        GameObject playerObj = Instantiate(selectedCharData.characterPrefab, point.position, point.rotation);

        PlayerController pc = playerObj.GetComponent<PlayerController>();
        if (pc != null)
        {
            // Логіка ініціалізації
            if (RunManager.Instance.currentFloor > 1 && RunManager.Instance.savedPlayerData != null)
            {
                // Завантажуємо збережені стати (HP, Level, XP)
                pc.stats.LoadFromSaveData(selectedCharData, RunManager.Instance.savedPlayerData);
            }
            else
            {
                // Новий старт
                pc.Initialize(selectedCharData);
            }

            // Link UI
            if (levelUpUIInstance != null)
            {
                pc.levelUpScreen = levelUpUIInstance;
                levelUpUIInstance.Initialize(pc);
                levelUpUIInstance.gameObject.SetActive(false);
            }
            if (hudManager != null)
            {
                pc.healthBar = hudManager.healthBar;
                pc.staminaBar = hudManager.staminaBar;
                pc.levelText = hudManager.levelText;
                pc.xpSlider = hudManager.xpSlider;
                pc.UpdateUI();
                hudManager.UpdateFloorText(currentFloor, maxFloors);
            }

            // Важливо: підписуємось на смерть
            pc.stats.OnDeath += OnPlayerDeath;
        }
    }

    // --- GAMEPLAY EVENTS ---
    public void SetTotalEnemies(int amount)
    {
        RunManager.Instance.totalEnemiesInRun = amount;
        RunManager.Instance.enemiesKilledInRun = 0;
    }

    public void RegisterEnemyKill()
    {
        RunManager.Instance.enemiesKilledInRun++;
    }

    public void OnBossDefeated()
    {
        RunManager.Instance.isBossDead = true;
        if (hudManager != null && hudManager.bossHPbar != null)
            hudManager.bossHPbar.gameObject.SetActive(false);

        GlobalProgressionManager.Instance.saveData.bossesDefeated++;
        TryDropBossArtifact();
        GlobalProgressionManager.Instance.SaveProgress();
    }

    private void TryDropBossArtifact()
    {
        // (Твій код без змін)
        List<ArtifactSO> potentialDrops = GlobalProgressionManager.Instance.allArtifactsDatabase
            .FindAll(a => a.unlockType == ArtifactUnlockType.BossDrop && !GlobalProgressionManager.Instance.IsArtifactUnlocked(a.artifactID));
        if (potentialDrops.Count == 0) return;
        ArtifactSO candidate = potentialDrops[Random.Range(0, potentialDrops.Count)];
        if (Random.Range(0f, 100f) <= candidate.dropChance)
        {
            GlobalProgressionManager.Instance.UnlockArtifact(candidate.artifactID);
            levelUnlocksLog += $"\n<color=red>BOSS DROP:</color> {candidate.artifactName}";
        }
    }

    public void FinishLevelLogic()
    {
        RunManager.Instance.CompleteLevel();

        float percentage = 0f;
        int total = RunManager.Instance.totalEnemiesInRun;
        int killed = RunManager.Instance.enemiesKilledInRun;
        if (total > 0) percentage = ((float)killed / total) * 100f;

        string rank = CalculateRank(percentage);
        GlobalProgressionManager.Instance.saveData.totalEnemiesKilled += killed;
        CheckAchievements(rank);

        bool isFinalLevel = (currentFloor >= maxFloors);
        string flavor = "";

        if (isFinalLevel)
        {
            CheckDifficultyUnlock();
            float totalTime = 0f;
            foreach (var t in RunManager.Instance.levelCompletionTimes) totalTime += t;
            flavor = $"ПЕРЕМОГА!\nЗагальний час: {FormatTime(totalTime)}\n\n";

            // ГРУ ПРОЙДЕНО -> Видаляємо активний сейв
            GlobalProgressionManager.Instance.DeleteActiveRun();
        }
        else
        {
            flavor = $"Поверх {currentFloor} зачищено!\nЧас: {FormatTime(RunManager.Instance.currentLevelTimer)}";
        }

        GlobalProgressionManager.Instance.SaveProgress();

        if (endGameUI != null)
        {
            endGameUI.ShowWinScreen(rank, flavor, Mathf.RoundToInt(percentage), killed, total, isFinalLevel, levelUnlocksLog);
        }
    }

    // --- HELPERS (Без змін) ---
    public float GetTotalXPMultiplier()
    {
        float difficultyMult = RunManager.Instance.GetDifficultyXPMultiplier();
        float floorBonus = 1f + ((currentFloor - 1) * 0.15f);
        float artifactMult = CalculateStat(1f, StatType.ExperienceGain);
        return difficultyMult * artifactMult * floorBonus;
    }

    public float GetHealthMultiplier() => RunManager.Instance.GetDifficultyHealthMultiplier();

    public float CalculateStat(float baseValue, StatType type)
    {
        float percentAdd = 0f;
        float flatAdd = 0f;
        foreach (var art in RunManager.Instance.equippedArtifacts)
        {
            if (art == null) continue;
            foreach (var mod in art.modifiers)
            {
                if (mod.statType == type)
                {
                    if (mod.mode == ModifierMode.PercentAdd) percentAdd += mod.value;
                    else if (mod.mode == ModifierMode.FlatAdd) flatAdd += mod.value;
                }
            }
        }
        return (baseValue * (1f + percentAdd)) + flatAdd;
    }

    public void ShowBossHPBar(GameObject boss)
    {
        if (hudManager != null && hudManager.bossHPbar != null)
        {
            hudManager.bossHPbar.gameObject.SetActive(true);
            var uiScript = hudManager.bossHPbar.GetComponent<BossHealthUI>();
            var bossCtrl = boss.GetComponent<BossGolem>();
            if (uiScript != null && bossCtrl != null) uiScript.Init(bossCtrl);
        }
    }

    // --- UI КНОПКИ ---
    public void RestartGameFull()
    {
        StartNewRun();
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        RunManager.Instance.isRunActive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Main Menu");
    }

    private string CalculateRank(float percentage)
    {
        if (percentage >= 100f) return "S";
        if (percentage >= 90f) return "A";
        if (percentage >= 80f) return "B";
        if (percentage >= 70f) return "C";
        if (percentage >= 60f) return "D";
        if (percentage >= 50f) return "E";
        return "F";
    }

    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void CheckAchievements(string rank)
    {
        List<string> currentRunConditions = new List<string>();
        if (rank == "S") currentRunConditions.Add("rank_s");
        if (rank == "A") currentRunConditions.Add("rank_a");
        if (rank == "S" && currentFloor >= 5) currentRunConditions.Add("rank_s_run");

        if (currentFloor >= maxFloors)
        {
            currentRunConditions.Add("finish_run");
            if (currentDifficulty == Difficulty.Easy) currentRunConditions.Add("beat_easy");
            if (currentDifficulty == Difficulty.Medium) currentRunConditions.Add("beat_medium");
            if (currentDifficulty == Difficulty.Hard) currentRunConditions.Add("beat_hard");
        }

        string unlocksLogStr = GlobalProgressionManager.Instance.CheckAchievements(currentRunConditions);
        levelUnlocksLog += unlocksLogStr;
    }

    private void CheckDifficultyUnlock()
    {
        if (currentDifficulty == Difficulty.Easy && !GlobalProgressionManager.Instance.saveData.mediumDifficultyUnlocked)
        {
            GlobalProgressionManager.Instance.UnlockDifficulty(1);
            levelUnlocksLog += "\n<color=green>NEW MODE:</color> Medium Difficulty!";
        }
        else if (currentDifficulty == Difficulty.Medium && !GlobalProgressionManager.Instance.saveData.hardDifficultyUnlocked)
        {
            GlobalProgressionManager.Instance.UnlockDifficulty(2);
            levelUnlocksLog += "\n<color=red>NEW MODE:</color> Hard Difficulty!";
        }
    }
}