using System.Collections.Generic;
using UnityEngine;

public abstract class RoomFactory : ScriptableObject
{
    public abstract Room GetStartRoom();
    public abstract Room GetBossRoom();
    public abstract Room GetRandomNormalRoom();
    public abstract Room GetRandomCorridor();

    public abstract GameObject GetRandomEnemy();    
    public abstract GameObject GetBoss();
    public abstract GameObject GetWall();
}

[CreateAssetMenu(menuName = "Dungeon/Dungeon Factory")]
public class DungeonRoomFactory : RoomFactory
{
    [Header("Main Rooms")]
    public Room startRoomPrefab;
    public Room bossRoomPrefab;
    public List<Room> normalRoomPrefabs;

    [Header("Corridors")]
    public List<Room> corridorPrefabs;

    [Header("Content")]
    public List<GameObject> enemyPrefabs;
    public GameObject bossEnemyPrefab;
    public GameObject wallPrefab;

    public override Room GetStartRoom() => startRoomPrefab;
    public override Room GetBossRoom() => bossRoomPrefab;

    public override Room GetRandomNormalRoom() => GetRandomFromList(normalRoomPrefabs);
    public override Room GetRandomCorridor() => GetRandomFromList(corridorPrefabs);

    public override GameObject GetRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
    }

    public override GameObject GetBoss() => bossEnemyPrefab;
    public override GameObject GetWall() => wallPrefab;

    private T GetRandomFromList<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[Random.Range(0, list.Count)];
    }

    public IEnumerable<Room> GetRandomizedOptions(bool wantCorridor)
    {
        List<Room> sourceList = wantCorridor ? corridorPrefabs : normalRoomPrefabs;
        List<Room> options = new List<Room>(sourceList);

        for (int i = 0; i < options.Count; i++)
        {
            Room temp = options[i];
            int randomIndex = Random.Range(i, options.Count);
            options[i] = options[randomIndex];
            options[randomIndex] = temp;
        }
        return options;
    }
}