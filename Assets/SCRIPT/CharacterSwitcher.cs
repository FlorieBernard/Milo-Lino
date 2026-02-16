using UnityEditor;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerMovementMilo miloMovement;
    [SerializeField] private PlayerMovementLino linoMovement;
    [SerializeField] private Collider2D miloCollider;
    [SerializeField] private Collider2D linoCollider;

    [Header("Effets visuels")]
    [SerializeField] private SpriteRenderer miloSprite;
    [SerializeField] private SpriteRenderer linoSprite;
    [SerializeField] private Camera mainCamera;

    [Header("Objets exclusifs à Lino")]
    [SerializeField] private GameObject[] linoOnlyObjects;
    [SerializeField] private Transform miloTransform;
    [SerializeField] private float detectionDistance = 3f;

    private bool isPlayingMilo = true;

    void Start()
    {
        // Milo actif au départ, Lino désactivé
        if (miloMovement != null) miloMovement.enabled = true;
        if (linoMovement != null) linoMovement.enabled = false;

        // Ignorer les collisions entre Milo et Lino
        if (miloCollider != null && linoCollider != null)
        {
            Physics2D.IgnoreCollision(miloCollider, linoCollider, true);
        }

        // Si miloTransform n'est pas défini, essayer de le trouver
        if (miloTransform == null && miloMovement != null)
        {
            miloTransform = miloMovement.transform;
        }

        // Couleurs et objets normaux au départ
        UpdateColors();
        UpdateLinoObjects();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCharacter();
        }

        // Met à jour la visibilité des objets en fonction de la distance
        UpdateLinoObjects();
    }

    void SwitchCharacter()
    {
        if (miloMovement != null && linoMovement != null)
        {
            // Inverse l'état des scripts de mouvement
            miloMovement.enabled = !miloMovement.enabled;
            linoMovement.enabled = !linoMovement.enabled;

            // Stop la vélocité des personnages (stopper leur mouvement)
            miloMovement.gameObject.GetComponent<Rigidbody2D>().linearVelocity = 
                new Vector2(0, miloMovement.gameObject.GetComponent<Rigidbody2D>().linearVelocity.y);
            linoMovement.gameObject.GetComponent<Rigidbody2D>().linearVelocity = 
                new Vector2(0, linoMovement.gameObject.GetComponent<Rigidbody2D>().linearVelocity.y);



            // Change quel personnage est actif
            isPlayingMilo = !isPlayingMilo;

            // Met à jour les couleurs
            UpdateColors();
        }
    }

    void UpdateColors()
    {
        if (isPlayingMilo)
        {
            // Milo actif : couleurs normales
            if (miloSprite != null) miloSprite.color = Color.white;
            if (linoSprite != null) linoSprite.color = Color.gray;
            if (mainCamera != null) mainCamera.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // Bleu ciel
        }
        else
        {
            // Lino actif : tout en gris
            if (miloSprite != null) miloSprite.color = Color.gray;
            if (linoSprite != null) linoSprite.color = Color.white;
            if (mainCamera != null) mainCamera.backgroundColor = new Color(0.5f, 0.5f, 0.5f); // Gris
        }
    }

    void UpdateLinoObjects()
    {
        // Active/désactive les objets exclusifs à Lino
        foreach (GameObject obj in linoOnlyObjects)
        {
            if (obj != null && miloTransform != null)
            {
                float distance = Vector3.Distance(miloTransform.position, obj.transform.position);

                // Visible si on joue avec Lino OU si Milo est proche
                bool shouldBeVisible = !isPlayingMilo || distance <= detectionDistance;
                obj.SetActive(shouldBeVisible);
            }
        }
    }
}