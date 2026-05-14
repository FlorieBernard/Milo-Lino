using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Vector3 _camStartPosition;
    [SerializeField] private float _camSize;

    [SerializeField] private CinemachineCamera _cineFollowCam;
    [SerializeField] private CinemachineCamera _cineFixedCam;

    private void Awake()
    {
        SetUpCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _cineFollowCam.gameObject.SetActive(true);
            _cineFixedCam.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            _cineFixedCam.gameObject.SetActive(true);
            _cineFollowCam.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchCamera();
        }
    }

    private void SetUpCamera()
    {
        _cam.transform.position = _camStartPosition;
        _cam.orthographicSize = _camSize;
    }

    public void SwitchCamera()
    {
        _cineFixedCam.gameObject.SetActive(!_cineFixedCam.gameObject.activeSelf);
        _cineFollowCam.gameObject.SetActive(!_cineFollowCam.gameObject.activeSelf);
    }
}
