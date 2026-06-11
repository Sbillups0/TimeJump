using UnityEngine;

public class BallProjectile : MonoBehaviour
{
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                health.TakeDamage(damage, knockbackDir);
            }

            Destroy(gameObject);
        }
    }
}
