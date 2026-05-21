using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Killzone : MonoBehaviour
{
    private void Awake()
    {
        Collider2D killzoneCollider = GetComponent<Collider2D>();
        killzoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Kill();
        }
    }
}