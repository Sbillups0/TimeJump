using System.Collections;
using System.Reflection;
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
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Character Switching")]
    [SerializeField] private GameObject switcher;
    [SerializeField] private bool restoreAllCharacterHealth = true;
    [SerializeField] private bool moveAllCharactersToCheckpoint = false;

    private bool isRespawning;

    private void Awake()
    {
        if (currentCheckpoint == null)
        {
            currentCheckpoint = initialSpawnPoint;
        }

        RefreshActivePlayerReferences();
    }

    private void Start()
    {
        if (currentCheckpoint != null)
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

        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.FadeToBlack();
        }

        yield return new WaitForSeconds(respawnDelay);

        RespawnPlayerImmediately();

        if (SceneTransitionManager.Instance != null)
        {
            yield return SceneTransitionManager.Instance.FadeFromBlack();
        }

        isRespawning = false;
    }

    private void RespawnPlayerImmediately()
    {
        RefreshActivePlayerReferences();

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

        if (moveAllCharactersToCheckpoint)
        {
            MoveAllSwitcherCharactersToCheckpoint();
        }
        else
        {
            MoveCharacterToCheckpoint(player.gameObject);
        }

        if (restoreAllCharacterHealth)
        {
            RestoreAllSwitcherCharacterHealth();
        }
        else
        {
            RestoreHealth(player.gameObject);
        }

        RefreshActivePlayerReferences();

        Debug.Log("Player respawned at: " + currentCheckpoint.name);
    }

    private void RefreshActivePlayerReferences()
    {
        Transform activePlayer = GetActiveSwitcherCharacter();

        if (activePlayer != null)
        {
            player = activePlayer;
        }

        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody2D>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private Transform GetActiveSwitcherCharacter()
    {
        if (switcher == null)
            return null;

        MonoBehaviour[] scripts = switcher.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
                continue;

            GameObject activeObject = TryGetActiveCharacterFromScript(script);

            if (activeObject != null)
            {
                return activeObject.transform;
            }
        }

        return null;
    }

    private GameObject TryGetActiveCharacterFromScript(MonoBehaviour script)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo lowerField = script.GetType().GetField("activeCharacter", flags);

        if (lowerField != null && lowerField.FieldType == typeof(GameObject))
        {
            GameObject value = lowerField.GetValue(script) as GameObject;

            if (value != null)
            {
                return value;
            }
        }

        FieldInfo upperField = script.GetType().GetField("ActiveCharacter", flags);

        if (upperField != null && upperField.FieldType == typeof(GameObject))
        {
            GameObject value = upperField.GetValue(script) as GameObject;

            if (value != null)
            {
                return value;
            }
        }

        PropertyInfo lowerProperty = script.GetType().GetProperty("activeCharacter", flags);

        if (lowerProperty != null && lowerProperty.PropertyType == typeof(GameObject))
        {
            GameObject value = lowerProperty.GetValue(script) as GameObject;

            if (value != null)
            {
                return value;
            }
        }

        PropertyInfo upperProperty = script.GetType().GetProperty("ActiveCharacter", flags);

        if (upperProperty != null && upperProperty.PropertyType == typeof(GameObject))
        {
            GameObject value = upperProperty.GetValue(script) as GameObject;

            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private void MoveCharacterToCheckpoint(GameObject character)
    {
        if (character == null || currentCheckpoint == null)
            return;

        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();

        if (resetPlayerVelocity && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        character.transform.position = currentCheckpoint.position;

        ResetCharacterMovement(character);
    }

    private void RestoreHealth(GameObject character)
    {
        if (character == null)
            return;

        PlayerHealth health = character.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.RestoreFullHealth();
        }
    }

    private void ResetCharacterMovement(GameObject character)
    {
        if (character == null)
            return;

        character.SendMessage("ResetMovementInput", SendMessageOptions.DontRequireReceiver);
    }

    private void MoveAllSwitcherCharactersToCheckpoint()
    {
        if (switcher == null || currentCheckpoint == null)
        {
            MoveCharacterToCheckpoint(player != null ? player.gameObject : null);
            return;
        }

        PlayerHealth[] allHealthComponents = switcher.GetComponentsInChildren<PlayerHealth>(true);

        if (allHealthComponents == null || allHealthComponents.Length == 0)
        {
            MoveCharacterToCheckpoint(player != null ? player.gameObject : null);
            return;
        }

        foreach (PlayerHealth health in allHealthComponents)
        {
            if (health == null)
                continue;

            MoveCharacterToCheckpoint(health.gameObject);
        }
    }

    private void RestoreAllSwitcherCharacterHealth()
    {
        if (switcher == null)
        {
            RestoreHealth(player != null ? player.gameObject : null);
            return;
        }

        PlayerHealth[] allHealthComponents = switcher.GetComponentsInChildren<PlayerHealth>(true);

        if (allHealthComponents == null || allHealthComponents.Length == 0)
        {
            RestoreHealth(player != null ? player.gameObject : null);
            return;
        }

        foreach (PlayerHealth health in allHealthComponents)
        {
            if (health != null)
            {
                health.RestoreFullHealth();
            }
        }
    }
}