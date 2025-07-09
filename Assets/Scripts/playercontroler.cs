using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Jetpack Settings")]
    [Tooltip("Force appliquée à chaque FixedUpdate quand on appuie")]
    public float thrust = 20f;

    [Header("Ground & Obstacle Tags")]
    [Tooltip("Tag à donner aux bords/sol")]
    public string groundTag = "Ground";
    [Tooltip("Tag à donner aux obstacles kill (zappers)")]
    public string obstacleTag = "Obstacle";

    [Header("Animation")]
    [Tooltip("Nom du paramètre bool dans l'Animator")]
    public string isFlyingParam = "isFlying";

    // Composants
    private Rigidbody2D rb;
    private Animator anim;

    // Contrôle
    private bool canControl = true;
    private bool isThrusting = false;

    // État du sol
    private bool isGrounded = false;

    // État initial (pour reset)
    private Vector3 startPos;
    private float   startGravity;
    private float   startDrag;
    private float   startMass;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.drag = 0f;  // pas de drag pour inertie constante
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        startPos     = transform.position;
        startGravity = rb.gravityScale;
        startDrag    = rb.drag;
        startMass    = rb.mass;
    }

    void OnEnable()
    {
        transform.position = startPos;
        rb.velocity        = Vector2.zero;
        rb.gravityScale    = startGravity;
        rb.drag            = startDrag;
        rb.mass            = startMass;

        canControl  = true;
        isGrounded  = false;
        anim.SetBool(isFlyingParam, false);
    }

    void Update()
    {
        isThrusting = canControl && (Input.GetMouseButton(0) || Input.touchCount > 0);

        anim.SetBool(isFlyingParam, !isGrounded);
    }

    void FixedUpdate()
    {
        if (isThrusting)
        {
            rb.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = true;
        }
        else if (collision.collider.CompareTag(obstacleTag))
        {
            canControl = false;
            GameManager.Instance.GameOver();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }
    void OnDrawGizmosSelected()
    {
        // Rien ici mon reuf
    }
}
