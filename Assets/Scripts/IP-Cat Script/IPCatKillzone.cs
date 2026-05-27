using UnityEngine;

public class IPCatKillzone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IPCatHealth health = other.GetComponent<IPCatHealth>();
            if (health != null)
                health.Kill();
        }
    }
}