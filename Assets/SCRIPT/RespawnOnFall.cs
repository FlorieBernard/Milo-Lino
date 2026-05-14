using UnityEngine;

public class RespawnOnFall : MonoBehaviour
{
    [SerializeField] private float _deathHeight = -10f;

    private Vector3 _spawnPosition;
    private Rigidbody2D _rb;

    private void Start()
    {
        _spawnPosition = transform.position;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (transform.position.y < _deathHeight)
            Respawn();
    }

    private void Respawn()
    {
        transform.position = _spawnPosition;
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }
}