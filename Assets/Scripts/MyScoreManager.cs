using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;

public class MyScoreManager : MonoBehaviour
{
    public TMP_Text bestScoreText;
    public TMP_Text totalScoreText;

    // Appelée automatiquement par JS
    public void OnMyScoresReceived(string json)
    {
        var obj = JObject.Parse(json);
        int bestScore = obj["bestScore"]?.Value<int>() ?? 0;
        int totalScore = obj["totalScore"]?.Value<int>() ?? 0;

        if (bestScoreText != null)
            bestScoreText.text = $"Best Score : {bestScore} m";
        if (totalScoreText != null)
            totalScoreText.text = $"Total Score : {totalScore} m";
    }

    public void RequestMyScores()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetMyScores();
#endif
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void GetMyScores();
}