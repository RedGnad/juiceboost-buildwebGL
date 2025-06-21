using UnityEngine;

public class CoinMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float despawnOffset = 0.2f;

    float destroyX;

    void Awake()
    {
        var cam = Camera.main;
        float horz = cam.orthographicSize * cam.aspect;
        destroyX = cam.transform.position.x - horz - despawnOffset;
    }

    void Update()
    {
        float speedMult = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentSpeedMultiplier : 1f;
        transform.Translate(Vector3.left * moveSpeed * speedMult * Time.deltaTime, Space.World);

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}