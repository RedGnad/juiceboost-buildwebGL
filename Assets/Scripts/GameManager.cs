using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using TMPro;
using Sample;

public class GameManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SubmitScore(int score);

    public static GameManager Instance { get; private set; }
    public TMP_Text gameOverText;

    private bool gameOver = false;
    private bool canRestart = false;
    private bool walletReady = false;
    private string _walletAddress;

    public bool IsGameOver => gameOver;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        AppKitInit.OnAppKitInitialized += OnAppKitInitialized;
    }

    private void OnAppKitInitialized()
    {
        var appKit = FindObjectOfType<AppKitInit>();
        if (appKit != null && !string.IsNullOrEmpty(appKit.WalletAddress))
        {
            _walletAddress = appKit.WalletAddress;
            walletReady = true;
            Debug.Log($"[GameManager] WalletAddress prêt : {_walletAddress}");
        }
        else
        {
            Debug.LogWarning("[GameManager] Pas d'adresse valide dans AppKitInit.");
        }
    }

    public void GameOver()
    {
        if (gameOver) return;
        gameOver = true;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(true);

        // Stoppe tous les spawners
        foreach (var spawner in FindObjectsOfType<ZapperSpawner>())
            spawner.StopSpawning();
        foreach (var spawner in FindObjectsOfType<WarningSpawner>())
            spawner.StopSpawning();

        // Désactive le mouvement du joueur si le panel wallet est affiché
        var appKitInit = FindObjectOfType<Sample.AppKitInit>();
        if (appKitInit != null && appKitInit.walletWaitPanel != null && appKitInit.walletWaitPanel.activeSelf)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null && player.enabled)
                player.enabled = false;
        }

        if (!walletReady)
        {
            Debug.LogWarning("[GameManager] GameOver déclenché avant que le wallet ne soit prêt.");
            return;
        }

        int finalScore = Mathf.FloorToInt(ScoreManager.Instance.CurrentScore);
        SubmitScore(finalScore);
        Debug.Log($"[GameManager] SubmitScore called with {finalScore}");
    }

    void Update()
    {
        if (!gameOver) return;
        if (!canRestart)
        {
            if (!Input.GetMouseButton(0) && Input.touchCount == 0)
                canRestart = true;
            return;
        }

        // Bloque le restart si le panel wallet est affiché
        var appKitInit = FindObjectOfType<Sample.AppKitInit>();
        if (appKitInit != null && appKitInit.walletWaitPanel != null && appKitInit.walletWaitPanel.activeSelf)
            return;

        // Réactive le mouvement du joueur si le panel wallet n'est plus affiché
        var player = FindObjectOfType<PlayerController>();
        if (player != null && !player.enabled)
            player.enabled = true;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            foreach (var spawner in FindObjectsOfType<ZapperSpawner>())
                spawner.RestartSpawning();
            foreach (var spawner in FindObjectsOfType<WarningSpawner>())
                spawner.RestartSpawning();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnDestroy()
    {
        AppKitInit.OnAppKitInitialized -= OnAppKitInitialized;
    }
}