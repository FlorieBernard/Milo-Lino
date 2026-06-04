using UnityEngine;

public class PlayerMovementMilo : PlayerMovementBase
{

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && CanJump)
            TryJump();

    }


}
