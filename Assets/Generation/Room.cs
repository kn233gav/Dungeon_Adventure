using UnityEngine;
using System.Collections.Generic;

public enum RoomType { Start, Normal, Boss, Corridor }

public class Room : MonoBehaviour
{
    [Header("Settings")]
    public RoomType type;

    public List<Transform> connectors;
    public List<BoxCollider> roomBounds;

    [Header("Spawns")]
    public List<Transform> enemySpawnPoints;
    public Transform bossSpawnPoint;
    public BossRoomTrigger bossTrigger;
    [Header("Player")]
    public Transform playerSpawnPoint;  
    [HideInInspector]
    public List<Room> childRooms = new List<Room>();

    public void DisableConnector(Transform connector)
    {
        if (connectors.Contains(connector))
        {
            connectors.Remove(connector);
        }
    }

    private void OnDrawGizmos()
    {
        if (connectors != null)
        {
            foreach (var c in connectors)
            {
                if (c == null) continue;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(c.position, c.position + c.forward * 1f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(c.position, 0.2f);
            }
        }
        if (enemySpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var s in enemySpawnPoints)
            {
                if (s == null) continue;
                Gizmos.DrawWireSphere(s.position, 0.5f);
            }
        }
    }
}