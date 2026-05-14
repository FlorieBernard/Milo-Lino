using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum Direction { Horizontal, Vertical }

    [Header("Movement Direction")]
    [SerializeField] private Direction _direction = Direction.Horizontal;

    [Header("Settings")]
    [SerializeField] private float _distance = 3f;
    [SerializeField] private float _speed = 2f;

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * _speed) * _distance;
        transform.position = _direction == Direction.Horizontal
            ? _startPosition + new Vector3(offset, 0f, 0f)
            : _startPosition + new Vector3(0f, offset, 0f);
    }
}
