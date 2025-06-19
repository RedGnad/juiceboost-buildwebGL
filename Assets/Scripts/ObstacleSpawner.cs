using System.Collections;
using UnityEngine;

public class ZapperSpawner : MonoBehaviour
{
    [Header("Prefab de Zapper")]
    public GameObject zapperPrefab;

    [Header("Timing aléatoire")]
    public float spawnIntervalMin = 1.5f;
    public float spawnIntervalMax = 3f;

    [Header("Padding vertical")]
    public float padding = 0.5f;

    [Header("Padding horizontal")]
    public float horizontalSpawnOffset = 0.2f;

    float yMin, yMax, spawnX;
    Coroutine spawnRoutine;

    void Start()
    {
        var cam = Camera.main;
        float vertExt = cam.orthographicSize;
        yMin   = -vertExt + padding;
        yMax   =  vertExt - padding;
        spawnX = cam.transform.position.x + vertExt * cam.aspect + horizontalSpawnOffset;

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            var zap = ObstaclePooler.Instance.GetPooledObstacle();
            float y    = Random.Range(yMin, yMax);
            float angle = Random.Range(0f, 360f);

            zap.transform.position = new Vector3(spawnX, y, 0f);
            zap.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            zap.SetActive(true);
        }
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void RestartSpawning()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnRoutine());
    }
}