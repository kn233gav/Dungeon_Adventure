using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPauseState : IPlayerState
{
    private PlayerController _player;
    private HUDManager _hud;

    public PlayerPauseState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        Debug.Log("PAUSE STATE: Entered");

        // 1. Зупиняємо час
        Time.timeScale = 0f;

        // 2. Отримуємо доступ до HUD
        _hud = HUDManager.Instance;
        if (_hud != null && _hud.pauseMenuPanel != null)
        {
            _hud.pauseMenuPanel.SetActive(true);

            // 3. Підписуємо методи на кнопки
            if (_hud.resumeButton) _hud.resumeButton.onClick.AddListener(OnResume);
            if (_hud.restartButton) _hud.restartButton.onClick.AddListener(OnRestart);
            if (_hud.quitButton) _hud.quitButton.onClick.AddListener(OnQuit);
        }

        // 4. Вмикаємо курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Exit()
    {
        // 1. Відновлюємо час
        Time.timeScale = 1f;

        // 2. Ховаємо меню та відписуємо кнопки (важливо для уникнення помилок)
        if (_hud != null)
        {
            if (_hud.pauseMenuPanel != null) _hud.pauseMenuPanel.SetActive(false);

            if (_hud.resumeButton) _hud.resumeButton.onClick.RemoveListener(OnResume);
            if (_hud.restartButton) _hud.restartButton.onClick.RemoveListener(OnRestart);
            if (_hud.quitButton) _hud.quitButton.onClick.RemoveListener(OnQuit);
        }

        // 3. Ховаємо курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void HandleInput()
    {
        // Дозволяємо вийти з паузи також на ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnResume();
        }
    }

    public void Update() { }
    public void FixedUpdate() { }

    // --- Методи для кнопок ---

    private void OnResume()
    {
        // Повертаємося в Idle стан (або можна зберегти попередній, але Idle - безпечний варіант)
        _player.ChangeState(_player.idleState);
    }

    private void OnRestart()
    {
        Debug.Log("PAUSE: Перезапуск забігу...");

        // 1. Повертаємо час, інакше в новій грі він стоятиме
        Time.timeScale = 1f;

        // 2. ЗАМІСТЬ простого перезавантаження сцени:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); <--- ЦЕ БУЛО НЕПРАВИЛЬНО

        // 3. Викликаємо повний рестарт через GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewRun(); // Це скине поверх на 1 і таймер на 0
        }
        else
        {
            // Страховка, якщо раптом GameManager немає
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnQuit()
    {
        Debug.Log("Вихід з гри...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}