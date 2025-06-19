using System.Collections.Generic;
using UnityEngine;

public class ObstaclePooler : MonoBehaviour
{
    public static ObstaclePooler Instance;

    [Tooltip("Prefab utilisé pour tous les obstacles")]
    public GameObject obstacleTemplate;
    public int initialPoolSize = 10;

    List<GameObject> pool = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (obstacleTemplate == null)
        {
            Debug.LogError("ObstaclePooler : obstacleTemplate non assigné !");
            return;
        }
        for (int i = 0; i < initialPoolSize; i++)
        {
            var go = Instantiate(obstacleTemplate, transform);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    public GameObject GetPooledObstacle()
    {
        foreach (var go in pool)
            if (!go.activeInHierarchy)
                return go;

        var newGo = Instantiate(obstacleTemplate, transform);
        newGo.SetActive(false);
        pool.Add(newGo);
        return newGo;
    }
}