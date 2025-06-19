using UnityEngine;

public class LaserController : MonoBehaviour
{
    public float descendSpeed = 8f;
    public float ascendSpeed = 8f;
    public float waitBeforeFire = 2f;
    public float fireDuration = 5f;
    public float postFireWait = 1f; // Temps d'attente après le danger avant de remonter

    [Header("Hitbox dimensions (en unités monde)")]
    public float hitboxWidth = 20f;
    public float hitboxHeight = 2f;

    private Vector3 targetPos;
    private Vector3 startPos;
    private Vector3 offscreenPos;

    private enum State { Descending, Waiting, Firing, PostFireWait, Ascending, Done }
    private State state = State.Descending;
    private float timer = 0f;

    private Transform player;
    private bool isDangerous = false;

    public void Init(Vector3 start, Vector3 target)
    {
        startPos = start;
        targetPos = target;
        offscreenPos = start;
        transform.position = startPos;
        state = State.Descending;
        timer = 0f;
        gameObject.SetActive(true);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        isDangerous = false;
    }

    void Update()
    {
        switch (state)
        {
            case State.Descending:
                transform.position = Vector3.MoveTowards(transform.position, targetPos, descendSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPos) < 0.01f)
                {
                    state = State.Waiting;
                    timer = 0f;
                }
                break;
            case State.Waiting:
                timer += Time.deltaTime;
                if (timer >= waitBeforeFire)
                {
                    // Force la position exacte avant de devenir dangereux
                    transform.position = targetPos;
                    state = State.Firing;
                    timer = 0f;
                    isDangerous = true;
                }
                break;
            case State.Firing:
                timer += Time.deltaTime;
                if (player != null && isDangerous)
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
                if (timer >= fireDuration)
                {
                    state = State.PostFireWait;
                    timer = 0f;
                    isDangerous = false;
                }
                break;
            case State.PostFireWait:
                timer += Time.deltaTime;
                if (timer >= postFireWait)
                {
                    state = State.Ascending;
                    timer = 0f;
                }
                break;
            case State.Ascending:
                transform.position = Vector3.MoveTowards(transform.position, offscreenPos, ascendSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, offscreenPos) < 0.01f)
                {
                    state = State.Done;
                    gameObject.SetActive(false);
                }
                break;
        }
    }
}