using UnityEngine;

public class IPCatHealth : MonoBehaviour
{
    public Transform respawnPoint;

    public void Kill()
    {
        // Respawn all enemies
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RespawnAllEnemies();

        // Reset player position and velocity
        transform.position = respawnPoint.position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Reset player state
        Player player = GetComponent<Player>();
        if (player != null)
            player.ResetState();
    }
}
