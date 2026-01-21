using UnityEngine;

public class RespawnOnFall : MonoBehaviour
{
    public float hauteurMort = -10f; // Hauteur en dessous de laquelle on respawn

    private Vector3 positionDepart;

    void Start()
    {
        // Sauvegarde la position de d�part
        positionDepart = transform.position;
    }

    void Update()
    {
        // Si le personnage tombe en dessous de la hauteur de mort
        if (transform.position.y < hauteurMort)
        {
            // Retour � la position de d�part
            transform.position = positionDepart;

            // Arr�te le mouvement
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}