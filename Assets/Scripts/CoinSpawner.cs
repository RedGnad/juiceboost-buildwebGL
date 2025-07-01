using System.Collections;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public float spawnIntervalMin = 1.5f;
    public float spawnIntervalMax = 3f;
    [Range(0f, 1f)]
    public float spawnProbability = 0.7f; 
    public int maxCoinsOnScreen = 5;

    float yMin, yMax, spawnX;
    Coroutine spawnRoutine;

    void Start()
    {
        var cam = Camera.main;
        float vertExt = cam.orthographicSize;
        yMin   = -vertExt + 0.5f;
        yMax   =  vertExt - 0.5f;
        spawnX = cam.transform.position.x + vertExt * cam.aspect + 1f;

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));

            if (GameObject.FindGameObjectsWithTag("Coin").Length >= maxCoinsOnScreen)
                continue;

            if (Random.value > spawnProbability)
                continue;

            float y = Random.Range(yMin, yMax);

            
            var coin = Instantiate(coinPrefab, new Vector3(spawnX, y, 0f), Quaternion.identity);
            coin.tag = "Coin";
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