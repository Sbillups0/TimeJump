using UnityEngine;

public class IPCatHealth : MonoBehaviour
{
    public Transform respawnPoint;

    public void Kill()
    {
        transform.position = respawnPoint.position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
