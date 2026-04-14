using UnityEngine;

public class LinoFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    private bool isFollowing = false;
    private float posFollowY;
    [SerializeField] private float followSpeed;

    private void Start()
    {
        Debug.Log("Function Start()");
        //StartFollow();
    }

    private void StartFollow()
    {
        posFollowY = transform.position.y;
        isFollowing = true;
        Debug.Log("Start following");
    }

    private void FixedUpdate()
    {
        if (isFollowing)
        {
            Vector2 targetPosTemp = new Vector2(followTarget.position.x, posFollowY);
            transform.position = Vector2.MoveTowards(transform.position, targetPosTemp, followSpeed * Time.deltaTime);
        }
    }
}
