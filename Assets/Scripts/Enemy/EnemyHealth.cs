using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private bool invincible = false;
    [SerializeField] private int maxHealth = 1;

    [Header("Death")]
    [SerializeField] private bool destroyImmediately = false;
    [SerializeField] private float destroyDelay = 0.7f;
    [SerializeField] private string deathTrigger = "Die";
    [SerializeField] private bool useDeathAnimation = true;
    [SerializeField] private bool disableCollidersOnDeath = true;
    [SerializeField] private bool stopMovementOnDeath = true;

    private int currentHealth;
    private bool isDead;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;
    private EnemyShooterAI enemyShooterAI;
    private PlantShooter plantShooter;

    public bool Invincible => invincible;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
        enemyShooterAI = GetComponent<EnemyShooterAI>();
        plantShooter = GetComponent<PlantShooter>();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log(name + " TakeDamage called: " + damage);

        if (isDead)
            return;

        if (invincible)
        {
            Debug.Log(name + " is invincible.");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(name + " HP: " + currentHealth + " / " + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(name + " died.");

        if (enemyShooterAI != null)
        {
            enemyShooterAI.enabled = false;
        }

        if (plantShooter != null)
        {
            plantShooter.enabled = false;
        }

        if (stopMovementOnDeath && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (disableCollidersOnDeath && colliders != null)
        {
            foreach (Collider2D col in colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        if (destroyImmediately)
        {
            Destroy(gameObject);
            return;
        }

        if (useDeathAnimation && animator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
        }

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}