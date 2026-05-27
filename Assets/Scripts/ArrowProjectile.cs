using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;

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
            IPCatHealth health = other.GetComponent<IPCatHealth>();
            if (health != null)
                health.Kill();

            Destroy(gameObject);
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject, 5f);
        }
    }
}
