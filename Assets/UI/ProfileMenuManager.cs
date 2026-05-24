using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileMenuManager : MonoBehaviour
{
    [Header("Статистика Гравця")]
    public TextMeshProUGUI totalRunsText;
    public TextMeshProUGUI totalKillsText;
    public TextMeshProUGUI bossKillsText;

    public GameObject slotPrefab;
    public Transform gridContainer;
    private void OnEnable()
    {
        UpdateStats();
        RefreshUI();
    }

    void UpdateStats()
    {
        // Беремо дані з нашого глобального сейва
        var data = GlobalProgressionManager.Instance.saveData;

        totalRunsText.text = data.totalRunsPlayed.ToString();
        totalKillsText.text = data.totalEnemiesKilled.ToString();
        bossKillsText.text = data.bossesDefeated.ToString();
    }

    public void RefreshUI()
    {
        // Очистити старі
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Отримати всі ачівки з менеджера
        List<AchievementSO> allAchievements = GlobalProgressionManager.Instance.allAchievementsDatabase;

        foreach (var ach in allAchievements)
        {
            GameObject newSlot = Instantiate(slotPrefab, gridContainer);
            AchievementSlotUI ui = newSlot.GetComponent<AchievementSlotUI>();

            // Перевіряємо статус через ID ачівки
            bool unlocked = GlobalProgressionManager.Instance.IsAchievementCompleted(ach.id);

            ui.Setup(ach, unlocked);
        }
    }

    // Метод для кнопки "Закрити"
    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}