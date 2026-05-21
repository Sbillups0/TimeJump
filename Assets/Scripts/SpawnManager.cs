using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Checkpoints")]
    [SerializeField] private Transform initialSpawnPoint;
    [SerializeField] private Transform currentCheckpoint;
    [SerializeField] private int currentCheckpointIndex = 0;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 0.25f;
    [SerializeField] private bool resetPlayerVelocity = true;

    private bool isRespawning;

    private void Awake()
    {
        if (currentCheckpoint == null)
        {
            currentCheckpoint = initialSpawnPoint;
        }

        if (player != null && playerRigidbody == null)
        {
            playerRigidbody = player.GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        if (player != null && currentCheckpoint != null)
        {
            RespawnPlayerImmediately();
        }
    }

    public bool TrySetCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
            return false;

        if (checkpoint.CheckpointIndex < currentCheckpointIndex)
            return false;

        currentCheckpointIndex = checkpoint.CheckpointIndex;
        currentCheckpoint = checkpoint.transform;

        Debug.Log("Checkpoint set: " + checkpoint.name);
        return true;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null)
            return;

        currentCheckpoint = checkpoint;
        Debug.Log("Checkpoint set: " + checkpoint.name);
    }

    public void RespawnPlayer()
    {
        if (isRespawning)
            return;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        yield return new WaitForSeconds(respawnDelay);

        RespawnPlayerImmediately();

        isRespawning = false;
    }

    private void RespawnPlayerImmediately()
    {
        if (player == null)
        {
            Debug.LogWarning("SpawnManager has no player assigned.");
            return;
        }

        if (currentCheckpoint == null)
        {
            Debug.LogWarning("SpawnManager has no checkpoint assigned.");
            return;
        }

        if (resetPlayerVelocity && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        player.position = currentCheckpoint.position;

        Debug.Log("Player respawned at: " + currentCheckpoint.name);
    }
}