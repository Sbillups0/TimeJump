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

    private Rigidbody2D rb;

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
        }

    jumpPressed = false; 
        Hover();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
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