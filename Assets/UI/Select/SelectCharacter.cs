using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.SceneManagement;

public class SelectCharacter : MonoBehaviour
{
    [Header("Налаштування сцени")]
    public Transform spawnPoint;

    [Header("Список персонажів")]
    public List<GameObject> characterPrefabs;

    [Header("UI Елементи")]
    public GameObject mainMenuPanel;
    public GameObject characterSelectPanel;
    public GameObject AchievmentsPanel;
    public GameObject SettingsPanel;

    [Header("Кнопка Продовжити")]
    public Button continueButton;
    public TextMeshProUGUI continueInfoText;

    private GameObject currentModel;
    private int selectedIndex = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        CheckContinueButton();
    }

    private void CheckContinueButton()
    {
        if (GlobalProgressionManager.Instance != null && GlobalProgressionManager.Instance.HasActiveRun())
        {
            continueButton.interactable = true;
            continueButton.gameObject.SetActive(true);
            var run = GlobalProgressionManager.Instance.saveData.activeRun;
            string diffName = ((GameManager.Difficulty)run.difficultyIndex).ToString();

            if (continueInfoText != null)
            {
                continueInfoText.text = $"ПРОДОВЖИТИ\nПоверх {run.floorNumber} | {diffName}";
            }
        }
        else
        {

            if (continueButton != null)
            {
                continueButton.interactable = false;
                if (continueInfoText != null) continueInfoText.text = "НЕМАЄ ЗБЕРЕЖЕНЬ";
            }
        }
    }
    public void ContinueRun()
    {
        if (!GlobalProgressionManager.Instance.HasActiveRun()) return;

        var save = GlobalProgressionManager.Instance.saveData.activeRun;
        var loadedArtifacts = GlobalProgressionManager.Instance.GetArtifactsFromIDs(save.equippedArtifactIDs);

        if (RunManager.Instance == null)
        {
            GameObject go = new GameObject("RunManager");
            go.AddComponent<RunManager>();
        }

        RunManager.Instance.StartRun(loadedArtifacts, save.difficultyIndex);

        RunManager.Instance.currentFloor = save.floorNumber;

        RunManager.Instance.savedPlayerData = save.playerData;

        Debug.Log($"Продовжуємо гру: Поверх {save.floorNumber}");
        SceneManager.LoadScene("Game");
    }

    public void OpenCharacterSelect()
    {
        Debug.Log("Захід в меню вибору");
        mainMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(true);

        ShowCharacter(0);
    }

    public void ShowCharacter(int index)
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        currentModel = Instantiate(characterPrefabs[index], spawnPoint.position, spawnPoint.rotation);
        selectedIndex = index;
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.equippedArtifacts != null)
            {
                GameManager.transferArtifactsBuffer = new List<ArtifactSO>(GameManager.Instance.equippedArtifacts);
            }
        }

        PlayerPrefs.SetInt("SelectedCharacter", selectedIndex);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewRun();
        }

        Debug.Log("Запуск нового забігу. Персонаж індекс: " + selectedIndex);
    }

    public void BackToMenu()
    {
        if (currentModel != null) Destroy(currentModel);
        characterSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        CheckContinueButton();
    }

    public void Exit()
    {
        Debug.Log("Вихід з гри");
        Application.Quit();
    }

    public void OpenAchievmentsTab()
    {
        AchievmentsPanel.SetActive(true);
    }
    public void CloseAchievmentsTab()
    {
        AchievmentsPanel.SetActive(false);
    }
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false );
        SettingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }
}