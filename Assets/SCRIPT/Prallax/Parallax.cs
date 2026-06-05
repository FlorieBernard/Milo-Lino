using UnityEngine;

/// <summary>
/// Scrolls a background layer at a fraction of the camera's movement to simulate depth.
/// Attach to each background layer GameObject.
///
/// Tips:
///   • Distant layers (sky, mountains) → _parallaxEffect 0.05–0.2
///   • Mid layers (trees, hills)       → _parallaxEffect 0.3–0.5
///   • Near layers (bushes, rocks)     → _parallaxEffect 0.7–0.9
///
/// Runs in LateUpdate so it executes AFTER Cinemachine updates the camera,
/// preventing the one-frame jitter you get with FixedUpdate or Update.
/// </summary>
public class Parallax : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float _parallaxEffect;

    private Camera _cam;
    private float _startPosX;
    private float _startCamPosX;

    private void Awake()
    {
        // Cache Camera.main — calling it every frame does a GetComponent internally.
        _cam = Camera.main;
    }

    private void Start()
    {
        _startPosX = transform.position.x;
        // Store camera start X so the offset is relative, not absolute.
        // Without this, if the camera doesn't start at x=0 the background jumps on load.
        _startCamPosX = _cam.transform.position.x;
    }

    private void LateUpdate()
    {
        float dist = (_cam.transform.position.x - _startCamPosX) * _parallaxEffect;
        transform.position = new Vector3(_startPosX + dist, transform.position.y, transform.position.z);
    }
}
