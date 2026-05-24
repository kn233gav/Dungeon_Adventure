using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Цей лог покаже ім'я об'єкта, який увійшов
        Debug.Log($"У тригер зайшов об'єкт: {other.name}, Тег: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Це гравець! Завершуємо рівень.");
            GameManager.Instance.FinishLevelLogic();
        }
        else
        {
            Debug.Log("Це НЕ гравець (невірний тег).");
        }
    }
}