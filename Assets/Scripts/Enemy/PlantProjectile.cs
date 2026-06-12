using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(BashableProjectile))]
public class PlantProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float environmentDestroyGraceTime = 0.08f;

    [Header("Damage")]
    [SerializeField] private int playerDamage = 1;
    [SerializeField] private int enemyDamageWhenBashed = 1;

    [Header("Collision")]
    [SerializeField] private bool destroyOnPlayerHit = true;
    [SerializeField] private bool destroyOnEnemyHitAfterBash = true;
    [SerializeField] private bool destroyOnEnvironmentHit = true;
    [SerializeField] private float ignoreOwnerTime = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugHits = true;

    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private BashableProjectile bashableProjectile;

    private GameObject owner;
    private EnemyHealth ownerHealth;

    private float spawnTime;
    private bool hasLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        bashableProjectile = GetComponent<BashableProjectile>();
    }

    private void Start()
    {
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction, GameObject projectileOwner = null)
    {
        owner = projectileOwner;
        ownerHealth = FindEnemyHealth(owner);

        if (direction == Vector2.zero)
        {
            direction = Vector2.up;
        }

        direction.Normalize();

        hasLaunched = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        IgnoreOwnerCollisionTemporarily();

        if (debugHits)
        {
            Debug.Log(name + " launched. Owner: " + (owner != null ? owner.name : "None"));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (!hasLaunched)
            return;

        if (other == null)
            return;

        EnemyHealth enemyHealth = FindEnemyHealth(other);
        PlayerHealth playerHealth = FindPlayerHealth(other);

        if (debugHits)
        {
            Debug.Log(
                name +
                " hit " + other.name +
                " | WasBashed: " + bashableProjectile.WasBashed +
                " | EnemyHealth: " + (enemyHealth != null ? enemyHealth.name : "None") +
                " | OwnerHealth: " + (ownerHealth != null ? ownerHealth.name : "None")
            );
        }

        // Before bash, don't hurt the plant that shot this.
        if (!bashableProjectile.WasBashed && ownerHealth != null && enemyHealth == ownerHealth)
        {
            return;
        }

        // Normal projectile hurts the player.
        if (playerHealth != null)
        {
            if (!bashableProjectile.WasBashed)
            {
                playerHealth.TakeDamage(playerDamage);

                if (destroyOnPlayerHit)
                {
                    Destroy(gameObject);
                }
            }

            return;
        }

        // Bashed projectile hurts enemies, including its owner.
        if (enemyHealth != null)
        {
            if (bashableProjectile.WasBashed)
            {
                enemyHealth.TakeDamage(enemyDamageWhenBashed);

                if (destroyOnEnemyHitAfterBash)
                {
                    Destroy(gameObject);
                }
            }

            return;
        }

        if (destroyOnEnvironmentHit && other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (Time.time < spawnTime + environmentDestroyGraceTime)
            {
                return;
            }

            Destroy(gameObject);
            return;
        }

        if (destroyOnEnvironmentHit && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private EnemyHealth FindEnemyHealth(Collider2D other)
    {
        if (other == null)
            return null;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInChildren<EnemyHealth>();
        }

        return enemyHealth;
    }

    private EnemyHealth FindEnemyHealth(GameObject obj)
    {
        if (obj == null)
            return null;

        EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            enemyHealth = obj.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth == null)
        {
            enemyHealth = obj.GetComponentInChildren<EnemyHealth>();
        }

        return enemyHealth;
    }

    private PlayerHealth FindPlayerHealth(Collider2D other)
    {
        if (other == null)
            return null;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        return playerHealth;
    }

    private void IgnoreOwnerCollisionTemporarily()
    {
        if (owner == null || projectileCollider == null)
            return;

        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();

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
        if (owner == null || projectileCollider == null)
            return;

        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, ownerCollider, false);
            }
        }

        if (debugHits)
        {
            Debug.Log(name + " restored owner collision.");
        }
    }
}