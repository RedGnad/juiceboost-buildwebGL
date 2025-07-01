using UnityEngine;
using UnityEngine.UI;
using Reown.AppKit.Unity;
using Sample;

public class SimpleConnectButton : MonoBehaviour
{
    [SerializeField] private Button connectButton;
    [SerializeField] private float retryInterval = 0.5f;

    private void Awake()
    {
        if (connectButton == null)
            connectButton = GetComponent<Button>();

        connectButton.interactable = true;

        connectButton.onClick.AddListener(OpenWalletModal);
    }

    private void OpenWalletModal()
    {
        if (AppKit.IsInitialized)
        {
            AppKit.OpenModal();
        }
        else
        {
            Debug.Log("AppKit n'est pas encore initialisé. Tentative d'initialisation...");
            
            AppKitInit.TryInitialize();
            
            StartCoroutine(RetryOpenModal());
        }
    }
    
    private System.Collections.IEnumerator RetryOpenModal()
    {
        yield return new WaitForSeconds(retryInterval);
        
        if (AppKit.IsInitialized)
        {
            Debug.Log("AppKit est maintenant initialisé! Ouverture du modal...");
            AppKit.OpenModal();
        }
    }
}