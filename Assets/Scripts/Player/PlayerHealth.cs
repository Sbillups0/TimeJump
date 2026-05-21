using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Damage")]
    [SerializeField] private float invincibilityTime = 1f;

    [Header("Respawn")]
    [SerializeField] private SpawnManager spawnManager;

    private bool isInvincible;
    private SpriteRenderer spriteRenderer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spawnManager == null)
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
        }
    }

    public void TakeDamage(int damage)
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

        StartCoroutine(InvincibilityRoutine());
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
        {
            spawnManager.RespawnPlayer();
        }
        else
        {
            Debug.LogWarning("PlayerHealth has no SpawnManager assigned.");
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float timer = 0f;
        float flashInterval = 0.1f;

        while (timer < invincibilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }
}