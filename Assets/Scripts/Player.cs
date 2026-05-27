using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public AudioClip jumpSound;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float moveInput;
    public ParticleSystem landingParticles;
    private bool wasGrounded;

    [Header("Punch")]
    public Transform punchPoint;
    public float punchRange = 0.8f;
    public float punchDamage = 10f;
    public LayerMask enemyLayer;
    private float punchCooldown = 0.4f;
    private float punchTimer = 0f;
    private Animator anim;

    [Header("Charge Punch")]
    public float maxChargeTime = 3f;
    public float maxChargeRange = 3f;
    public Sprite[] chargeFrames;
    public Sprite punchFrame;
    public float frameDuration = 0.25f;
    public float holdThreshold = 0.2f;
    public GameObject punchExtendPrefab;
    public float minPunchLength = 0.5f;
    public float maxPunchLength = 5f;
    private float chargeTime = 0f;
    private bool isCharging = false;
    private bool chargingStarted = false;
    private float frameTimer = 0f;
    private int currentFrame = 0;
    private SpriteRenderer sr;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Keyboard.current != null ?
            (Keyboard.current.dKey.isPressed ? 1f :
             Keyboard.current.aKey.isPressed ? -1f : 0f) : 0f;

        if (moveInput > 0) facingRight = true;
        if (moveInput < 0) facingRight = false;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
            audioSource.PlayOneShot(jumpSound);
        }

        punchTimer = Mathf.Max(0f, punchTimer - Time.deltaTime);

        if (Keyboard.current.pKey.wasPressedThisFrame && punchTimer <= 0f)
        {
            chargingStarted = true;
            chargeTime = 0f;
            currentFrame = 0;
            frameTimer = 0f;
        }

        if (chargingStarted && Keyboard.current.pKey.isPressed)
        {
            chargeTime += Time.deltaTime;

            if (chargeTime >= holdThreshold && !isCharging)
            {
                isCharging = true;
                anim.enabled = false;
                if (chargeFrames.Length > 0)
                    sr.sprite = chargeFrames[0];
            }

            if (isCharging)
            {
                chargeTime = Mathf.Min(chargeTime, maxChargeTime);
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameDuration)
                {
                    frameTimer = 0f;
                    if (currentFrame < chargeFrames.Length - 1)
                    {
                        currentFrame++;
                        sr.sprite = chargeFrames[currentFrame];
                    }
                }
            }
        }

        if (chargingStarted && Keyboard.current.pKey.wasReleasedThisFrame)
        {
            if (isCharging)
            {
                isCharging = false;
                chargingStarted = false;
                punchTimer = punchCooldown;

                if (punchFrame != null)
                    sr.sprite = punchFrame;

                float chargePercent = chargeTime / maxChargeTime;
                float punchLength = Mathf.Lerp(minPunchLength, maxPunchLength, chargePercent);

                if (punchExtendPrefab != null)
                {
                    float direction = facingRight ? 1f : -1f;

                    Vector3 spawnPos = transform.position + new Vector3(
                        direction * (punchLength / 2f), 0f, 0f);

                    GameObject punch = Instantiate(
                        punchExtendPrefab, spawnPos, Quaternion.identity);

                    PunchExtend pe = punch.GetComponent<PunchExtend>();
                    if (pe != null)
                        pe.Launch(punchLength, facingRight);
                }

                Invoke("ResetToIdle", 0.5f);
            }
            else
            {
                isCharging = false;
                chargingStarted = false;
                punchTimer = punchCooldown;
                anim.SetTrigger("Punch");

                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    punchPoint.position, punchRange);

                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Enemy"))
                    {
                        Destroy(hit.gameObject);
                    }
                }
            }
        }

        bool isGrounded = Physics2D.OverlapCircle(
            new Vector2(transform.position.x, transform.position.y - 0.5f),
            0.1f,
            LayerMask.GetMask("Ground")
        );

        if (isGrounded && !wasGrounded)
        {
            landingParticles.Play();
        }
        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void ResetToIdle()
    {
        anim.enabled = true;
        anim.Play("Idle");
    }

    void OnDrawGizmosSelected()
    {
        if (punchPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchPoint.position, punchRange);
    }
}