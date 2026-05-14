using System.Collections;
using UnityEngine;

public class PlayerMovementMilo : PlayerMovementBase
{
    [SerializeField] private ParticleSystem smokePrefab;

    private bool _wasGrounded = true;
    private bool _canJump = true;
    private bool _removeJumpInvoked = false;

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && _canJump)
            TryJump();

        UpdateGroundState();
    }

    private void UpdateGroundState()
    {
        if (!IsGrounded())
        {
            if (_canJump && !_removeJumpInvoked)
            {
                _removeJumpInvoked = true;
                Invoke(nameof(DisableJump), 0.5f);
            }
            _wasGrounded = false;
        }
        else
        {
            _removeJumpInvoked = false;

            if (!_wasGrounded)
            {
                SpawnSmoke();
                AudioManager.Instance?.Play("Land");
                _wasGrounded = true;
            }

            _canJump = true;
        }
    }

    private void DisableJump()
    {
        if (!IsGrounded())
            _canJump = false;
    }

    private void SpawnSmoke()
    {
        if (smokePrefab == null) return;

        Vector3 spawnPos = transform.position + new Vector3(0f, -0.5f, 0f);
        ParticleSystem smoke = Instantiate(smokePrefab, spawnPos, Quaternion.identity);
        smoke.Play();
        Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
    }
}