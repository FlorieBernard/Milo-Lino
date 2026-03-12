using UnityEngine;

public class LandingSmoke : MonoBehaviour
{
    [Header("VFX")]
    public ParticleSystem smokePrefab;

    private bool isJumping = false;

    void Update()
    {
        // Détecte quand le perso touche le sol
        if (isJumping && IsGrounded())
        {
            isJumping = false;
            SpawnSmoke();
        }

        // Détecte quand il saute
        if (Input.GetButtonDown("Jump"))
        {
            isJumping = true;
        }
    }

    bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
    }

    void SpawnSmoke()
    {
        if (smokePrefab == null) return;

        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        ParticleSystem smoke = Instantiate(smokePrefab, spawnPos, Quaternion.identity);
        smoke.Play();

        Destroy(smoke.gameObject, smoke.main.duration + smoke.main.startLifetime.constantMax);
    }
}