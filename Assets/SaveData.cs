using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Глобальна статистика (вже є у тебе)
    public int totalRunsPlayed;
    public int totalEnemiesKilled;
    public int bossesDefeated;
    public bool mediumDifficultyUnlocked;
    public bool hardDifficultyUnlocked;

    public List<string> completedAchievements = new List<string>();
    public List<string> unlockedArtifactIDs = new List<string>();

    // --- НОВЕ: Дані про незавершений забіг ---
    public ActiveRunData activeRun;
}

[System.Serializable]
public class ActiveRunData
{
    public int floorNumber;
    public int difficultyIndex;

    // Зберігаємо тільки ID артефактів (string), бо SO не серіалізуються в JSON
    public List<string> equippedArtifactIDs = new List<string>();

    // Стати гравця (HP, XP, Potions...)
    public PlayerRunData playerData;
}