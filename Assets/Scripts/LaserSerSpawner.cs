using System.Collections;
using UnityEngine;

public class LaserSetSpawner : MonoBehaviour
{
    public GameObject laserPrefab;
    public float minDistance = 60f;
    public float maxDistance = 120f;

    public float descendSpeed = 8f;
    public float ascendSpeed = 8f;
    public float waitBeforeFire = 2f;
    public float fireDuration = 5f;

    private float nextLaserAt = 0f;
    private bool lasersActive = false;

    void Start()
    {
        ScheduleNextLaser();
        StartCoroutine(LaserRoutine());
    }

    void ScheduleNextLaser()
    {
        float dist = Random.Range(minDistance, maxDistance);
        nextLaserAt = ScoreManager.Instance.CurrentScore + dist;
    }

    IEnumerator LaserRoutine()
    {
        while (true)
        {
            // Attendre le prochain déclenchement
            while (ScoreManager.Instance.CurrentScore < nextLaserAt)
                yield return null;

            // Stopper les autres spawners
            foreach (var spawner in FindObjectsOfType<ZapperSpawner>())
                spawner.StopSpawning();
            foreach (var spawner in FindObjectsOfType<WarningSpawner>())
                spawner.StopSpawning();

            lasersActive = true;

            // Détermine le quart libre (0, 1, 2, 3)
            int freeQuarter = Random.Range(0, 4);

            Camera cam = Camera.main;
            float vert = cam.orthographicSize * 2f;
            float hori = cam.orthographicSize * cam.aspect * 2f;
            float left = cam.transform.position.x - hori / 2f;
            float top = cam.transform.position.y + vert / 2f;

            float quarterHeight = vert / 4f;
            float laserWidth = hori;
            float laserHeight = quarterHeight;

            // Instancie les 3 lasers
            LaserController[] lasers = new LaserController[3];
            int idx = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i == freeQuarter) continue;
                // Position offscreen (au-dessus)
                Vector3 start = new Vector3(
                    cam.transform.position.x,
                    top + laserHeight / 2f + 1f, // 1f au-dessus de l'écran
                    0f
                );
                // Position cible (dans le quart)
                Vector3 target = new Vector3(
                    cam.transform.position.x,
                    top - (i + 0.5f) * quarterHeight,
                    0f
                );
                var go = Instantiate(laserPrefab, start, Quaternion.identity);
                var lc = go.GetComponent<LaserController>();
                lc.descendSpeed = descendSpeed;
                lc.ascendSpeed = ascendSpeed;
                lc.waitBeforeFire = waitBeforeFire;
                lc.fireDuration = fireDuration;

                // Redimensionne le laser (évite division par zéro)
                var sr = lc.GetComponent<SpriteRenderer>();
                if (sr != null && sr.bounds.size.x > 0f && sr.bounds.size.y > 0f)
                {
                    go.transform.localScale = new Vector3(
                        laserWidth / sr.bounds.size.x,
                        laserHeight / sr.bounds.size.y,
                        1f
                    );
                }
                else
                {
                    Debug.LogError("Le SpriteRenderer du laser a une taille nulle ou n'est pas trouvé !");
                }

                lc.Init(start, target);
                lasers[idx++] = lc;
            }

            // Attend que tous les lasers aient terminé leur cycle
            bool anyActive = true;
            while (anyActive)
            {
                anyActive = false;
                foreach (var l in lasers)
                    if (l != null && l.gameObject.activeSelf)
                        anyActive = true;
                yield return null;
            }

            // Relance les autres spawners
            foreach (var spawner in FindObjectsOfType<ZapperSpawner>())
                spawner.RestartSpawning();
            foreach (var spawner in FindObjectsOfType<WarningSpawner>())
                spawner.RestartSpawning();

            lasersActive = false;
            ScheduleNextLaser();
        }
    }
}