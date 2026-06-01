using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float speed = 5f;
    public float jumpForce = 5f;
    public bool isGrounded;
    public bool isHovering;
    public float hoverTime;
    public bool isJumping = false;
    public float maxHoverTime = 4f;
    private Vector2 moveInput;
    private bool jumpPressed;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip landingSound;


    private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    private SpriteRenderer spriteRenderer;

    public static float ExponentialLerp(float current, float target, float lambda, float dt)
    {
        return Mathf.Lerp(current, target, 1 - Mathf.Exp(-lambda * dt));
    }
    void Hover()
    {
        float maxHover = 0.5f;
        float minHover = 1f;
        if (Input.GetKey(KeyCode.Space) && hoverTime > 0)
        {
            isHovering = true;
            hoverTime = Mathf.Max(0, hoverTime - Time.deltaTime);
            rb.gravityScale = ExponentialLerp(minHover, maxHover, maxHoverTime, hoverTime);
        }
        else
        {
            isHovering = false;
            rb.gravityScale = 1f;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        bool isMoving = moveInput.sqrMagnitude > 0.01f; // better than Abs(x)

        animator.SetBool("isRunning", isMoving);

        if (!isMoving)
        {
            // character stopped moving
            animator.SetBool("isRunning", false);
            return;
        }

        if (moveInput.x > 0.1f)
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x < -0.1f)
        {
            spriteRenderer.flipX = false;
        }
    }

    public void OnJump (InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
        }
    }

    void Start()
    {
        hoverTime = maxHoverTime;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    //audio stuff
        if (audioSource == null){
            audioSource = GetComponent<AudioSource>();
        }
    
    }

    // Update is called once per frame
    void Update()
    {
        
       
    }
// Called Per Physics Update
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

    if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;

            if(jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

    jumpPressed = false; 
        Hover();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if(!isGrounded && landingSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(landingSound);
            }

            isGrounded = true;
            hoverTime = maxHoverTime; // Reset hover time when landing
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}

// bool ifHover X
// do hover time as a var, resets when you land
//gets consumed per second, delta time X
//check if space 

// fn hover X 
/*
 * for check if space --- do in fixed update
 * true: gravity decaty
 * false: return gravity to normal
 * 
 * 
 * 
 * 
 * 
 * 
 */

//gravity decay, curve from 0-1, 1 being the most hovering, 0 being the least. curve is based on hover time, so the longer you hover, X
//the more gravity decays, but it will never fully decay. when you land, it resets to 0. X
// do something similar to speed X
// can set own exponential graph for the decay X

// TO DO:
// if i wanna do the collision where it decreases hover time I do a collision check
// on collision it decreases their hovering time
// display of hover time?