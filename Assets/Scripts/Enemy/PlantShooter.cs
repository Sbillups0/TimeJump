using System.Collections;
using UnityEngine;

public class PlantShooter : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float firstShotDelay = 0.5f;
    [SerializeField] private float projectileSpawnForwardOffset = 0.25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";
    [SerializeField] private bool useAnimationEventForProjectile = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootSoundVolume = 1f;

    private Coroutine shootRoutine;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        shootRoutine = StartCoroutine(ShootLoop());
    }

    private void OnDisable()
    {
        if (shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
            shootRoutine = null;
        }
    }

    private IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(firstShotDelay);

        while (true)
        {
            TriggerShoot();

            yield return new WaitForSeconds(shootInterval);
        }
    }

    private void TriggerShoot()
    {
        if (animator != null)
        {
            animator.SetTrigger(shootTriggerName);
        }

        if (!useAnimationEventForProjectile)
        {
            FireProjectile();
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning(name + " is missing projectilePrefab or firePoint.");
            return;
        }

        Vector2 shootDirection = firePoint.up.normalized;
        Vector3 spawnPosition = firePoint.position + (Vector3)(shootDirection * projectileSpawnForwardOffset);

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        PlantProjectile projectile = projectileObject.GetComponent<PlantProjectile>();

        if (projectile != null)
        {
            projectile.Launch(shootDirection, gameObject);

            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound, shootSoundVolume);
            }
        }
        else
        {
            Debug.LogWarning("Plant projectile prefab is missing PlantProjectile script.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = firePoint != null ? firePoint : transform;
        Vector3 shootDirection = origin.up.normalized;
        Vector3 spawnPosition = origin.position + shootDirection * projectileSpawnForwardOffset;

        Gizmos.DrawWireSphere(origin.position, 0.08f);
        Gizmos.DrawLine(origin.position, origin.position + shootDirection);
        Gizmos.DrawWireSphere(spawnPosition, 0.12f);
    }
}