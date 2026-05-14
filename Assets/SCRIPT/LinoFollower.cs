using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to Lino (the cat).
/// Inside "Corridor" trigger zones: Lino follows Milo with a slight delay.
/// On the ground → Lino keeps his own Y (normal gravity).
/// In the air → Lino also copies Y to follow the jump arc.
/// </summary>
public class LinoFollower : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform _milo;

    [Header("Follow Settings")]
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _followDelay = 0.3f;

    private bool _isInCorridor = false;
    private readonly Queue<Vector3> _positionHistory = new Queue<Vector3>();
    private float _timer = 0f;

    private Rigidbody2D _miloRb;
    private Rigidbody2D _linoRb;

    private void Start()
    {
        if (_milo != null)
            _miloRb = _milo.GetComponent<Rigidbody2D>();

        _linoRb = GetComponent<Rigidbody2D>();

        if (_linoRb != null)
            _linoRb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (_milo == null) return;

        bool miloIsAirborne = _miloRb != null && Mathf.Abs(_miloRb.linearVelocity.y) > 0.1f;

        _timer += Time.fixedDeltaTime;
        if (_timer >= _followDelay)
        {
            _positionHistory.Enqueue(_milo.position);
            _timer = 0f;
        }

        if (_isInCorridor && _positionHistory.Count > 0)
        {
            Vector3 targetPos = _positionHistory.Peek();

            if (!miloIsAirborne)
                targetPos.y = transform.position.y;

            targetPos.z = transform.position.z;

            float distance = Vector2.Distance(transform.position, targetPos);

            if (distance > _minDistance)
            {
                Vector2 newPos = Vector2.MoveTowards(transform.position, targetPos, _speed * Time.fixedDeltaTime);
                _linoRb.MovePosition(newPos);

                float scaleX = targetPos.x < transform.position.x
                    ? -Mathf.Abs(transform.localScale.x)
                    : Mathf.Abs(transform.localScale.x);
                transform.localScale = new Vector3(scaleX, transform.localScale.y, 1f);
            }
            else
            {
                _positionHistory.Dequeue();
            }
        }
        else if (!_isInCorridor)
        {
            _positionHistory.Clear();
            _timer = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Corridor"))
            _isInCorridor = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Corridor"))
            _isInCorridor = false;
    }
}
