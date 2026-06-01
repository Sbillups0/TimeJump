using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Jump Charge")]
    public float minJumpVelocity = 10f;
    public float maxJumpVelocity = 35f;
    public float maxChargeTime = 0.5f;

    [Header("Floaty Jump")]
    public float normalGravity = 2f;
    public float risingGravity = 0.8f;
    public float fallingGravity = 1.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Enemy Stomp")]
    public int stompDamage = 1;
    public float stompBounceVelocity = 12f;

    [Header("Attack Damage")]
    public int attackDamage = 1;
    public Vector2 attackBoxSize = new Vector2(0.8f, 0.6f);
    public Vector2 attackBoxOffset = new Vector2(0.7f, 0f);
    public LayerMask enemyLayer;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip chargeJumpSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip landingSound;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool isCharging;
    private float chargeTimer;

    private bool jumpRequested;
    private float jumpVelocity;

    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        CheckGrounded();

        if (!wasGrounded && isGrounded)
        {
            PlaySound(landingSound);
        }

        wasGrounded = isGrounded;

        HandleMovementInput();
        HandleAttackInput();
        HandleJumpChargeInput();

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
        }
    }

    void FixedUpdate()
    {
        float xVelocity = isCharging ? 0f : moveInput * moveSpeed;
        float yVelocity = rb.linearVelocity.y;

        if (jumpRequested)
        {
            yVelocity = jumpVelocity;
            jumpRequested = false;
        }

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);

        ApplyFloatyGravity();
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            Debug.LogWarning("GroundCheck is not assigned.");
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void HandleMovementInput()
    {
        moveInput = 0f;

        if (Keyboard.current == null)
        {
            Debug.LogWarning("Keyboard.current is null. Input System may not be set up.");
            return;
        }

        if (!isCharging)
        {
            if (Keyboard.current.aKey.isPressed)
            {
                moveInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                moveInput = 1f;
            }
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void HandleAttackInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            PlaySound(attackSound);
            TryAttackEnemies();
        }
    }

    private void HandleJumpChargeInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isCharging)
        {
            jumpVelocity = minJumpVelocity;
            jumpRequested = true;

            PlaySound(jumpSound);
        }

        if (Keyboard.current.pKey.wasPressedThisFrame && isGrounded)
        {
            isCharging = true;
            chargeTimer = 0f;

            if (animator != null)
            {
                animator.SetBool("Charging", true);
            }
        }

        if (isCharging && Keyboard.current.pKey.isPressed)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, maxChargeTime);
        }

        if (isCharging && Keyboard.current.pKey.wasReleasedThisFrame)
        {
            isCharging = false;

            if (animator != null)
            {
                animator.SetBool("Charging", false);
            }

            float chargePercent = chargeTimer / maxChargeTime;
            jumpVelocity = Mathf.Lerp(minJumpVelocity, maxJumpVelocity, chargePercent);

            jumpRequested = true;

            PlaySound(chargeJumpSound);
        }
    }

    private void ApplyFloatyGravity()
    {
        if (isGrounded)
        {
            rb.gravityScale = normalGravity;
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.gravityScale = risingGravity;
        }
        else
        {
            rb.gravityScale = fallingGravity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryStompEnemyFromCollision(collision);
    }

    private void TryStompEnemyFromCollision(Collision2D collision)
    {
        EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
        {
            return;
        }

        bool hitEnemyFromAbove = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                hitEnemyFromAbove = true;
                break;
            }
        }

        if (!hitEnemyFromAbove)
        {
            return;
        }

        enemyHealth.TakeDamage(stompDamage);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceVelocity);

        isCharging = false;
        jumpRequested = false;

        if (animator != null)
        {
            animator.SetBool("Charging", false);
        }
    }

    private void TryAttackEnemies()
    {
        float direction = 1f;

        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            direction = -1f;
        }

        Vector2 attackOffset = new Vector2(
            attackBoxOffset.x * direction,
            attackBoxOffset.y
        );

        Vector2 boxCenter = (Vector2)transform.position + attackOffset;

        Collider2D[] enemyHits = Physics2D.OverlapBoxAll(
            boxCenter,
            attackBoxSize,
            0f,
            enemyLayer
        );

        if (enemyHits.Length == 0)
        {
            return;
        }

        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (Collider2D enemyHit in enemyHits)
        {
            EnemyHealth enemyHealth = enemyHit.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
            {
                continue;
            }

            if (damagedEnemies.Contains(enemyHealth))
            {
                continue;
            }

            enemyHealth.TakeDamage(attackDamage);
            damagedEnemies.Add(enemyHealth);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        float direction = 1f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null && sr.flipX)
        {
            direction = -1f;
        }

        Vector2 attackOffset = new Vector2(
            attackBoxOffset.x * direction,
            attackBoxOffset.y
        );

        Gizmos.DrawWireCube(
            transform.position + (Vector3)attackOffset,
            attackBoxSize
        );

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}