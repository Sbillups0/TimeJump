using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseballProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float spinSpeed = 720f;

    [Header("Aim Assist")]
    [SerializeField] private bool useAimAssist = true;
    [SerializeField] private float aimAssistRadius = 3f;
    [SerializeField] private float aimAssistStrength = 4f;
    [SerializeField] private float maxAimAssistAngle = 45f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField, Range(0f, 1f)] private float bounceVolume = 1f;
    [SerializeField] private float bounceSoundCooldown = 0.05f;

    private float lastBounceSoundTime;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        ApplyAimAssist();
    }

    private void Update()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            direction = rb.linearVelocity.normalized;
        }

        float spinDirection = direction.x >= 0f ? -1f : 1f;
        transform.Rotate(0f, 0f, spinDirection * spinSpeed * Time.deltaTime);
    }

    public void Launch(Vector2 launchDirection)
    {
        if (launchDirection == Vector2.zero)
        {
            launchDirection = Vector2.right;
        }

        direction = launchDirection.normalized;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = direction * speed;

        Debug.Log("Baseball velocity: " + rb.linearVelocity);
    }

    private void ApplyAimAssist()
    {
        if (!useAimAssist)
            return;

        if (rb.linearVelocity.sqrMagnitude < 0.01f)
            return;

        Transform target = FindBestAimAssistTarget();

        if (target == null)
            return;

        Vector2 currentDirection = rb.linearVelocity.normalized;
        Vector2 targetDirection = ((Vector2)target.position - rb.position).normalized;

        float angleToTarget = Vector2.Angle(currentDirection, targetDirection);

        if (angleToTarget > maxAimAssistAngle)
            return;

        Vector2 assistedDirection = Vector2.Lerp(
            currentDirection,
            targetDirection,
            aimAssistStrength * Time.fixedDeltaTime
        ).normalized;

        rb.linearVelocity = assistedDirection * speed;
    }

    private Transform FindBestAimAssistTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            aimAssistRadius,
            enemyLayer
        );

        Transform bestTarget = null;
        float bestScore = Mathf.Infinity;

        Vector2 currentDirection = rb.linearVelocity.normalized;

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth == null || enemyHealth.IsDead)
                continue;

            Vector2 toEnemy = (Vector2)hit.bounds.center - rb.position;

            if (toEnemy.sqrMagnitude < 0.01f)
                continue;

            Vector2 directionToEnemy = toEnemy.normalized;
            float angle = Vector2.Angle(currentDirection, directionToEnemy);

            if (angle > maxAimAssistAngle)
                continue;

            float distance = toEnemy.magnitude;

            // Lower score is better. This favors enemies close to the ball's current path.
            float score = angle * 0.75f + distance * 0.25f;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemyHealth.transform;
            }
        }

        return bestTarget;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            return;
        }

        EnemyHealth enemyHealth = collision.collider.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            enemyHealth = collision.collider.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            Debug.Log("Baseball hit enemy: " + enemyHealth.name);
            enemyHealth.TakeDamage(1);
            Destroy(gameObject);
            return;
        }

        PlayBounceSound();
    }

    private void PlayBounceSound()
    {
        if (audioSource == null || bounceSound == null)
        {
            return;
        }

        if (Time.time < lastBounceSoundTime + bounceSoundCooldown)
        {
            return;
        }

        lastBounceSoundTime = Time.time;
        audioSource.PlayOneShot(bounceSound, bounceVolume);
    }

    private void OnDrawGizmosSelected()
    {
        if (!useAimAssist)
            return;

        Gizmos.DrawWireSphere(transform.position, aimAssistRadius);
    }
}