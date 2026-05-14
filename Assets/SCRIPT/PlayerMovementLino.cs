using UnityEngine;

public class PlayerMovementLino : PlayerMovementBase
{
    private LinoBlocker _blocker;

    private void Start()
    {
        _blocker = GetComponent<LinoBlocker>();
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && (_blocker == null || !_blocker.IsBlocked))
            TryJump();
    }
}