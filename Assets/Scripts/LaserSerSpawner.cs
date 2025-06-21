using System.Collections;
using UnityEngine;

public class LaserSetSpawner : MonoBehaviour
{
    public GameObject laserPrefab;
    public float minDistance = 60f;
    public float maxDistance = 120f;

    public float descendSpeed = 8f;
    public float ascendSpeed = 8f;

    [Header("Laser Timing")]
    public float waitBeforeFire = 2f;
    public float fireDuration = 5f;
    [Tooltip("Durée d'attente après la phase dangereuse (en secondes)")]
    public float waitAfterFire = 1f;

    [Header("Laser SFX")]
    public AudioClip laserChargeSfx;
    public AudioClip laserFireSfx;
    public AudioSource sfxAudioSource;

    private float nextLaserAt = 0f;

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
            while (ScoreManager.Instance.CurrentScore < nextLaserAt)
                yield return null;

            foreach (var spawner in FindObjectsOfType<ZapperSpawner>())
                spawner.StopSpawning();
            foreach (var spawner in FindObjectsOfType<WarningSpawner>())
                spawner.StopSpawning();

            // Détermine le quart libre (0, 1, 2, 3)
            int freeQuarter = Random.Range(0, 4);

            Camera cam = Camera.main;
            float vert = cam.orthographicSize * 2f;
            float hori = cam.orthographicSize * cam.aspect * 2f;
            float top = cam.transform.position.y + vert / 2f;

            float quarterHeight = vert / 4f;
            float laserWidth = hori;
            float laserHeight = quarterHeight;

            // Instancie les 3 lasers
            LaserController[] lasers = new LaserController[3];
            Vector3[] targets = new Vector3[3];
            Vector3[] starts = new Vector3[3];
            int idx = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i == freeQuarter) continue;
                Vector3 start = new Vector3(
                    cam.transform.position.x,
                    top + laserHeight / 2f + 1f,
                    0f
                );
                Vector3 target = new Vector3(
                    cam.transform.position.x,
                    top - (i + 0.5f) * quarterHeight,
                    0f
                );
                var go = Instantiate(laserPrefab, start, Quaternion.identity);
                var lc = go.GetComponent<LaserController>();

                var sr = lc.GetComponent<SpriteRenderer>();
                if (sr != null && sr.bounds.size.x > 0f && sr.bounds.size.y > 0f)
                {
                    go.transform.localScale = new Vector3(
                        laserWidth / sr.bounds.size.x,
                        laserHeight / sr.bounds.size.y,
                        1f
                    );
                    lc.hitboxWidth = laserWidth;
                    lc.hitboxHeight = laserHeight;
                }

                lc.Init(start, target);
                lasers[idx] = lc;
                targets[idx] = target;
                starts[idx] = start;
                idx++;
            }

            // 1. Descente synchronisée
            bool allAtTarget = false;
            while (!allAtTarget)
            {
                allAtTarget = true;
                for (int i = 0; i < lasers.Length; i++)
                {
                    if (lasers[i] == null) continue;
                    lasers[i].transform.position = Vector3.MoveTowards(
                        lasers[i].transform.position,
                        targets[i],
                        descendSpeed * Time.deltaTime
                    );
                    if (Vector3.Distance(lasers[i].transform.position, targets[i]) > 0.01f)
                        allAtTarget = false;
                }
                yield return null;
            }
            // Force la position exacte
            for (int i = 0; i < lasers.Length; i++)
                lasers[i].transform.position = targets[i];

            // 2. Attente avant dangerosité (synchronisé)
            float timer = 0f;

            // SFX de chargement
            if (laserChargeSfx != null && sfxAudioSource != null)
                sfxAudioSource.PlayOneShot(laserChargeSfx);

            while (timer < waitBeforeFire)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // 3. Phase dangereuse (synchronisé)
            if (laserFireSfx != null && sfxAudioSource != null)
                sfxAudioSource.PlayOneShot(laserFireSfx);

            for (int i = 0; i < lasers.Length; i++)
                lasers[i].SetDangerous(true);

            timer = 0f;
            while (timer < fireDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < lasers.Length; i++)
                lasers[i].SetDangerous(false);

            // 4. Attente après la phase dangereuse (waitAfterFire)
            timer = 0f;
            while (timer < waitAfterFire)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // 5. Remontée synchronisée
            bool allOffscreen = false;
            while (!allOffscreen)
            {
                allOffscreen = true;
                for (int i = 0; i < lasers.Length; i++)
                {
                    if (lasers[i] == null) continue;
                    lasers[i].transform.position = Vector3.MoveTowards(
                        lasers[i].transform.position,
                        starts[i],
                        ascendSpeed * Time.deltaTime
                    );
                    if (Vector3.Distance(lasers[i].transform.position, starts[i]) > 0.01f)
                        allOffscreen = false;
                }
                yield return null;
            }
            for (int i = 0; i < lasers.Length; i++)
                if (lasers[i] != null)
                    lasers[i].gameObject.SetActive(false);

            foreach (var spawner in FindObjectsOfType<ZapperSpawner>())
                spawner.RestartSpawning();
            foreach (var spawner in FindObjectsOfType<WarningSpawner>())
                spawner.RestartSpawning();

            ScheduleNextLaser();
        }
    }
}