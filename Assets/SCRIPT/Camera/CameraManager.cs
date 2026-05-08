using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Vector3 _camTransform;
    [SerializeField] private float _camSize;

    [SerializeField] private CinemachineCamera _cineFollowCam;
    [SerializeField] private CinemachineCamera _cineFixedCam;


    public void Awake()
    {
        SetUpCameraOnStart();
    }

    public void SetUpCameraOnStart()
    {
        _cam.transform.position = _camTransform;
        _cam.orthographicSize = _camSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ActiveCinemachine(_cineFollowCam);
            DesactiveCinemachine(_cineFixedCam);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ActiveCinemachine(_cineFixedCam);
            DesactiveCinemachine(_cineFollowCam);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchCamera();
        }
    }

    public void ActiveCinemachine(CinemachineCamera cineCam)
    {
        cineCam.gameObject.SetActive(true);
    }

    public void DesactiveCinemachine(CinemachineCamera cineCam)
    {
        cineCam.gameObject.SetActive(false);
    }

    public void SwitchCamera()
    {
        _cineFixedCam.gameObject.SetActive(!_cineFixedCam.gameObject.activeSelf);
        _cineFollowCam.gameObject.SetActive(!_cineFollowCam.gameObject.activeSelf);
    }
}
