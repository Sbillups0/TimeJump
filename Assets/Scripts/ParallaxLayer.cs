using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float parallaxFactor = 0.15f;
    [SerializeField] private bool parallaxY = false;

    [Header("Vertical Behavior")]
    [SerializeField] private bool followCameraY = false;
    [SerializeField] private float yOffset = 0f;

    [Header("Looping")]
    [SerializeField] private bool loopHorizontally = true;

    [Header("Sprite Layout")]
    [SerializeField] private Vector2 backgroundScale = Vector2.one;
    [SerializeField] private float overlap = 0.02f;

    private Vector3 lastCameraPosition;
    private float spriteWidth;

    private void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        lastCameraPosition = cameraTransform.position;

        ApplyChildScale();
        CalculateSpriteWidth();
        ArrangeChildren();
    }

    private void OnValidate()
    {
        ApplyChildScale();
        CalculateSpriteWidth();
        ArrangeChildren();
    }

    private void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - lastCameraPosition;

        float moveX = delta.x * parallaxFactor;
        float moveY = parallaxY ? delta.y * parallaxFactor : 0f;

        transform.position += new Vector3(moveX, moveY, 0f);

        if (followCameraY)
        {
            Vector3 position = transform.position;
            position.y = cameraTransform.position.y + yOffset;
            transform.position = position;
        }

        lastCameraPosition = cameraTransform.position;

        if (loopHorizontally)
        {
            LoopHorizontally();
        }
    }

    private void ApplyChildScale()
    {
        if (transform.childCount == 0)
            return;

        foreach (Transform child in transform)
        {
            child.localScale = new Vector3(
                backgroundScale.x,
                backgroundScale.y,
                1f
            );
        }
    }

    private void CalculateSpriteWidth()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();

        if (renderer == null)
        {
            spriteWidth = 0f;
            return;
        }

        spriteWidth = renderer.bounds.size.x;
    }

    private void ArrangeChildren()
    {
        if (!loopHorizontally || spriteWidth <= 0f)
            return;

        if (transform.childCount < 3)
            return;

        float spacing = spriteWidth - overlap;

        transform.GetChild(0).localPosition = new Vector3(-spacing, 0f, 0f);
        transform.GetChild(1).localPosition = new Vector3(0f, 0f, 0f);
        transform.GetChild(2).localPosition = new Vector3(spacing, 0f, 0f);
    }

    private void LoopHorizontally()
    {
        if (spriteWidth <= 0f)
            return;

        float cameraX = cameraTransform.position.x;
        float layerX = transform.position.x;

        float distanceFromCamera = cameraX - layerX;

        if (distanceFromCamera > spriteWidth)
        {
            transform.position += new Vector3(spriteWidth, 0f, 0f);
        }
        else if (distanceFromCamera < -spriteWidth)
        {
            transform.position -= new Vector3(spriteWidth, 0f, 0f);
        }
    }
}