using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Checkpoint Order")]
    [SerializeField] private int checkpointIndex;

    [Header("Visuals")]
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;
    
    [Header("Audio")]
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioSource audioSource;

    private bool isActivated;

    public int CheckpointIndex => checkpointIndex;

    private void Awake()
    {
        Collider2D checkpointCollider = GetComponent<Collider2D>();
        checkpointCollider.isTrigger = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        SetVisual(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        if (spawnManager == null)
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        if (spawnManager == null)
        {
            Debug.LogWarning("No SpawnManager found in scene.");
            return;
        }

        bool accepted = spawnManager.TrySetCheckpoint(this);

        if (!accepted)
            return;

        isActivated = true;
        SetVisual(true);

        PlayCheckpointSound();

        Debug.Log("Checkpoint activated: " + name);
    }

    private void PlayCheckpointSound()
    {
        if (audioSource == null || checkpointSound == null)
            return;

        audioSource.PlayOneShot(checkpointSound);
    }

    private void SetVisual(bool active)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = active ? activeColor : inactiveColor;
    }
}