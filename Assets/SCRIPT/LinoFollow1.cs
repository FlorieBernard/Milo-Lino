using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// À attacher sur Lino (le chat).
/// - Au sol : Lino suit uniquement le X de Milo (avec délai). La gravité fait son travail.
/// - En l'air : Lino copie aussi le Y de Milo pour suivre la trajectoire du saut.
/// - Gravité de Lino jamais modifiée.
/// - Fonctionne uniquement dans les zones "couloir" (Trigger avec tag "Couloir").
/// </summary>
public class LinoFollower : MonoBehaviour
{
    [Header("Référence")]
    public Transform milo;

    [Header("Paramètres de suivi")]
    public float speed = 3f;
    public float minDistance = 0.5f;
    public float followDelay = 0.3f;

    // ---- Variables internes ----
    private bool isInCorridor = false;
    private List<Vector3> positionHistory = new List<Vector3>();
    private float timer = 0f;

    private Rigidbody2D miloRb;
    private Rigidbody2D linoRb;

    void Start()
    {
        if (milo != null)
            miloRb = milo.GetComponent<Rigidbody2D>();

        linoRb = GetComponent<Rigidbody2D>();

        if (linoRb != null)
            linoRb.freezeRotation = true; // Lino ne tourne pas
    }

    void FixedUpdate()
    {
        if (milo == null) return;

        // Milo est-il en l'air ?
        bool miloIsAirborne = false;
        if (miloRb != null)
            miloIsAirborne = Mathf.Abs(miloRb.linearVelocity.y) > 0.1f;

        // Enregistrement avec délai
        timer += Time.fixedDeltaTime;
        if (timer >= followDelay)
        {
            positionHistory.Add(milo.position);
            timer = 0f;
        }

        if (isInCorridor && positionHistory.Count > 0)
        {
            Vector3 targetPos = positionHistory[0];

            // Si Milo est au sol → Lino garde son propre Y (gravité normale)
            // Si Milo est en l'air → Lino copie aussi le Y pour suivre le saut
            if (!miloIsAirborne)
                targetPos.y = transform.position.y;

            targetPos.z = transform.position.z;

            float distance = Vector2.Distance(transform.position, targetPos);

            if (distance > minDistance)
            {
                Vector2 newPos = Vector2.MoveTowards(
                    transform.position,
                    targetPos,
                    speed * Time.fixedDeltaTime
                );

                linoRb.MovePosition(newPos);

                // Flip du sprite
                if (targetPos.x < transform.position.x)
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
                else
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            }
            else
            {
                positionHistory.RemoveAt(0);
            }
        }
        else if (!isInCorridor)
        {
            positionHistory.Clear();
            timer = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Couloir"))
        {
            isInCorridor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Couloir"))
        {
            isInCorridor = false;
        }
    }
}
