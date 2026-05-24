using UnityEngine;

public class PotionPickup : MonoBehaviour
{
    [Header("Налаштування")]
    public int amount = 1; // Скільки зілля додаємо
    public string pickupSound = "Potion_Pickup"; // Назва звуку (якщо є в AudioService)

    [Header("Візуальні ефекти")]
    public float rotateSpeed = 10f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private Vector3 startPos;
    private IAudioService _audioService;

    void Start()
    {
        startPos = transform.position;
        // Спробуємо знайти аудіо сервіс (не обов'язково, але бажано)
        try { _audioService = ServiceLocator.Get<IAudioService>(); } catch { }

        // Щоб предмет не провалювався, якщо спавниться низько
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; // Важливо! Це має бути тригер
    }

    void Update()
    {
        // Обертання навколо своєї осі
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // Легке погойдування вгору-вниз
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // Додаємо зілля
                player.AddPotion(amount);

                // Граємо звук (якщо є)
                if (_audioService != null) _audioService.PlaySFX(pickupSound, transform.position);

                // Візуальний ефект зникнення (опціонально можна додати партикли)
                Debug.Log("Зілля підібрано!");

                // Знищуємо об'єкт зі сцени
                Destroy(gameObject);
            }
        }
    }
}