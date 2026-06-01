using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BashableProjectile : MonoBehaviour
{
    public enum BashableType
    {
        Projectile,
        Enemy
    }

    [Header("Bash Type")]
    [SerializeField] private BashableType bashableType = BashableType.Projectile;

    [Header("Projectile Bash")]
    [SerializeField] private float redirectedSpeed = 16f;

    [Header("Enemy Bash")]
    [SerializeField] private bool damageEnemyOnBash = true;
    [SerializeField] private int bashDamage = 1;
    [SerializeField] private bool knockEnemyBack = true;
    [SerializeField] private float enemyKnockbackForce = 6f;
    [SerializeField] private float enemyKnockbackLockTime = 0.15f;

    [Header("Hold Behavior")]
    [SerializeField] private bool freezeDuringBash = true;

    private Rigidbody2D rb;
    private Vector2 storedVelocity;
    private float storedGravityScale;
    private RigidbodyType2D storedBodyType;
    private bool isHeldByBash;
    private float movementLockedUntil;

    public Rigidbody2D Rigidbody => rb;
    public bool IsHeldByBash => isHeldByBash;
    public bool WasBashed { get; private set; }
    public bool IsProjectile => bashableType == BashableType.Projectile;
    public bool IsEnemy => bashableType == BashableType.Enemy;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        storedGravityScale = rb.gravityScale;
        storedBodyType = rb.bodyType;
    }

    private void FixedUpdate()
    {
        if (!IsEnemy)
            return;

        if (Time.time < movementLockedUntil)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void HoldForBash()
    {
        if (isHeldByBash)
            return;

        isHeldByBash = true;

        storedVelocity = rb.linearVelocity;
        storedGravityScale = rb.gravityScale;
        storedBodyType = rb.bodyType;

        if (freezeDuringBash)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            if (IsProjectile)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else if (IsEnemy)
            {
                rb.gravityScale = 0f;
            }
        }

        Debug.Log("Held for bash: " + name);
    }

    public void ReleaseFromBash(Vector2 direction, float speedOverride = -1f)
    {
        isHeldByBash = false;
        WasBashed = true;

        if (direction == Vector2.zero)
        {
            direction = storedVelocity.sqrMagnitude > 0.01f
                ? storedVelocity.normalized
                : Vector2.right;
        }

        direction = direction.normalized;

        if (IsProjectile)
        {
            ReleaseProjectile(direction, speedOverride);
        }
        else if (IsEnemy)
        {
            ReleaseEnemy(direction);
        }

        Debug.Log("Released from bash: " + name + " Direction: " + direction);
    }

    private void ReleaseProjectile(Vector2 direction, float speedOverride)
    {
        float finalSpeed = speedOverride > 0f ? speedOverride : redirectedSpeed;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = storedGravityScale;
        rb.linearVelocity = direction * finalSpeed;
    }

    private void ReleaseEnemy(Vector2 direction)
    {
        rb.bodyType = storedBodyType;
        rb.gravityScale = storedGravityScale;

        if (damageEnemyOnBash)
        {
            EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(bashDamage);
            }
        }

        if (knockEnemyBack)
        {
            rb.linearVelocity = direction * enemyKnockbackForce;
            movementLockedUntil = Time.time + enemyKnockbackLockTime;
        }
    }

    public void MoveSlightly(Vector2 direction, float distance)
    {
        if (direction == Vector2.zero)
            return;

        transform.position += (Vector3)(direction.normalized * distance);
    }
}