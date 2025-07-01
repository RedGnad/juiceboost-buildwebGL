using UnityEngine;

public class MissileController : MonoBehaviour
{
    [Header("Vitesse")]
    public float baseSpeed = 8f;
    public float speedIncrement = 0.01f; 
    private float currentSpeed;

    void Awake()
    {
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.GameOver();
        }
        else if (transform.position.x < Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - 1f)
        {
            Destroy(gameObject);
        }
    }
}
