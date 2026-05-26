using UnityEngine;

public class PlayerMovementMilo : PlayerMovementBase
{
    [SerializeField] private ParticleSystem smokePrefab;

    private bool _wasGrounded = true;

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && CanJump)
            TryJump();

        HandleLanding();
    }

    private void HandleLanding()
    {
        bool grounded = IsGrounded();

        if (grounded && !_wasGrounded)
        {
            SpawnSmoke();
            AudioManager.Instance?.Play("Land");
        }

        _wasGrounded = grounded;
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
