using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Damage")]
    [SerializeField] private float invincibilityTime = 1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Respawn")]
    [SerializeField] private SpawnManager spawnManager;

    private bool isInvincible;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spawnManager == null)
            spawnManager = FindFirstObjectByType<SpawnManager>();
    }

    // Original method kept so teammate's code still works
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector2.zero);
    }

    // New overload for boss damage with knockback
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player HP: " + currentHealth + " / " + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (knockbackDirection != Vector2.zero)
            StartCoroutine(KnockbackRoutine(knockbackDirection));

        StartCoroutine(InvincibilityRoutine());
    }

    public void SetKnockedBack(bool value)
    {
        // handled by KnockbackRoutine but kept for Player.cs compatibility
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Player healed. HP: " + currentHealth + " / " + maxHealth);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        Debug.Log("Player health restored.");
    }

    public void Kill()
    {
        currentHealth = 0;
        Die();
    }

    private void Die()
    {
        Debug.Log("Player died.");

        if (spawnManager != null)
            spawnManager.RespawnPlayer();
        else
            Debug.LogWarning("PlayerHealth has no SpawnManager assigned.");
    }

    private IEnumerator KnockbackRoutine(Vector2 knockbackDirection)
    {
        Player player = GetComponent<Player>();
        if (player != null) player.SetKnockedBack(true);

        if (rb != null)
            rb.linearVelocity = knockbackDirection * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        if (player != null) player.SetKnockedBack(false);
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float timer = 0f;
        float flashInterval = 0.1f;

        while (timer < invincibilityTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvincible = false;
    }
}