using System;
using UnityEngine;

public interface IEventService
{
    // --- ПОДІЇ (на них підписуються) ---
    event Action OnPlayerDied;
    event Action OnBossDied;
    event Action<float> OnPlayerTookDamage; // float = скільки шкоди отримав
    event Action<GameObject> OnEnemyDied;   // GameObject = який саме ворог помер (для спавну луту тощо)

    // --- МЕТОДИ ВИКЛИКУ (їх викликають, коли щось сталось) ---
    void TriggerPlayerDied();
    void TriggerBossDied();
    void TriggerPlayerTookDamage(float damage);
    void TriggerEnemyDied(GameObject enemy);
}