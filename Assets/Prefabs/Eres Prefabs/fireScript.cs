using UnityEngine;

public class fireScript : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifetime = 3f;
    private Rigidbody2D rb;

    [SerializeField] private int damage = 3;

    private float direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float dir)
    {
        direction = dir;
        rb.linearVelocity = new Vector2(direction * speed, 0f);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit");

        EnemyHealth enemy = collision.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
        }
    }

    
}
