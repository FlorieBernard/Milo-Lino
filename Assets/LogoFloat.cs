using UnityEngine;

public class LogoFloat : MonoBehaviour
{
    [Header("Flottement")]
    public float amplitude = 20f;
    public float speed = 1.5f;

    [Header("Rotation")]
    public float rotationAmount = 3f;
    public float rotationSpeed = 1.2f;

    [Header("Pulsation scale")]
    public float scaleAmount = 0.04f;
    public float scaleSpeed = 1.8f;

    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        startPos   = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        float t = Time.time;

        float offsetY = Mathf.Sin(t * speed) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, offsetY, 0f);

        float angle = Mathf.Sin(t * rotationSpeed) * rotationAmount;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        float scaleMult = 1f + Mathf.Sin(t * scaleSpeed) * scaleAmount;
        transform.localScale = startScale * scaleMult;
    }
}