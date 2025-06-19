using System.Collections;
using UnityEngine;

public class WarningSpawner : MonoBehaviour
{
    [Header("Warning Settings")]
    public GameObject warningIconPrefab;
    public AudioClip warningSound;
    public float warningDuration = 2f;

    [Header("Position Padding")]
    [Tooltip("Décalage (en fraction de largeur d'écran) depuis le bord droit")]
    [Range(0f, 0.5f)]
    public float viewportPaddingX = 0.05f;  

    [Header("Missile")]
    public GameObject missilePrefab;
    public Transform player;
    public AudioClip missileSpawnSound;
    public AudioSource missileAudioSource;

    [Header("Spawn Timing")]
    public float spawnIntervalMin = 2f;
    public float spawnIntervalMax = 5f;

    public float missileSoundAdvance = 0.5f;

    Coroutine spawnRoutine;

    void Start()
    {
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        Camera cam = Camera.main;
        float vpZ = cam.nearClipPlane + 0.01f;

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));

            float spawnVpX = 1f - viewportPaddingX;
            float spawnVpY = cam.WorldToViewportPoint(player.position).y;
            Vector3 vpPos = new Vector3(spawnVpX, spawnVpY, vpZ);
            Vector3 worldPos = cam.ViewportToWorldPoint(vpPos);

            GameObject warning = Instantiate(
                warningIconPrefab,
                worldPos,
                Quaternion.identity
            );

            var sr = warning.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float halfW = sr.bounds.extents.x;
                warning.transform.position += Vector3.left * halfW;
                sr.sortingLayerName = "UIOverlay";
                sr.sortingOrder     = 100;
            }

            GameObject warningAudioObj = new GameObject("WarningSFX");
            warningAudioObj.transform.position = cam.transform.position;
            AudioSource warningSrc = warningAudioObj.AddComponent<AudioSource>();
            warningSrc.clip = warningSound;
            warningSrc.pitch = 0.8f;
            warningSrc.volume = 0.65f;
            warningSrc.Play();
            Destroy(warningAudioObj, warningSound.length / warningSrc.pitch);

            Destroy(warning, warningDuration);

            if (missileSpawnSound != null && warningDuration > missileSoundAdvance)
            {
                yield return new WaitForSeconds(warningDuration - missileSoundAdvance);

                if (missileAudioSource != null)
                {
                    missileAudioSource.pitch = Random.Range(0.9f, 1.2f);
                    missileAudioSource.PlayOneShot(missileSpawnSound);
                }
                else
                {
                    GameObject audioObj = new GameObject("MissileSFX");
                    audioObj.transform.position = cam.transform.position;
                    AudioSource src = audioObj.AddComponent<AudioSource>();
                    src.clip = missileSpawnSound;
                    src.pitch = Random.Range(0.9f, 1.2f);
                    src.Play();
                    Destroy(audioObj, missileSpawnSound.length / src.pitch);
                }

                yield return new WaitForSeconds(missileSoundAdvance);
            }
            else
            {
                yield return new WaitForSeconds(warningDuration);
                if (missileSpawnSound != null)
                {
                    if (missileAudioSource != null)
                    {
                        missileAudioSource.pitch = Random.Range(0.9f, 1.2f);
                        missileAudioSource.PlayOneShot(missileSpawnSound);
                    }
                    else
                    {
                        GameObject audioObj = new GameObject("MissileSFX");
                        audioObj.transform.position = cam.transform.position;
                        AudioSource src = audioObj.AddComponent<AudioSource>();
                        src.clip = missileSpawnSound;
                        src.pitch = Random.Range(0.9f, 1.2f);
                        src.Play();
                        Destroy(audioObj, missileSpawnSound.length / src.pitch);
                    }
                }
            }

            Vector3 missilePos = new Vector3(worldPos.x, worldPos.y, 0f);
            Instantiate(missilePrefab, missilePos, Quaternion.identity);
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