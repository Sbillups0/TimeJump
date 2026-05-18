using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float spinSpeed = 720f;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField, Range(0f, 1f)] private float bounceVolume = 1f;
    [SerializeField] private float bounceSoundCooldown = 0.05f;

    private float lastBounceSoundTime;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        float spinDirection = direction.x >= 0f ? -1f : 1f;
        transform.Rotate(0f, 0f, spinDirection * spinSpeed * Time.deltaTime);
    }

    public void Launch(Vector2 launchDirection)
    {
        if (launchDirection == Vector2.zero)
        {
            launchDirection = Vector2.right;
        }

        direction = launchDirection.normalized;
        rb.linearVelocity = direction * speed;

        Debug.Log("Baseball velocity: " + rb.linearVelocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            return;
        }
        //Enemy hit logic can be added here
        if (collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Baseball hit enemy: " + collision.collider.name);
            Destroy(gameObject);
            // Example: Apply damage to enemy here
        }
        PlayBounceSound();
    }
    private void PlayBounceSound()
    {
        if (audioSource == null || bounceSound == null)
        {
            return;
        }

        if (Time.time < lastBounceSoundTime + bounceSoundCooldown)
        {
            return;
        }

        lastBounceSoundTime = Time.time;
        audioSource.PlayOneShot(bounceSound, bounceVolume);
    }
}