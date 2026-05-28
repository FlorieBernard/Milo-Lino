using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private Camera _cam;
    [SerializeField] private Vector3 _camStartPosition;
    [SerializeField] private float _camSize;

    [SerializeField] private CinemachineCamera _cineFollowCam;
    [SerializeField] private CinemachineCamera _cineFixedCam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SetUpCamera();
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Debug shortcuts — editor only, stripped from builds.
        if (Input.GetKeyDown(KeyCode.I)) SetParallaxMode(true);
        if (Input.GetKeyDown(KeyCode.O)) SetParallaxMode(false);
#endif
    }

    /// <summary>
    /// Switches between the follow camera (parallax visible) and the fixed camera.
    /// Called automatically by ParallaxZone — no need to call this manually.
    /// </summary>
    public void SetParallaxMode(bool enabled)
    {
        _cineFollowCam.gameObject.SetActive(enabled);
        _cineFixedCam.gameObject.SetActive(!enabled);
    }

    private void SetUpCamera()
    {
        _cam.transform.position = _camStartPosition;
        _cam.orthographicSize = _camSize;
    }
}
