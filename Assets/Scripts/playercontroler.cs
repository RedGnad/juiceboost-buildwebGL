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
        // Récupération des composants
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Configuration physique
        rb.drag = 0f;  // pas de drag pour inertie constante
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Sauvegarde de l'état initial
        startPos     = transform.position;
        startGravity = rb.gravityScale;
        startDrag    = rb.drag;
        startMass    = rb.mass;
    }

    void OnEnable()
    {
        // Reset complet du joueur
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
        // Lecture de l'input (frame-rate indépendant)
        isThrusting = canControl && (Input.GetMouseButton(0) || Input.touchCount > 0);

        // Animation selon l'état isGrounded
        anim.SetBool(isFlyingParam, !isGrounded);
    }

    void FixedUpdate()
    {
        // Application de la poussée
        if (isThrusting)
        {
            rb.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }
    }

    // Détection du sol
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si c'est un bord/sol
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = true;
        }
        // Si c'est un obstacle kill
        else if (collision.collider.CompareTag(obstacleTag))
        {
            canControl = false;
            GameManager.Instance.GameOver();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Quand on quitte le sol
        if (collision.collider.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }

    // (Optionnel) Pour visualiser en scène la zone de groundCheck, si besoin
    void OnDrawGizmosSelected()
    {
        // Rien ici si on n'utilise pas OverlapCircle
    }
}
