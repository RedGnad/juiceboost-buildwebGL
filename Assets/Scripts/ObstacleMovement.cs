using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    [Header("Défilement")]
    public float moveSpeed = 5f;

    [Header("Désactivation")]
    public float despawnOffset = 0.2f;

    [Header("Rotation dynamique")]
    [Range(0,1)]
    public float rotationChance   = 0.2f;
    public float maxRotationSpeed = 180f;

    float destroyX;
    float rotSpeed;

    void Awake()
    {
        var cam = Camera.main;
        float horz = cam.orthographicSize * cam.aspect;
        destroyX = cam.transform.position.x - horz - despawnOffset;
    }

    void OnEnable()
    {
        rotSpeed = (Random.value < rotationChance)
            ? Random.Range(-maxRotationSpeed, maxRotationSpeed)
            : 0f;
    }

    void Update()
    {
        float speedMult = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentSpeedMultiplier : 1f;
        transform.Translate(Vector3.left * moveSpeed * speedMult * Time.deltaTime, Space.World);

        if (rotSpeed != 0f)
            transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime, Space.Self);

        if (transform.position.x < destroyX)
            gameObject.SetActive(false);
    }
}