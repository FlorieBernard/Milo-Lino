using UnityEngine;

public class LinoBlocker : MonoBehaviour
{
    public bool IsBlocked { get; private set; } = true;

    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (IsBlocked && _rb != null)
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    public void Unblock()
    {
        IsBlocked = false;
    }
}