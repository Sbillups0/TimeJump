using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeScript : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float stopBuffer = 0.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private int damage = 2;

    [Header("Facing")]
    [SerializeField] private bool spriteFacesRightByDefault = true;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0.6f, 0f);


    [Header("Animation")]

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;


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

    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > detectionRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toPlayer.normalized;

        rb.linearVelocity = direction * moveSpeed;

        FaceDirection(direction);

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth =
                collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
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

    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}