using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Меню Паузи")]
    public GameObject pauseMenuPanel; // Сама панелька (Panel)
    public Button resumeButton;       // Кнопка "Продовжити"
    public Button restartButton;      // Кнопка "Рестарт"
    public Button quitButton;         // Кнопка "Вихід"

    [Header("Елементи Інтерфейсу")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;
    public Slider healthBar;
    public Slider staminaBar;
    public TextMeshProUGUI potionCountText;
    public TextMeshProUGUI stateDebug;
        
    [Header("Boss")]
    public Slider bossHPbar;

    [Header("Run Info UI")]
    public TextMeshProUGUI timerText;   // <-- Перетягни сюди текст таймера
    public TextMeshProUGUI floorText;   // <-- Перетягни сюди текст поверху

    // Викликати з GameManager.Update()
    public void UpdateTimerUI(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Викликати з GameManager.SpawnPlayerAt()
    public void UpdateFloorText(int current, int max)
    {
        if (floorText != null)
        {
            floorText.text = $"FLOOR {current} / {max}";
        }
    }
    void Awake()
    {
        // Жорстко перезаписуємо Instance на поточний об'єкт
        // Це гарантує, що Instance завжди вказує на живий HUD цієї сцени
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject); // Або Destroy(this.gameObject), але краще просто перезаписати:
        }
        Instance = this;

        // Ховаємо меню на старті
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }
}