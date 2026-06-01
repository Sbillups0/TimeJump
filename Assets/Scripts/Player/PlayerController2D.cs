using System.Collections;
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

    [Header("Baseball Projectile")]
    [SerializeField] private GameObject baseballProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Bash / Deflect")]
    [SerializeField] private float bashDetectionRadius = 1.2f;
    [SerializeField] private LayerMask bashableLayer;
    [SerializeField] private float bashTimeScale = 0.15f;
    [SerializeField] private float playerBashBoostForce = 14f;
    [SerializeField] private float projectileBashSpeed = 16f;
    [SerializeField] private float bashCooldown = 0.35f;
    [SerializeField] private float bashMovementLockTime = 0.18f;
    [SerializeField] private float ignoreBashedProjectileCollisionTime = 0.25f;
    [SerializeField] private bool freezePlayerDuringBash = true;

    [Header("Bash Aim Indicator")]
    [SerializeField] private Transform bashAimIndicator;
    [SerializeField] private float aimIndicatorDistance = 1.25f;
    [SerializeField] private float aimIndicatorRotationOffset = 0f;
    [SerializeField] private bool showAimIndicator = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackHitSound;
    [SerializeField, Range(0f, 1f)] private float attackHitVolume = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction specialAction;
    private Collider2D[] playerColliders;

    private Vector2 moveInput;
    private Vector2 lastAimDirection = Vector2.right;
    private Vector2 pendingAttackDirection = Vector2.right;

    private bool isGrounded;
    private bool isBashing;

    private float facingDirection = 1f;
    private float nextAttackTime;
    private float nextBashTime;
    private float movementLockedUntil;
    private float originalGravityScale;
    private float originalFixedDeltaTime;

    private BashableProjectile currentBashTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        playerColliders = GetComponentsInChildren<Collider2D>();

        originalGravityScale = rb.gravityScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        SubscribeToSpecialAction();
    }

    private void OnDisable()
    {
        UnsubscribeFromSpecialAction();
        ForceEndBashState();
    }

    private void Update()
    {
        CheckGrounded();
        FlipCharacter();
        UpdateProjectileSpawnPoint();
        UpdateAnimator();

        if (isBashing)
        {
            UpdateAimFromCurrentMoveInput();
        }

        UpdateBashAimIndicator();
    }

    private void FixedUpdate()
    {
        if (isBashing && freezePlayerDuringBash)
            return;

        if (Time.time < movementLockedUntil)
            return;

        Move();
    }

    // ----------------------------------------------------------------------
    // Input callbacks - called by PlayerInput Send Messages
    // ----------------------------------------------------------------------

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        UpdateFacingAndAimFromMoveInput();
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
        pendingAttackDirection = AdjustAimForGround(lastAimDirection);

        StartBaseballAttack();
    }

    public void OnSpecial(InputValue value)
    {
        if (value.isPressed)
        {
            TryStartBash();
        }
        else
        {
            ReleaseBash();
        }
    }

    // ----------------------------------------------------------------------
    // Direct InputAction callbacks - used to reliably catch Special release
    // ----------------------------------------------------------------------

    private void SubscribeToSpecialAction()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogWarning("PlayerInput or InputActions missing.");
            return;
        }

        specialAction = playerInput.actions.FindAction("Special", false);

        if (specialAction == null)
        {
            Debug.LogWarning("Could not find Special input action.");
            return;
        }

        specialAction.started += OnSpecialStarted;
        specialAction.performed += OnSpecialPerformed;
        specialAction.canceled += OnSpecialCanceled;
    }

    private void UnsubscribeFromSpecialAction()
    {
        if (specialAction == null)
            return;

        specialAction.started -= OnSpecialStarted;
        specialAction.performed -= OnSpecialPerformed;
        specialAction.canceled -= OnSpecialCanceled;
    }

    private void OnSpecialStarted(InputAction.CallbackContext context)
    {
        TryStartBash();
    }

    private void OnSpecialPerformed(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            TryStartBash();
        }
    }

    private void OnSpecialCanceled(InputAction.CallbackContext context)
    {
        ReleaseBash();
    }

    // ----------------------------------------------------------------------
    // Movement / aiming
    // ----------------------------------------------------------------------

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

    private void UpdateFacingAndAimFromMoveInput()
    {
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            facingDirection = Mathf.Sign(moveInput.x);
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastAimDirection = GetEightDirection(moveInput);
        }
        else
        {
            lastAimDirection = new Vector2(facingDirection, 0f);
        }
    }

    private void UpdateAimFromCurrentMoveInput()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastAimDirection = GetEightDirection(moveInput);
        }
    }

    private Vector2 GetEightDirection(Vector2 input)
    {
        float x = 0f;
        float y = 0f;

        if (input.x > 0.25f)
        {
            x = 1f;
        }
        else if (input.x < -0.25f)
        {
            x = -1f;
        }

        if (input.y > 0.25f)
        {
            y = 1f;
        }
        else if (input.y < -0.25f)
        {
            y = -1f;
        }

        Vector2 direction = new Vector2(x, y);

        if (direction == Vector2.zero)
        {
            direction = new Vector2(facingDirection, 0f);
        }

        return direction.normalized;
    }

    private Vector2 AdjustAimForGround(Vector2 aimDirection)
    {
        if (!isGrounded)
            return aimDirection;

        if (aimDirection.y < -0.25f && Mathf.Abs(aimDirection.x) < 0.25f)
        {
            return new Vector2(facingDirection, 0f);
        }

        return aimDirection;
    }

    // ----------------------------------------------------------------------
    // Animation
    // ----------------------------------------------------------------------

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // ----------------------------------------------------------------------
    // Baseball attack
    // ----------------------------------------------------------------------

    private void StartBaseballAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Debug.Log("Base attack animation started. Direction: " + pendingAttackDirection);
    }

    public void SpawnBaseballProjectile()
    {
        if (baseballProjectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("Baseball projectile prefab or spawn point not assigned.");
            return;
        }

        PlayAttackSound();

        Vector2 launchDirection = pendingAttackDirection;

        if (launchDirection == Vector2.zero)
        {
            launchDirection = new Vector2(facingDirection, 0f);
        }

        UpdateProjectileSpawnPointForDirection(launchDirection);

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

    private void UpdateProjectileSpawnPoint()
    {
        UpdateProjectileSpawnPointForDirection(lastAimDirection);
    }

    private void UpdateProjectileSpawnPointForDirection(Vector2 direction)
    {
        if (projectileSpawnPoint == null)
            return;

        float horizontalSign = direction.x != 0f
            ? Mathf.Sign(direction.x)
            : facingDirection;

        float xOffset = 0.6f * horizontalSign;
        float yOffset = 0.15f;

        if (direction.y > 0.25f)
        {
            yOffset = 0.45f;
        }
        else if (direction.y < -0.25f)
        {
            yOffset = -0.05f;
        }

        projectileSpawnPoint.localPosition = new Vector3(xOffset, yOffset, 0f);
    }

    private void PlayAttackSound()
    {
        if (audioSource != null && attackHitSound != null)
        {
            audioSource.PlayOneShot(attackHitSound, attackHitVolume);
        }
    }

    // ----------------------------------------------------------------------
    // Bash / deflect
    // ----------------------------------------------------------------------
    private void TryStartBash()
    {
        if (isBashing || Time.unscaledTime < nextBashTime)
            return;

        currentBashTarget = FindNearestBashableProjectile();

        if (currentBashTarget == null)
        {
            StartSpecialWithoutTarget();
            return;
        }

        isBashing = true;
        UpdateBashAimIndicator();

        if (animator != null)
        {
            animator.SetTrigger("Special");
        }

        currentBashTarget.HoldForBash();

        if (freezePlayerDuringBash)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        Time.timeScale = bashTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * bashTimeScale;

        Debug.Log("Bash started on: " + currentBashTarget.name);
    }

    private void ReleaseBash()
    {
        if (!isBashing)
            return;

        Vector2 aimDirection = GetCurrentAimDirection();
        Vector2 projectileDirection = -aimDirection;

        EndSlowMotion();

        rb.gravityScale = originalGravityScale;

        BashableProjectile releasedTarget = currentBashTarget;

        if (releasedTarget != null)
        {
            TemporarilyIgnoreProjectileCollision(releasedTarget);
            releasedTarget.ReleaseFromBash(projectileDirection, projectileBashSpeed);
        }

        rb.linearVelocity = aimDirection * playerBashBoostForce;
        movementLockedUntil = Time.time + bashMovementLockTime;

        isBashing = false;
        currentBashTarget = null;
        nextBashTime = Time.unscaledTime + bashCooldown;

        HideBashAimIndicator();

        Debug.Log("Bash released. Player: " + aimDirection + " Projectile: " + projectileDirection);
    }

    private BashableProjectile FindNearestBashableProjectile()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            bashDetectionRadius,
            bashableLayer
        );

        BashableProjectile nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            BashableProjectile bashable = hit.GetComponent<BashableProjectile>();

            if (bashable == null)
            {
                bashable = hit.GetComponentInParent<BashableProjectile>();
            }

            if (bashable == null)
            {
                bashable = hit.GetComponentInChildren<BashableProjectile>();
            }

            if (bashable == null || bashable.IsHeldByBash)
                continue;

            EnemyHealth enemyHealth = bashable.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = bashable.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null && enemyHealth.IsDead)
                continue;

            float distance = Vector2.Distance(transform.position, hit.bounds.center);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = bashable;
            }
        }

        return nearest;
    }

    private void StartSpecialWithoutTarget()
    {
        if (animator != null)
        {
            animator.SetTrigger("Special");
        }

        Debug.Log("Special: deflect placeholder.");
    }

    private Vector2 GetCurrentAimDirection()
    {
        Vector2 aimDirection = lastAimDirection;

        if (aimDirection == Vector2.zero)
        {
            aimDirection = new Vector2(facingDirection, 0f);
        }

        return aimDirection.normalized;
    }

    private void EndSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    private void ForceEndBashState()
    {
        EndSlowMotion();

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }

        if (currentBashTarget != null)
        {
            currentBashTarget.ReleaseFromBash(new Vector2(facingDirection, 0f), projectileBashSpeed);
        }

        currentBashTarget = null;
        isBashing = false;

        HideBashAimIndicator();
    }

    // ----------------------------------------------------------------------
    // Bash aim indicator
    // ----------------------------------------------------------------------

    private void UpdateBashAimIndicator()
    {
        if (!showAimIndicator || bashAimIndicator == null)
            return;

        if (!isBashing)
        {
            HideBashAimIndicator();
            return;
        }

        Vector2 aimDirection = GetCurrentAimDirection();

        bashAimIndicator.gameObject.SetActive(true);
        bashAimIndicator.localPosition = aimDirection * aimIndicatorDistance;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        bashAimIndicator.localRotation = Quaternion.Euler(
            0f,
            0f,
            angle + aimIndicatorRotationOffset
        );

        bashAimIndicator.localScale = new Vector3(
            Mathf.Abs(bashAimIndicator.localScale.x),
            Mathf.Abs(bashAimIndicator.localScale.y),
            1f
        );
    }

    private void HideBashAimIndicator()
    {
        if (bashAimIndicator != null)
        {
            bashAimIndicator.gameObject.SetActive(false);
        }
    }

    // ----------------------------------------------------------------------
    // Temporary projectile collision ignore
    // ----------------------------------------------------------------------
    private void TemporarilyIgnoreProjectileCollision(BashableProjectile bashable)
    {
        if (bashable == null)
            return;

        Collider2D[] projectileColliders = bashable.GetComponentsInChildren<Collider2D>();

        if (playerColliders == null || projectileColliders == null)
            return;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null)
                continue;

            foreach (Collider2D projectileCollider in projectileColliders)
            {
                if (projectileCollider == null)
                    continue;

                Physics2D.IgnoreCollision(playerCollider, projectileCollider, true);
            }
        }

        StartCoroutine(RestoreProjectileCollisionAfterDelay(projectileColliders));
    }
    private IEnumerator RestoreProjectileCollisionAfterDelay(Collider2D[] projectileColliders)
    {
        yield return new WaitForSeconds(ignoreBashedProjectileCollisionTime);

        if (playerColliders == null || projectileColliders == null)
            yield break;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null)
                continue;

            foreach (Collider2D projectileCollider in projectileColliders)
            {
                if (projectileCollider == null)
                    continue;

                Physics2D.IgnoreCollision(playerCollider, projectileCollider, false);
            }
        }
    }

    // ----------------------------------------------------------------------
    // External reset used by respawn system
    // ----------------------------------------------------------------------
    public void ResetMovementInput()
    {
        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravityScale;

        if (isBashing)
        {
            EndSlowMotion();

            if (currentBashTarget != null)
            {
                currentBashTarget.ReleaseFromBash(new Vector2(facingDirection, 0f));
            }

            currentBashTarget = null;
            isBashing = false;
        }

        HideBashAimIndicator();

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
        }
    }

    // ----------------------------------------------------------------------
    // Debug gizmos
    // ----------------------------------------------------------------------

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

        if (projectileSpawnPoint != null)
        {
            Gizmos.DrawWireSphere(projectileSpawnPoint.position, 0.1f);
        }

        Gizmos.DrawWireSphere(transform.position, bashDetectionRadius);
    }
}