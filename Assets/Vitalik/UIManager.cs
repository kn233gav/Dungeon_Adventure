using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Елементи UI")]
    public GameObject gameOverPanel; // Панель, яка вмикається при смерті
    public TextMeshProUGUI notificationText; // Наприклад, текст отриманої шкоди

    // Посилання на сервіс подій
    private IEventService _eventService;

    void Start()
    {
        // 1. Отримуємо сервіс
        _eventService = ServiceLocator.Get<IEventService>();

        // 2. ПІДПИСУЄМОСЯ на події
        // Коли станеться OnPlayerDied -> виконається метод ShowGameOver
        _eventService.OnPlayerDied += ShowGameOver;

        // Коли станеться OnPlayerTookDamage -> виконається метод ShowDamageEffect
        _eventService.OnPlayerTookDamage += ShowDamageEffect;

        // Ховаємо панель програшу на старті
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void OnDestroy()
    {
        // 3. ВІДПИСУЄМОСЯ (Обов'язково!)
        // Якщо цього не зробити, при перезавантаженні сцени будуть помилки
        if (_eventService != null)
        {
            _eventService.OnPlayerDied -= ShowGameOver;
            _eventService.OnPlayerTookDamage -= ShowDamageEffect;
        }
    }

    // Цей метод спрацює сам, коли PlayerController викличе TriggerPlayerDied()
    private void ShowGameOver()
    {
        Debug.Log("UI: Показую екран смерті");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Тут можна додати зупинку часу або курсор
        // Time.timeScale = 0; 
    }

    // Цей метод спрацює, коли гравець отримає удар
    private void ShowDamageEffect(float damage)
    {
        if (notificationText != null)
        {
            notificationText.text = $"-{damage} HP";
            // Можна додати анімацію зникнення тексту, але це вже деталі
            StartCoroutine(ClearTextAfterDelay());
        }
    }

    private System.Collections.IEnumerator ClearTextAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (notificationText != null) notificationText.text = "";
    }
}