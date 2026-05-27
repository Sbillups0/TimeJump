using UnityEngine;
 
public class BallProjectile : MonoBehaviour
{
    void Start()
    {
        // Destroy the ball after 5 seconds so it doesn't pile up forever
        Destroy(gameObject, 5f);
    }
 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kill the player
            Destroy(other.gameObject);
 
            // Destroy the ball too
            Destroy(gameObject);
        }
    }
}
