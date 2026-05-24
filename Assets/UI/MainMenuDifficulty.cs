using UnityEngine;
using UnityEngine.UI;

public class MainMenuDifficulty : MonoBehaviour
{
    [Header("Кнопки (Перетягни сюди Button component)")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("Іконки замків (Опціонально)")]
    public GameObject mediumLockObj; // Картинка замка на кнопці Medium
    public GameObject hardLockObj;   // Картинка замка на кнопці Hard

    private void Start()
    {
        UpdateButtonsState();
    }

    private void OnEnable()
    {
        UpdateButtonsState();
    }

    void UpdateButtonsState()
    {
        // Отримуємо дані про відкриті складності
        bool isMediumUnlocked = GlobalProgressionManager.Instance.saveData.mediumDifficultyUnlocked;
        bool isHardUnlocked = GlobalProgressionManager.Instance.saveData.hardDifficultyUnlocked;

        // --- ЛЕГКА --- (Завжди відкрита)
        if (easyButton) easyButton.interactable = true;

        // --- СЕРЕДНЯ ---
        if (mediumButton) mediumButton.interactable = isMediumUnlocked;
        if (mediumLockObj) mediumLockObj.SetActive(!isMediumUnlocked);

        // --- ВАЖКА ---
        if (hardButton) hardButton.interactable = isHardUnlocked;
        if (hardLockObj) hardLockObj.SetActive(!isHardUnlocked);
    }

    // --- МЕТОДИ ДЛЯ КНОПОК ---

    public void SetEasy()
    {
        SelectDifficulty(0);
        Debug.Log("Меню: Обрано легку складність");
    }

    public void SetMedium()
    {
        // Додатковий захист від кліків (якщо interactable зламається)
        if (!GlobalProgressionManager.Instance.saveData.mediumDifficultyUnlocked) return;

        SelectDifficulty(1);
        Debug.Log("Меню: Обрано середню складність");
    }

    public void SetHard()
    {
        if (!GlobalProgressionManager.Instance.saveData.hardDifficultyUnlocked) return;

        SelectDifficulty(2);
        Debug.Log("Меню: Обрано важку складність");
    }

    private void SelectDifficulty(int difficultyIndex)
    {
        PlayerPrefs.SetInt("SelectedDifficulty", difficultyIndex);
        PlayerPrefs.Save();
    }
}