using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField][Range(0,1)] private float _parallaxEffect;

    private float _startPos;

    void Start()
    {
        _startPos = transform.position.x;
    }

    void FixedUpdate()
    {
        float dist = Camera.main.transform.position.x * _parallaxEffect;
        transform.position = new Vector3(_startPos + dist, transform.position.y, transform.position.z);
    }
}
