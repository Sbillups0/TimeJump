using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public bool startMovingRight = false;

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Edge Check")]
    public Transform edgeCheck;
    public float edgeCheckDistance = 0.8f;

    [Header("Flip Settings")]
    public float flipCooldown = 0.2f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int moveDirection = -1;
    private float lastFlipTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        moveDirection = startMovingRight ? 1 : -1;

        MoveChecksToFront();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

        bool hitWall = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * moveDirection,
            wallCheckDistance,
            groundLayer
        );

        bool groundAhead = Physics2D.Raycast(
            edgeCheck.position,
            Vector2.down,
            edgeCheckDistance,
            groundLayer
        );

        if ((hitWall || !groundAhead) && Time.time >= lastFlipTime + flipCooldown)
        {
            Flip();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collision2D collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                return;
            }
        }

        playerHealth.TakeDamage(damage);
    }

    private void Flip()
    {
        lastFlipTime = Time.time;

        moveDirection *= -1;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection > 0;
        }

        MoveChecksToFront();
    }

    private void MoveChecksToFront()
    {
        if (wallCheck != null)
        {
            Vector3 wallLocalPos = wallCheck.localPosition;
            wallLocalPos.x = Mathf.Abs(wallLocalPos.x) * moveDirection;
            wallCheck.localPosition = wallLocalPos;
        }

        if (edgeCheck != null)
        {
            Vector3 edgeLocalPos = edgeCheck.localPosition;
            edgeLocalPos.x = Mathf.Abs(edgeLocalPos.x) * moveDirection;
            edgeCheck.localPosition = edgeLocalPos;
        }
    }

    private void OnDrawGizmos()
    {
        int gizmoDirection = startMovingRight ? 1 : -1;

        if (Application.isPlaying)
        {
            gizmoDirection = moveDirection;
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + Vector3.right * gizmoDirection * wallCheckDistance
            );
        }

        if (edgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                edgeCheck.position,
                edgeCheck.position + Vector3.down * edgeCheckDistance
            );
        }
    }
}