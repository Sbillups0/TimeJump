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
    [SerializeField] private bool destroyOnPlayerHit = true;
    [SerializeField] private bool destroyOnEnvironmentHit = true;

    [Header("Deflected Behavior")]
    [SerializeField] private int enemyDamageWhenBashed = 1;
    [SerializeField] private bool destroyOnEnemyHitAfterBash = true;

    private Rigidbody2D rb;
    private BashableProjectile bashableProjectile;
    private GameObject owner;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        if (direction == Vector2.zero)
        {
            direction = Vector2.up;
        }

        direction.Normalize();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner)
            return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerDamage);
            }

            if (destroyOnPlayerHit)
            {
                Destroy(gameObject);
            }

            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            if (bashableProjectile != null && bashableProjectile.WasBashed)
            {
                EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(enemyDamageWhenBashed);
                }

                if (destroyOnEnemyHitAfterBash)
                {
                    Destroy(gameObject);
                }
            }

            return;
        }

        if (destroyOnEnvironmentHit && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (Time.time < spawnTime + environmentDestroyGraceTime)
            {
                return;
            }
            
            Destroy(gameObject);
        }
    }
}