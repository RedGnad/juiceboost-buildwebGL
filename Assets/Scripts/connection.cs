using UnityEngine;
using UnityEngine.UI;
using Reown.AppKit.Unity;
using Sample;

public class SimpleConnectButton : MonoBehaviour
{
    [SerializeField] private Button connectButton;
    [SerializeField] private float retryInterval = 0.5f; // Intervalle entre les tentatives si AppKit n'est pas prêt

    private void Awake()
    {
        if (connectButton == null)
            connectButton = GetComponent<Button>();

        // Le bouton est toujours actif
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
            // Informer l'utilisateur et lancer l'initialisation
            Debug.Log("AppKit n'est pas encore initialisé. Tentative d'initialisation...");
            
            // Déclencher une tentative d'initialisation
            AppKitInit.TryInitialize();
            
            // Attendre un moment puis réessayer automatiquement
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