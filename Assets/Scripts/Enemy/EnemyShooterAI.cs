using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyShooterAI : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float shootingRange = 4f;
    [SerializeField] private float stopBuffer = 0.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool flyingEnemy = true;
    [Header("Facing")]
    [SerializeField] private bool spriteFacesRightByDefault = true;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0.6f, 0f);

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float shootCooldown = 1.5f;

    [Header("Animation")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string shootTrigger = "Shoot";

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float nextShootTime;
    private Vector2 lastAimDirection = Vector2.left;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (flyingEnemy)
        {
            rb.gravityScale = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            StopMoving();
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
        {
            StopMoving();
            return;
        }

        Vector2 directionToPlayer = toPlayer.normalized;

        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            lastAimDirection = directionToPlayer;
        }

        FaceDirection(directionToPlayer);

        if (distanceToPlayer > shootingRange + stopBuffer)
        {
            MoveTowardPlayer(directionToPlayer);
        }
        else
        {
            StopMoving();

            if (Time.time >= nextShootTime)
            {
                TryShoot();
            }
        }
    }

    private void MoveTowardPlayer(Vector2 direction)
    {
        if (flyingEnemy)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }

        if (animator != null)
        {
            animator.SetFloat(speedParameter, rb.linearVelocity.magnitude);
        }
    }

    private void StopMoving()
    {
        if (flyingEnemy)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (animator != null)
        {
            animator.SetFloat(speedParameter, 0f);
        }
    }

    private void TryShoot()
    {
        nextShootTime = Time.time + shootCooldown;

        if (animator != null)
        {
            animator.SetTrigger(shootTrigger);
        }
        else
        {
            FireProjectile();
        }
    }

    // Call this from an Animation Event during the shoot animation.
    public void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("Enemy projectile prefab or spawn point is missing.");
            return;
        }

        Vector2 direction = lastAimDirection;

        if (player != null)
        {
            direction = ((Vector2)player.position - (Vector2)projectileSpawnPoint.position).normalized;
        }

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile != null)
        {
            projectile.Launch(direction, gameObject);
        }
        else
        {
            Debug.LogWarning("Enemy projectile prefab needs an EnemyProjectile script.");
        }
    }

    private void FaceDirection(Vector2 direction)
    {
        if (spriteRenderer == null)
            return;

        if (Mathf.Abs(direction.x) <= 0.05f)
            return;

        bool movingLeft = direction.x < 0f;

        if (spriteFacesRightByDefault)
        {
            spriteRenderer.flipX = movingLeft;
        }
        else
        {
            spriteRenderer.flipX = !movingLeft;
        }

        UpdateProjectileSpawnPointSide(direction);
    }

    private void UpdateProjectileSpawnPointSide(Vector2 direction)
    {
        if (projectileSpawnPoint == null)
            return;

        float xSign = direction.x >= 0f ? 1f : -1f;

        projectileSpawnPoint.localPosition = new Vector3(
            Mathf.Abs(spawnOffset.x) * xSign,
            spawnOffset.y,
            0f
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}