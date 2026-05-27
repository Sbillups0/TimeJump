using UnityEngine;

public class BirdEnemy : MonoBehaviour
{
    public float speed = 3f;
    public float leftPoint = -2f;
    public float rightPoint = 8f;
    public GameObject ballPrefab;
    public float dropInterval = 1f;

    private bool movingRight = true;
    private float dropTimer;
    private SpriteRenderer sr;

    void Start()
    {
        dropTimer = dropInterval;
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            sr.flipX = true;

            if (transform.position.x >= rightPoint)
                movingRight = false;
        }
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            sr.flipX = false;

            if (transform.position.x <= leftPoint)
                movingRight = true;
        }

        dropTimer -= Time.deltaTime;
        if (dropTimer <= 0f)
        {
            DropBall();
            dropTimer = dropInterval;
        }
    }

    void DropBall()
    {
        if (ballPrefab != null)
        {
            Vector3 spawnPos = new Vector3(
                sr.transform.position.x,
                sr.transform.position.y - 0.3f,
                0f);
            Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        }
    }
}
