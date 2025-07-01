using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text coinText;

    [Header("SFX")]
    public AudioClip coinCollectSfx;
    public AudioSource sfxAudioSource;

    private int coins = 0;
    public int Coins => coins;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        coins = 0;
        UpdateUI();
    }

    public void AddCoin()
    {
        coins++;
        UpdateUI();

        if (coinCollectSfx != null && sfxAudioSource != null)
            sfxAudioSource.PlayOneShot(coinCollectSfx);
    }

    public void ResetCoins()
    {
        coins = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coins.ToString();
    }
}