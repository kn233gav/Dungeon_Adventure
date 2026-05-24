using System;
using UnityEngine;

public class EventBus : IEventService
{
    public event Action OnPlayerDied;
    public event Action OnBossDied;
    public event Action<float> OnPlayerTookDamage;
    public event Action<GameObject> OnEnemyDied;

    public void TriggerPlayerDied()
    {
        // ?.Invoke() означає "викликати, якщо є хоча б один підписник"
        Debug.Log("[EventBus] Player Died!");
        OnPlayerDied?.Invoke();
    }

    public void TriggerBossDied()
    {
        Debug.Log("[EventBus] Boss Died!");
        OnBossDied?.Invoke();
    }

    public void TriggerPlayerTookDamage(float damage)
    {
        // Debug.Log($"[EventBus] Player took {damage} damage");
        OnPlayerTookDamage?.Invoke(damage);
    }

    public void TriggerEnemyDied(GameObject enemy)
    {
        Debug.Log($"[EventBus] Enemy Died: {enemy.name}");
        OnEnemyDied?.Invoke(enemy);
    }
}