using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float moveSpeed;
    private float range;
    private Vector3 startPosition;
    private Rigidbody rb;
    private float damageAmount;

    [Header("Damage Settings")]
    [SerializeField] private LayerMask collisionLayers;

    public void Setup(float speed, float maxRange, float damage)
    {
        moveSpeed = speed;
        range = maxRange;
        damageAmount = damage;

        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = transform.forward * moveSpeed;
    }

    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            DisableProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Куля врізалася в: {other.name} (Шар: {LayerMask.LayerToName(other.gameObject.layer)})");
        if ((collisionLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            IDamageable damageableTarget = other.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(damageAmount);
                Debug.Log($"Hit target: {other.name}");
            }


            DisableProjectile();
        }
    }

    private void DisableProjectile()
    {

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}