using UnityEngine;

public class TextFloat : MonoBehaviour
{
    [Header("Flottement")]
    public float amplitude = 12f;        // un peu moins que le logo
    public float speed = 1.5f;

    [Header("Rotation")]
    public float rotationAmount = 2f;
    public float rotationSpeed = 1.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float t = Time.time;

        float offsetY = Mathf.Sin(t * speed) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, offsetY, 0f);

        float angle = Mathf.Sin(t * rotationSpeed) * rotationAmount;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}