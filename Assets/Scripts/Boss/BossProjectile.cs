using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
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
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
                Debug.Log("Player hit by projectile for " + damage + " damage!");
            }
            Destroy(gameObject);
        }

        if (other.CompareTag("Ground"))
            Destroy(gameObject);
    }
}
