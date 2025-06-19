using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Le texte où afficher le score en mètres")]
    public TMP_Text scoreText;

    [Header("Paramètres")]
    [Tooltip("Vitesse de comptage (mètres par seconde)")]
    public float scoreMultiplier = 2f;

    [Header("Accélération")]
    [Tooltip("Score (m) à partir duquel la vitesse augmente")]
    public float[] speedThresholds = { 100, 200, 400 };
    [Tooltip("Multiplicateurs de vitesse pour chaque palier")]
    public float[] speedMultipliers = { 1f, 1.2f, 1.5f, 2f };

    private float score = 0f;

    public float CurrentScore => score;

    public float CurrentSpeedMultiplier
    {
        get
        {
            int idx = 0;
            for (int i = 0; i < speedThresholds.Length; i++)
                if (score >= speedThresholds[i])
                    idx = i + 1;
                else
                    break;
            return speedMultipliers[Mathf.Clamp(idx, 0, speedMultipliers.Length - 1)];
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        score = 0f;
        if (scoreText != null)
            scoreText.text = "0 m";
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // Incrémente le score en fonction du multiplicateur de vitesse
        score += scoreMultiplier * CurrentSpeedMultiplier * Time.deltaTime;
        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(score) + " m";
    }
}