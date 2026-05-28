using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum Direction { Horizontal, Vertical }

    [Header("Movement Direction")]
    [SerializeField] private Direction _direction = Direction.Horizontal;

    [Header("Settings")]
    [SerializeField] private float _distance = 3f;
    [SerializeField] private float _speed = 2f;

    /// <summary>
    /// Position change applied this physics step.
    /// Read by PlayerMovementBase to carry characters standing on the platform.
    /// </summary>
    public Vector2 Delta { get; private set; }

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
        Delta = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Vector3 prev = transform.position;
        float offset = Mathf.Sin(Time.time * _speed) * _distance;
        transform.position = _direction == Direction.Horizontal
            ? _startPosition + new Vector3(offset, 0f, 0f)
            : _startPosition + new Vector3(0f, offset, 0f);
        Delta = transform.position - prev;
    }
}
