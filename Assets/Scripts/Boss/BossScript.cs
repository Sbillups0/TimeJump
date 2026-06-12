using System.Collections;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseWeight = 0.6f;
    public float directionChangeTime = 2f;

    [Header("Floating")]
    public float floatHeight = 3f;
    public float floatSmoothing = 3f;

    [Header("Bob Animation")]
    public float bobAmount = 0.2f;
    public float bobSpeed = 2f;

    [Header("Dash Attack")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 4f;
    public float windUpTime = 1f;
    public int dashDamage = 20;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootCooldown = 3f;
    public float farDistance = 8f;
    public float shootAnimationTime = 1f;

    [Header("References")]
    public Transform player;

    private Rigidbody2D rb;
    private Animator anim;
    private float randomDirectionX;
    private float timer;
    private float shootCooldownTimer = 0f;

    private enum BossState { Moving, WindUp, Dashing, Shooting }
    private BossState state = BossState.Moving;

    private float dashCooldownTimer = 4f;
    private float windUpTimer = 0f;
    private float dashTimer = 0f;
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        PickNewRandomDirection();
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
            PickNewRandomDirection();

        shootCooldownTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (state == BossState.Moving)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (shootCooldownTimer <= 0 && distanceToPlayer >= farDistance)
            {
                StartCoroutine(ShootAnimationRoutine());
                shootCooldownTimer = shootCooldown;
            }
            else if (dashCooldownTimer <= 0 && distanceToPlayer < farDistance)
            {
                state = BossState.WindUp;
                windUpTimer = windUpTime;
            }
        }
        else if (state == BossState.WindUp)
        {
            windUpTimer -= Time.deltaTime;
            dashDirection = (player.position - transform.position).normalized;

            if (windUpTimer <= 0)
            {
                state = BossState.Dashing;
                dashTimer = dashDuration;
            }
        }
        else if (state == BossState.Dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                state = BossState.Moving;
                dashCooldownTimer = dashCooldown;
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (state == BossState.Dashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else if (state == BossState.WindUp)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            float toPlayerX = Mathf.Sign(player.position.x - transform.position.x);
            float moveX = (toPlayerX * chaseWeight + randomDirectionX * (1f - chaseWeight));

            float targetY = player.position.y + floatHeight;
            targetY += Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            float newY = Mathf.Lerp(transform.position.y, targetY, Time.fixedDeltaTime * floatSmoothing);
            float moveY = (newY - transform.position.y) / Time.fixedDeltaTime;

            rb.linearVelocity = new Vector2(moveX * moveSpeed, moveY);
        }

        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    IEnumerator ShootAnimationRoutine()
    {
        if (anim != null)
            anim.SetBool("isShooting", true);

        // Wait for animation to reach last frame
        yield return new WaitForSeconds(shootAnimationTime - 0.1f);

        // Fire projectile on last frame
        FireProjectile();

        // Brief pause then return to idle
        yield return new WaitForSeconds(0.1f);

        if (anim != null)
            anim.SetBool("isShooting", false);
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector2 direction = (player.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = direction.x > 0;

        BossProjectile bp = proj.GetComponent<BossProjectile>();
        if (bp != null)
            bp.Launch(direction);

        Debug.Log("Boss fired projectile!");
    }

    void PickNewRandomDirection()
    {
        randomDirectionX = Random.Range(-1f, 1f);
        timer = directionChangeTime + Random.Range(-0.5f, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && state == BossState.Dashing)
            DealDamageToPlayer(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && state == BossState.Dashing)
            DealDamageToPlayer(other);
    }

    void DealDamageToPlayer(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            playerHealth.TakeDamage(dashDamage, knockbackDir);
            Debug.Log("Player hit by dash for " + dashDamage + " damage!");
        }

        state = BossState.Moving;
        dashCooldownTimer = dashCooldown;
    }

    void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}
