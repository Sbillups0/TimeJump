using UnityEngine;

public class ArcherEnemy : MonoBehaviour
{
    public float detectionRange = 10f;
    public float fireRate = 2f;
    public GameObject arrowPrefab;
    public Transform firePoint;

    private Transform player;
    private float fireTimer;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        fireTimer = fireRate;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                ShootArrow();
                fireTimer = fireRate;
            }
        }
    }

    void ShootArrow()
    {
        if (arrowPrefab == null || firePoint == null) return;

        Vector3 direction = (player.position - firePoint.position).normalized;
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        ArrowProjectile ap = arrow.GetComponent<ArrowProjectile>();

        if (ap != null)
            ap.SetDirection(direction);
    }

    public void ResetState()
    {
        player = GameObject.FindWithTag("Player").transform;
        fireTimer = fireRate;
    }
}
