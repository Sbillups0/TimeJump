using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BashableProjectile : MonoBehaviour
{
    [SerializeField] private float redirectedSpeed = 16f;

    private Rigidbody2D rb;
    private Vector2 storedVelocity;
    private bool isHeldByBash;

    public Rigidbody2D Rigidbody => rb;
    public bool IsHeldByBash => isHeldByBash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void HoldForBash()
    {
        if (isHeldByBash)
            return;

        isHeldByBash = true;

        storedVelocity = rb.linearVelocity;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Debug.Log("Projectile held for bash: " + name);
    }

    public void ReleaseFromBash(Vector2 direction, float speedOverride = -1f)
    {
        isHeldByBash = false;

        if (direction == Vector2.zero)
        {
            direction = storedVelocity.sqrMagnitude > 0.01f
                ? storedVelocity.normalized
                : Vector2.right;
        }

        float finalSpeed = speedOverride > 0f ? speedOverride : redirectedSpeed;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = direction.normalized * finalSpeed;

        Debug.Log("Projectile released from bash. Direction: " + direction.normalized + " Speed: " + finalSpeed);
    }

    public void MoveSlightly(Vector2 direction, float distance)
    {
        if (direction == Vector2.zero)
            return;

        transform.position += (Vector3)(direction.normalized * distance);
    }
}