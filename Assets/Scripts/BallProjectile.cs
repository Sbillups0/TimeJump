using UnityEngine;

public class BallProjectile : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 5f);
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
    }
}
