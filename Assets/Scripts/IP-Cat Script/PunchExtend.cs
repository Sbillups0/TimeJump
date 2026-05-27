using UnityEngine;

public class PunchExtend : MonoBehaviour
{
    public float duration = 0.3f;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    public void Launch(float length, bool facingRight)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = !facingRight;

        transform.localScale = new Vector3(length, 1f, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}