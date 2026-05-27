using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;

        // Rotate arrow to face the direction it's flying
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject, 5f);
        }
    }
}
