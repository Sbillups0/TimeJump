using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BashableProjectile))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;

    [Header("Collision")]
    [SerializeField] private float ignoreOwnerTime = 0.2f;

    private Rigidbody2D rb;
    private BashableProjectile bashableProjectile;
    private GameObject owner;
    private bool hasLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bashableProjectile = GetComponent<BashableProjectile>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction, GameObject projectileOwner)
    {
        if (direction == Vector2.zero)
        {
            direction = Vector2.left;
        }

        owner = projectileOwner;
        hasLaunched = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        IgnoreOwnerCollisionTemporarily();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider2D other)
    {
        if (!hasLaunched)
            return;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInParent<EnemyHealth>();
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        // Ignore the shooter only while the projectile has NOT been bashed.
        if (!bashableProjectile.WasBashed && owner != null)
        {
            EnemyHealth ownerHealth = owner.GetComponent<EnemyHealth>();

            if (enemyHealth != null && enemyHealth == ownerHealth)
            {
                return;
            }
        }

        // Normal enemy projectile damages player.
        if (playerHealth != null)
        {
            if (!bashableProjectile.WasBashed)
            {
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
            }

            return;
        }

        // Bashed projectile damages enemies, including the enemy that shot it.
        if (enemyHealth != null)
        {
            if (bashableProjectile.WasBashed)
            {
                enemyHealth.TakeDamage(damage);
                Destroy(gameObject);
            }

            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private void IgnoreOwnerCollisionTemporarily()
    {
        if (owner == null)
            return;

        Collider2D projectileCollider = GetComponent<Collider2D>();
        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();

        if (projectileCollider == null || ownerColliders == null)
            return;

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, ownerCollider, true);
            }
        }

        Invoke(nameof(RestoreOwnerCollision), ignoreOwnerTime);
    }

    private void RestoreOwnerCollision()
    {
        if (owner == null)
            return;

        Collider2D projectileCollider = GetComponent<Collider2D>();
        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();

        if (projectileCollider == null || ownerColliders == null)
            return;

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, ownerCollider, false);
            }
        }
    }
}