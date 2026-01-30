using UnityEngine;

// Script � mettre sur Lino
public class BloquerLino : MonoBehaviour
{
    private bool estBloque = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (estBloque)
        {
            // Bloque tous les mouvements de Lino
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void Debloquer()
    {
        estBloque = false;
        Debug.Log("Lino est d�bloqu� !");
    }
}