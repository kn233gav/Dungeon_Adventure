using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("Керування Дверима")]
    public GameObject entryGate; // Вхід (закриється)
    public GameObject exitGate;  // Вихід (відкриється після перемоги)

    [Header("Посилання")]
    private BossHealthUI bossUI; // Знайдемо автоматично
    private BossGolem currentBoss; // Або BossGolem, якщо ти перейменував клас

    private bool isTriggered = false;

    private void Start()
    {
        // 1. На старті знаходимо UI на сцені (він один)
        bossUI = FindObjectOfType<BossHealthUI>(true);

        // 2. Налаштовуємо двері: вхід відкритий, вихід закритий (стіна)
        if (entryGate != null) entryGate.SetActive(false);
        if (exitGate != null) exitGate.SetActive(true);
    }

    // Цей метод можна викликати з генератора, якщо треба
    public void Setup(GameObject bossObj)
    {
        // Якщо у тебе клас називається BossGolem, зміни тут тип
        if (bossObj != null)
            currentBoss = bossObj.GetComponent<BossGolem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            Debug.Log("BossRoom: Бій почався!");

            // А. Шукаємо боса, якщо його ще не призначили
            if (currentBoss == null)
            {
                // Шукаємо BossGolem у дочірніх об'єктах кімнати або на сцені поруч
                currentBoss = GetComponentInParent<Room>()?.GetComponentInChildren<BossGolem>();

                // Якщо не знайшли в батьківському, шукаємо просто поруч (для тесту)
                if (currentBoss == null)
                    currentBoss = FindObjectOfType<BossGolem>();
            }

            // Б. Якщо бос є — починаємо магію
            if (currentBoss != null)
            {
                // 1. Закриваємо вхід
                if (entryGate != null) entryGate.SetActive(true);

                // 2. Підключаємо UI (ініціалізуємо його босом)
                if (bossUI != null)
                {
                    bossUI.gameObject.SetActive(true);

                    // !!! ВАЖЛИВО: Переконайся, що в BossHealthUI метод Init приймає правильний тип (BossGolem або GolemController)
                    bossUI.Init(currentBoss);
                }

                // 3. ПІДПИСУЄМОСЬ НА СМЕРТЬ БОСА
                // Замість перевірки в Update, ми просто чекаємо подію
                currentBoss.OnDeathEvent += HandleBossDeath;
            }
            else
            {
                Debug.LogError("BossRoom: Боса не знайдено! Двері не закриються.");
            }
        }
    }

    // Цей метод спрацює сам, коли бос помре
    private void HandleBossDeath()
    {
        Debug.Log("BossRoom: Перемога! Відкриваю двері.");

        // 1. Відкриваємо вихід
        if (exitGate != null) exitGate.SetActive(false);

        // 2. Відкриваємо вхід (щоб можна було повернутися назад, якщо треба)
        if (entryGate != null) entryGate.SetActive(false);

        // 3. Ховаємо UI через пару секунд
        if (bossUI != null)
        {
            // Відписуємось від подій всередині UI скрипта, тут просто ховаємо
            StartCoroutine(HideUIRoutine());
        }
    }

    private System.Collections.IEnumerator HideUIRoutine()
    {
        yield return new WaitForSeconds(3f); // Даємо гравцю насолодитись перемогою
        if (bossUI != null) bossUI.gameObject.SetActive(false);
    }

    // Обов'язково відписуємось при знищенні об'єкта, щоб уникнути помилок
    private void OnDestroy()
    {
        if (currentBoss != null)
        {
            currentBoss.OnDeathEvent -= HandleBossDeath;
        }
    }
}