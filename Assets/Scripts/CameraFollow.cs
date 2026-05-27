using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;      // Drag your player here
    public float smoothSpeed = 5f;
    public Vector2 offset;        // Adjust to position camera

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(
            player.position.x + offset.x,
            player.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
