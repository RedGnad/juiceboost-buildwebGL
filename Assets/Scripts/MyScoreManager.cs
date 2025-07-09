using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using System.Collections;

public class MyScoreManager : MonoBehaviour
{
    public TMP_Text bestScoreText;
    public TMP_Text totalScoreText;

    private bool hasReceivedScores = false;
    private Coroutine retryCoroutine;

    void Awake()
    {
        Debug.Log("[MyScoreManager] Awake. bestScoreText assigned: " + (bestScoreText != null) + ", totalScoreText assigned: " + (totalScoreText != null));
    }

    void Start()
    {
        // Test d'affichage manuel pour vérifier l'UI
        // bestScoreText.text = "TEST BEST";
        // totalScoreText.text = "TEST TOTAL";
    }

    public void OnMyScoresReceived(string json)
    {
        Debug.LogError("=== [MyScoreManager] OnMyScoresReceived CALLED === json: " + json);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("=== [MyScoreManager] Received empty JSON! ===");
            return;
        }

        int bestScore = 0;
        int totalScore = 0;
        try
        {
            JObject obj = JObject.Parse(json);
            bestScore = obj["bestScore"]?.Value<int>() ?? 0;
            totalScore = obj["totalScore"]?.Value<int>() ?? 0;
            Debug.LogError("=== [MyScoreManager] Parsed bestScore: " + bestScore + " / totalScore: " + totalScore);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("=== [MyScoreManager] JSON Parse Error: " + ex.Message);
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best Score : {bestScore} m";
            Debug.LogError("=== [MyScoreManager] bestScoreText updated ===");
        }
        if (totalScoreText != null)
        {
            totalScoreText.text = $"Total Score : {totalScore} m";
            Debug.LogError("=== [MyScoreManager] totalScoreText updated ===");
        }

        hasReceivedScores = true;
        if (retryCoroutine != null)
        {
            StopCoroutine(retryCoroutine);
            retryCoroutine = null;
        }
    }

    public void RequestMyScores()
    {
        Debug.LogError("[MyScoreManager] RequestMyScores called");
        hasReceivedScores = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        GetMyScores();
        if (retryCoroutine != null) StopCoroutine(retryCoroutine);
        retryCoroutine = StartCoroutine(RetryUntilReceived());
#else
        Debug.LogError("[MyScoreManager] Not WebGL build, GetMyScores() not called.");
#endif
    }

    private IEnumerator RetryUntilReceived()
    {
        while (!hasReceivedScores)
        {
            yield return new WaitForSeconds(5f);
            if (!hasReceivedScores)
            {
                Debug.LogError("[MyScoreManager] Retrying GetMyScores...");
                GetMyScores();
            }
        }
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void GetMyScores();

    public void TestCall(string msg)
    {
        Debug.LogError("=== [MyScoreManager] TestCall received: " + msg);
    }
}