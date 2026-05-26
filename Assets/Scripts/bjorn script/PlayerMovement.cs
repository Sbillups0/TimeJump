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

    [Header("Enemy Stomp / Landing Damage")]
    public int stompDamage = 1;
    public float stompBounceVelocity = 12f;

    // Bigger X = wider landing damage.
    // Smaller Y = less likely to hit enemies from the side.
    public Vector2 stompBoxSize = new Vector2(1.2f, 0.25f);

    // Position of the stomp box under the player.
    // Y should usually be negative.
    public Vector2 stompBoxOffset = new Vector2(0f, -0.55f);

    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool isCharging;
    private float chargeTimer;

    private bool jumpRequested;
    private float jumpVelocity;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        CheckGrounded();
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
        if (animator == null)
        {
            return;
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Attack");
        }
    }

    private void HandleJumpChargeInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isCharging)
        {
            jumpVelocity = minJumpVelocity;
            jumpRequested = true;
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

            Debug.Log("Charge: " + chargeTimer + " | Jump Velocity: " + jumpVelocity);
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
        TryStompEnemies();
    }

    private void TryStompEnemies()
    {
        // Only do landing damage while falling or landing.
        if (rb.linearVelocity.y > 0f)
        {
            return;
        }

        Vector2 boxCenter = (Vector2)transform.position + stompBoxOffset;

        Collider2D[] enemyHits = Physics2D.OverlapBoxAll(
            boxCenter,
            stompBoxSize,
            0f,
            enemyLayer
        );

        if (enemyHits.Length == 0)
        {
            return;
        }

        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
        bool stompedAtLeastOneEnemy = false;

        float stompLeft = boxCenter.x - stompBoxSize.x / 2f;
        float stompRight = boxCenter.x + stompBoxSize.x / 2f;
        float stompBottom = boxCenter.y - stompBoxSize.y / 2f;
        float stompTop = boxCenter.y + stompBoxSize.y / 2f;

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

            Bounds enemyBounds = enemyHit.bounds;

            bool horizontallyInside =
                enemyBounds.max.x >= stompLeft &&
                enemyBounds.min.x <= stompRight;

            bool verticallyInside =
                enemyBounds.max.y >= stompBottom &&
                enemyBounds.min.y <= stompTop;

            bool actuallyInsideStompBox = horizontallyInside && verticallyInside;

            if (!actuallyInsideStompBox)
            {
                continue;
            }

            enemyHealth.TakeDamage(stompDamage);
            damagedEnemies.Add(enemyHealth);
            stompedAtLeastOneEnemy = true;
        }

        if (stompedAtLeastOneEnemy)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceVelocity);

            isCharging = false;
            jumpRequested = false;

            if (animator != null)
            {
                animator.SetBool("Charging", false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(
            transform.position + (Vector3)stompBoxOffset,
            stompBoxSize
        );

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}