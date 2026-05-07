using UnityEngine;

// Script � mettre sur Lino
public class BloquerLino : MonoBehaviour
{
    private bool estBloque = true;
    public bool EstBloque 
    {  get { return estBloque; } 
        private set { estBloque = value; }
    }

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (estBloque)
        {
            // Bloque tous les mouvements de Lino
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0,rb.linearVelocity.y);
            }
        }
    }

    public void Debloquer()
    {
        estBloque = false;
        Debug.Log("Lino est d�bloqu� !");
    }
}