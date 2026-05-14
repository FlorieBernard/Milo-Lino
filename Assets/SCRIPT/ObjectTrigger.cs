using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _lino;
    [SerializeField] private GameObject _objectToDisappear;
    [SerializeField] private string _miloTag = "Milo";
    [SerializeField] private bool _destroyInsteadOfDisable = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_miloTag) || _objectToDisappear == null) return;

        if (_destroyInsteadOfDisable)
        {
            Destroy(_objectToDisappear);
        }
        else
        {
            _objectToDisappear.SetActive(false);
            _lino?.GetComponent<LinoBlocker>()?.Unblock();
        }

        Destroy(gameObject);
    }
}