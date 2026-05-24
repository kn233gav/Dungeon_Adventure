using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panelObject;
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI flavorText;
    public TextMeshProUGUI statsText;

    // --- НОВЕ ПОЛЕ ---
    public TextMeshProUGUI unlocksText; // Створи це в Unity і прив'яжи!
    // -----------------

    [Header("Buttons")]
    public Button nextLevelButton;
    public Button restartButton;
    public Button menuButton;

    private void Start()
    {
        if (panelObject != null) panelObject.SetActive(false);

        if (restartButton) restartButton.onClick.AddListener(() => GameManager.Instance.RestartGameFull());
        if (menuButton) menuButton.onClick.AddListener(() => GameManager.Instance.GoToMenu());
        if (nextLevelButton) nextLevelButton.onClick.AddListener(() => GameManager.Instance.LoadNextLevel());
    }

    // Додав параметр `unlocksInfo`
    public void ShowWinScreen(string rank, string flavor, int percentage, int killed, int total, bool isFinalLevel, string unlocksInfo)
    {
        if (panelObject != null) panelObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        rankText.text = rank;
        flavorText.text = flavor;
        statsText.text = $"Знищено: {killed} / {total}\n({percentage}%)";

        // Відображаємо, що ми відкрили
        if (unlocksText != null) unlocksText.text = unlocksInfo;

        // Колір рангу
        switch (rank)
        {
            case "S": rankText.color = new Color(1f, 0.84f, 0f); break;
            case "A": rankText.color = Color.red; break;
            case "F": rankText.color = Color.gray; break;
            default: rankText.color = Color.white; break;
        }

        if (isFinalLevel)
        {
            if (nextLevelButton) nextLevelButton.gameObject.SetActive(false);
            if (restartButton) restartButton.gameObject.SetActive(true);
        }
        else
        {
            if (nextLevelButton) nextLevelButton.gameObject.SetActive(true);
            if (restartButton) restartButton.gameObject.SetActive(false);
        }
    }
}