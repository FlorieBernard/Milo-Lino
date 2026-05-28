using UnityEngine;

/// <summary>
/// Shared base class for both playable cats.
/// Handles horizontal movement, jump, flip, ground detection and ice surfaces.
/// </summary>
public abstract class PlayerMovementBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float speed = 8f;
    [SerializeField] protected float jumpingPower = 16f;
    [Tooltip("Time window (seconds) during which the player can still jump after walking off a ledge.")]
    [SerializeField] private float _coyoteTime = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckRadius = 0.2f;

    [Header("Ice Surface")]
    [Tooltip("Lower = more slippery. Higher = less slippery.")]
    [SerializeField] private float _iceAcceleration = 2f;

    [Header("Visual Effects (VFX)")]
    [SerializeField] private ParticleSystem _runVFX;
    [SerializeField] private ParticleSystem _jumpVFX;

    protected float Horizontal { get; private set; }
    protected Rigidbody2D Rb => _rb;

    private bool           _isOnIce = false;
    private float          _currentHorizontalSpeed = 0f;
    private float          _coyoteTimer = 0f;
    private MovingPlatform _platform = null;

    /// <summary>True while the player may jump (grounded or within coyote window).</summary>
    protected bool CanJump => _coyoteTimer > 0f;

    protected virtual void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        UpdateCoyoteTimer();
        HandleJumpCut();
        HandleFlip();
        HandleRunVFX();
    }

    protected virtual void FixedUpdate()
    {
        float targetSpeed = Horizontal * speed;

        if (_isOnIce)
            _currentHorizontalSpeed = Mathf.Lerp(_currentHorizontalSpeed, targetSpeed, _iceAcceleration * Time.fixedDeltaTime);
        else
            _currentHorizontalSpeed = targetSpeed;

        _rb.linearVelocity = new Vector2(_currentHorizontalSpeed, _rb.linearVelocity.y);

        // Detect moving platform each frame via the same ground check.
        Collider2D groundCol = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        _platform = groundCol != null ? groundCol.GetComponentInParent<MovingPlatform>() : null;

        // Carry the character with the platform.
        if (_platform != null)
            _rb.MovePosition(_rb.position + _platform.Delta);
    }

    protected bool IsGrounded()
        => Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

    protected void TryJump()
    {
        _coyoteTimer = 0f; // consume the window so the player can't jump again mid-air
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpingPower);
        AudioManager.Instance?.Play("Jump");
        if (_jumpVFX != null) _jumpVFX.Play();
    }

    private void UpdateCoyoteTimer()
    {
        if (IsGrounded())
            _coyoteTimer = _coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ice"))
            _isOnIce = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ice"))
            _isOnIce = false;
    }

    private void HandleJumpCut()
    {
        if (Input.GetButtonUp("Jump") && _rb.linearVelocity.y > 0f)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f);
    }

    private void HandleFlip()
    {
        if (Horizontal == 0f) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(Horizontal) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void HandleRunVFX()
    {
        if (_runVFX == null) return;

        // Play the run particle system only when moving horizontally on the ground
        if (Mathf.Abs(Horizontal) > 0.1f && IsGrounded())
        {
            if (!_runVFX.isPlaying)
                _runVFX.Play();
        }
        else
        {
            if (_runVFX.isPlaying)
                _runVFX.Stop();
        }
    }
}
