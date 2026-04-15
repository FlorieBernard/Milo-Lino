using UnityEngine;

public class PlatformeMobile : MonoBehaviour
{
    public enum Direction { GaucheADroite, HautEnBas }

    [Header("Direction du mouvement")]
    public Direction direction = Direction.GaucheADroite;

    [Header("Paramètres")]
    public float distance = 3f;   // Distance parcourue de chaque côté
    public float vitesse = 2f;    // Vitesse de déplacement

    private Vector3 pointDepart;

    void Start()
    {
        pointDepart = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * vitesse) * distance;

        if (direction == Direction.GaucheADroite)
            transform.position = pointDepart + new Vector3(offset, 0f, 0f);
        else
            transform.position = pointDepart + new Vector3(0f, offset, 0f);
    }
}
