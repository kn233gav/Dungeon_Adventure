using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Slider healthSlider;

    private BossGolem boss;

    public void Init(BossGolem bossController)
    {
        this.boss = bossController;

        // Налаштовуємо межі слайдера
        healthSlider.maxValue = boss.maxHealth;
        healthSlider.value = boss.currentHealth;

        // Підписуємось на подію (щоб чути зміни)
        boss.OnHealthChanged += UpdateSlider;
    }

    void UpdateSlider(float current, float max)
    {
        healthSlider.value = current;
    }

    private void OnDisable()
    {
        // Відписуємось, коли панель зникає або бос вмирає
        if (boss != null)
        {
            boss.OnHealthChanged -= UpdateSlider;
        }
    }
}