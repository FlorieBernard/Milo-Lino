using UnityEngine;

/// <summary>
/// Moves a platform back and forth sinusoidally.
/// Requires a Kinematic Rigidbody2D on the same GameObject.
///
/// Inspector setup:
///   • Rigidbody2D → Body Type = Kinematic, Gravity Scale = 0, Freeze Rotation ✅
///   • Platform layer must be included in the character's _groundLayer mask
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public enum Direction { Horizontal, Vertical }

    [Header("Movement Direction")]
    [SerializeField] private Direction _direction = Direction.Horizontal;

    [Header("Settings")]
    [SerializeField] private float _distance = 3f;
    [SerializeField] private float _speed = 2f;

    /// <summary>Position delta applied this physics step. Read by PlayerMovementBase.</summary>
    public Vector2 Delta { get; private set; }

    private Rigidbody2D _rb;
    private Vector3     _startPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType     = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        _startPosition = transform.position;
        Delta = Vector2.zero;
    }

    private void FixedUpdate()
    {
        float offset = Mathf.Sin(Time.time * _speed) * _distance;
        Vector2 target = _direction == Direction.Horizontal
            ? (Vector2)_startPosition + new Vector2(offset, 0f)
            : (Vector2)_startPosition + new Vector2(0f, offset);

        Delta = target - _rb.position;
        _rb.MovePosition(target);
    }
}
