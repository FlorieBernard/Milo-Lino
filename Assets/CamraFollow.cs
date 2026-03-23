using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform milo;
    public Transform lino;
    public float vitesseSuivi = 5f;
    public float offsetZ = -10f; // Pour la 2D, la caméra doit être en arrière
    public bool isGreatRoom = true;

    void LateUpdate()
    {
        if(isGreatRoom)
                return;

        if (milo != null && lino != null)
        {
            // Calcule le point milieu entre Milo et Lino
            Vector3 pointMilieu = (milo.position + lino.position) / 2f;

            // Position cible de la caméra
            Vector3 positionCible = new Vector3(pointMilieu.x, pointMilieu.y, offsetZ);

            // Déplace la caméra en douceur
            transform.position = Vector3.Lerp(transform.position, positionCible, vitesseSuivi * Time.deltaTime);
        }
    }
}