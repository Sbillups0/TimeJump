using UnityEngine;

public class ArcherEnemy : MonoBehaviour
{
    public float detectionRange = 10f;  // How far the archer can see
    public float fireRate = 2f;         // Seconds between shots
    public GameObject arrowPrefab;      // Drag your Arrow prefab here
    public Transform firePoint;         // Where the arrow spawns from

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
}
