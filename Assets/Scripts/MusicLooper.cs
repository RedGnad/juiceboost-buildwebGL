using UnityEngine;

public class MusicLooper : MonoBehaviour
{
    public AudioSource musicSource;
    public float speedIncreasePerLoop = 0.1f; // 10% plus rapide à chaque boucle

    [Header("UI")]
    public GameObject walletWaitPanel;

    private float basePitch = 1f;
    private int loopCount = 0;
    private float musicLength;
    private float timer = 0f;

    // On ne démarre jamais la musique automatiquement
    void Start()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
        if (musicSource == null || musicSource.clip == null)
        {
            Debug.LogError("MusicLooper: AudioSource ou AudioClip manquant !");
            enabled = false;
            return;
        }
        musicLength = musicSource.clip.length;
        // Ne pas jouer la musique ici
        musicSource.Stop();
    }

    void Update()
    {
        // Si la musique n'est pas en cours, ne rien faire
        if (!musicSource.isPlaying)
            return;

        timer += Time.deltaTime * musicSource.pitch;
        if (timer >= musicLength)
        {
            loopCount++;
            timer = 0f;
            musicSource.time = 0f;
            musicSource.pitch = basePitch + speedIncreasePerLoop * loopCount;
            musicSource.Play();
        }
    }

    // Appelle cette méthode lors du "replay" si le panel est caché
    public void PlayMusicFromStart()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        musicSource.pitch = basePitch;
        musicSource.time = 0f;
        timer = 0f;
        loopCount = 0;
        musicSource.Play();
    }

    // Appelle cette méthode si besoin pour forcer l'arrêt
    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
        timer = 0f;
        loopCount = 0;
        musicSource.pitch = basePitch;
    }
}