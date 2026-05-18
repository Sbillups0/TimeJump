using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;

    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private float specialCooldown = 0.6f;

    [Header("Baseball Projectile")]
    [SerializeField] private GameObject baseballProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpawnOffsetX = 0.6f;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackHitSound;
    [SerializeField, Range(0f, 1f)] private float attackHitVolume = 1f;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private bool isGrounded;

    private float facingDirection = 1f; // 1 for right, -1 for left
    private float nextAttackTime;
    private float nextSpecialTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        CheckGrounded();
        FlipCharacter();
        UpdateProjectileSpawnPoint();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            facingDirection = Mathf.Sign(moveInput.x);
        }
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        StartBaseballAttack();
    }

    public void OnSpecial(InputValue value)
    {
        if (!value.isPressed || Time.time < nextSpecialTime)
            return;

        nextSpecialTime = Time.time + specialCooldown;
        StartSpecial();
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void FlipCharacter()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDirection < 0f;
        }
    }

    private void UpdateProjectileSpawnPoint()
    {
        if (projectileSpawnPoint == null)
            return;

        Vector3 localPosition = projectileSpawnPoint.localPosition;
        localPosition.x = Mathf.Abs(projectileSpawnOffsetX) * facingDirection;
        projectileSpawnPoint.localPosition = localPosition;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    private void StartBaseballAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Debug.Log("Base attack animation started.");
    }

    private void StartSpecial()
    {
        // Placeholder for future deflect ability.
        // For now, this only triggers the special animation if one exists.

        if (animator != null)
        {
            animator.SetTrigger("Special");
        }

        Debug.Log("Special: deflect placeholder.");
    }

    public void SpawnBaseballProjectile()
    {
        if (baseballProjectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("Baseball projectile prefab or spawn point not assigned.");
            return;
        }
        if (audioSource != null && attackHitSound != null)
        {
            audioSource.PlayOneShot(attackHitSound, attackHitVolume);
        }

        Vector2 launchDirection = new Vector2(facingDirection, 0f);

        GameObject projectile = Instantiate(
            baseballProjectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        BaseballProjectile baseball = projectile.GetComponent<BaseballProjectile>();

        if (baseball != null)
        {
            baseball.Launch(launchDirection);
        }
        else
        {
            Debug.LogWarning("The spawned baseball prefab does not have a BaseballProjectile script.");
        }

        Debug.Log("Baseball spawned from animation event " + launchDirection);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, 0.25f);
        }
    }
}