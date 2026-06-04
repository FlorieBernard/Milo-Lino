using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
    [SerializeField] LayerMask collideWith;
    ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collideWith.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            if (particles != null) particles.Play();
        }
    }
}
