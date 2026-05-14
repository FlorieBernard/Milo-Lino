using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _milo;
    [SerializeField] private Transform _lino;
    [SerializeField] private float _followSpeed = 5f;
    [SerializeField] private float _offsetZ = -10f;
    [SerializeField] private bool _isGreatRoom = true;

    private void LateUpdate()
    {
        if (_isGreatRoom || _milo == null || _lino == null) return;

        Vector3 midpoint = (_milo.position + _lino.position) / 2f;
        Vector3 targetPosition = new Vector3(midpoint.x, midpoint.y, _offsetZ);
        transform.position = Vector3.Lerp(transform.position, targetPosition, _followSpeed * Time.deltaTime);
    }
}
