using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Killzone : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;

    private void Awake()
    {
        Collider2D killzoneCollider = GetComponent<Collider2D>();
        killzoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (spawnManager == null)
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        if (spawnManager != null)
        {
            spawnManager.RespawnPlayer();
        }
        else
        {
            Debug.LogWarning("No SpawnManager found in scene.");
        }
    }
}