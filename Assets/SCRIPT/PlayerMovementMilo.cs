using UnityEngine;

public class PlayerMovementMilo : MonoBehaviour
{
    public float horizontal;
    public float speed = 8f;
    public float jumpingPower = 16f;
    // public bool isFacingRight = true; <-- n'a plus aucun intérêt

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocity.y * 0.5f);
        }

        if (horizontal != 0f)// <-- j'ai ajouté une vérification pour que le flip soit actif uniquement durant le déplacement
        {
            Flip(horizontal); // <-- Flip() + la direction en paramètre
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

    }

    private bool isGrounded()

    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip(float localScaleDirection) // <-- j'ai ajouté un paramètre afin d'y apliquer la direction de Milo
    {
        // isFacingRight = !isFacingRight;  <-- n'a plus aucun intérêt
        Vector3 localScale = transform.localScale;
        localScale.x = localScaleDirection; // <-- j'ai remplacé *=-1 par le paramètre çi-dessus
        transform.localScale = localScale;
    }
}