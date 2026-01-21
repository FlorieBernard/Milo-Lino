using Unity.VisualScripting;
using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    [SerializeField] private GameObject objectToDisappear;
    [SerializeField] private string miloTag = "Milo";
    [SerializeField] private bool destroyInsteadOfDisable = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(miloTag))
        {
            if (objectToDisappear != null)
            {
                if (destroyInsteadOfDisable)
                {
                    Destroy(objectToDisappear);
                }
                else
                {
                    objectToDisappear.SetActive(false);
                }
            }
            Destroy(gameObject);
        }
    }
}