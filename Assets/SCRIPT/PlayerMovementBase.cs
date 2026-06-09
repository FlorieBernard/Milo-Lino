using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

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

    [Header("Footsteps")]
    [SerializeField] private float _footstepInterval = 0.35f;

    private float _footstepTimer;

    protected float Horizontal { get; private set; }
    protected Rigidbody2D Rb => _rb;

    private bool _isOnIce = false;
    private float _currentHorizontalSpeed = 0f;
    private float _coyoteTimer = 0f;
    private MovingPlatform _platform = null;

    /// <summary>True while the player may jump (grounded or within coyote window).</summary>
    protected bool CanJump => _coyoteTimer > 0f;

    [SerializeField] GameObject idleAnim, runAnim, jumpAnim, fallAnim, afraidAnim;

    [SerializeField] private ParticleSystem smokePrefab;
    [SerializeField] private Transform _vfxAnchor;

    public enum CatState {Idle,Running,Jumping,Falling,Afraid}
    CatState currentState;

    private void Awake()
    {
        EnterState(CatState.Idle);
    }

    public void EnterState(CatState newState)
    {
        ExitCurrentState();
        switch(newState)
        {
            case CatState.Idle:
                ShowAnim(idleAnim);
            break;
            case CatState.Running:
                ShowAnim(runAnim);
            break;
            case CatState.Jumping:
                ShowAnim(jumpAnim);
            break;
            case CatState.Falling:
                ShowAnim(fallAnim);
                break;
            case CatState.Afraid:
                ShowAnim(afraidAnim);
                break;
        }
        currentState = newState;
    }

    void UpdateCurrentState()
    {
        switch (currentState)
        {
            case CatState.Idle:
                if (Mathf.Abs(_currentHorizontalSpeed) > 0.01f) EnterState(CatState.Running);
                break;
            case CatState.Running:
                if (Mathf.Abs(_currentHorizontalSpeed) < 0.01f) EnterState(CatState.Idle);
                break;
            case CatState.Jumping:
                if (_rb.linearVelocity.y < 0) EnterState(CatState.Falling);
            break;
            case CatState.Falling:
                if (IsGrounded()) Landing();
            break;
            case CatState.Afraid: break;
        }
    }

    void ExitCurrentState()
    {
        switch (currentState)
        {
            case CatState.Idle: break;
            case CatState.Running: break;
            case CatState.Jumping: break;
            case CatState.Falling: break;
            case CatState.Afraid: break;
        }
    }


    void ShowAnim(GameObject desired)
    {
        idleAnim.SetActive(false);
        runAnim.SetActive(false);
        jumpAnim.SetActive(false);
        fallAnim.SetActive(false);
        afraidAnim.SetActive(false);
        desired.SetActive(true);
    }

    protected virtual void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        UpdateCoyoteTimer();
        HandleJumpCut();
        HandleFlip();
        HandleRunVFX();
        HandleFootsteps();
        UpdateCurrentState();
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

        // Carry the character with the platform (position offset, does not conflict with linearVelocity).
        if (_platform != null)
            _rb.position += _platform.Delta;
    }

    protected bool IsGrounded()
        => Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

    protected void TryJump()
    {
        EnterState(CatState.Jumping);
        _coyoteTimer = 0f; // consume the window so the player can't jump again mid-air
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpingPower);
        AudioManager.Instance?.Play("Jump");
        if (_jumpVFX != null) _jumpVFX.Play();
        ShowAnim(jumpAnim);
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
            //Debug.Log("Toto");
            if (!_runVFX.isPlaying)
                _runVFX.Play();
        }
        else
        {
            //Debug.Log("Toto2");
            if (_runVFX.isPlaying)
                _runVFX.Stop();
        }
    }



    void Landing()
    {
        EnterState(CatState.Running);
        SpawnSmoke();
        AudioManager.Instance?.Play("Land");
    }

    /// <summary>
    /// Triggers a footstep sound at regular intervals while the character
    /// is grounded and moving. Resets the timer when the character stops.
    /// </summary>
    private void HandleFootsteps()
    {
        if (IsGrounded() && Mathf.Abs(Horizontal) > 0.1f)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                PlayFootstep();
                _footstepTimer = _footstepInterval;
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }

    /// <summary>
    /// Detects the surface underfoot and plays a random clip from its FootstepSurface pool.
    /// Silent if the surface has no FootstepSurface component.
    /// </summary>
    private void PlayFootstep()
    {
        Collider2D ground = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        if (ground == null) return;

        FootstepSurface surface = ground.GetComponentInParent<FootstepSurface>();
        if (surface == null) return;

        AudioManager.Instance?.PlayOneShot(surface.GetRandom());
    }

    private void SpawnSmoke()
    {
        if (smokePrefab == null) return;
        Vector3 spawnPos = transform.position + new Vector3(0f, -0.5f, 0f);
        ParticleSystem smoke = Instantiate(smokePrefab, _vfxAnchor.position, Quaternion.identity);
        smoke.Play();
        Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
    }


}