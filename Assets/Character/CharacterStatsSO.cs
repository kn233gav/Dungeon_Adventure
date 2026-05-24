using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character/Character Data")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Загальна інформація")]
    public string characterName;
    public GameObject characterPrefab; 

    [Header("Бойові параметри")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;

    [Header("Рух")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float combatMoveSpeed = 4f;

    [Header("Атака")]
    public float attackDuration = 0.6f;
    public float attackStaminaCost = 10f;

    [Header("Специфіка класу (для дальніх)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float attackRange = 50f;

}