using UnityEngine;

public class LaserController : MonoBehaviour
{
    public float hitboxWidth = 20f;
    public float hitboxHeight = 2f;

    private Transform player;
    private bool isDangerous = false;

    public void Init(Vector3 start, Vector3 target)
    {
        transform.position = start;
        gameObject.SetActive(true);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        isDangerous = false;
    }

    public void SetDangerous(bool value)
    {
        isDangerous = value;
    }

    void Update()
    {
        if (isDangerous && player != null)
        {
            Vector2 laserCenter = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.position.x, player.position.y);
            float halfW = hitboxWidth / 2f;
            float halfH = hitboxHeight / 2f;
            if (playerPos.x > laserCenter.x - halfW && playerPos.x < laserCenter.x + halfW &&
                playerPos.y > laserCenter.y - halfH && playerPos.y < laserCenter.y + halfH)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}