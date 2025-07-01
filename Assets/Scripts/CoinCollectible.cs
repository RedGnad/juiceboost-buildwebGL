using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoin();
            gameObject.SetActive(false); 
        }
    }
}