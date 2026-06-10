using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 10;
    public float lifetime = 5f;
    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IPCatHealth playerHealth = other.GetComponent<IPCatHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
                Debug.Log("Player hit by projectile for " + damage + " damage!");
            }
            Destroy(gameObject);
        }

        // Destroy on hitting ground
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
