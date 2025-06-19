/*using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InteractionBlocker : MonoBehaviour
{
    [Tooltip("Liste des GameObjects interactifs autorisés même quand le panel est affiché (ex: boutons de connexion)")]
    public List<GameObject> allowedInteractives;

    [Tooltip("Le panel de connexion à surveiller")]
    public GameObject walletWaitPanel;

    private CanvasGroup blockerCanvasGroup;

    void Start()
    {
        // Crée un CanvasGroup qui bloque tout sauf les boutons autorisés
        blockerCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        blockerCanvasGroup.interactable = false;
        blockerCanvasGroup.blocksRaycasts = true;
        blockerCanvasGroup.alpha = 0f; // invisible

        // Met le GameObject tout en haut de la hiérarchie UI
        transform.SetAsLastSibling();
    }

    void Update()
    {
        if (walletWaitPanel != null && walletWaitPanel.activeSelf)
        {
            EnableBlocker();
        }
        else
        {
            DisableBlocker();
        }
    }

    void EnableBlocker()
    {
        blockerCanvasGroup.blocksRaycasts = true;
    }

    void DisableBlocker()
    {
        blockerCanvasGroup.blocksRaycasts = false;
    }

    // Cette méthode est appelée automatiquement par Unity pour chaque clic/touch
    public void OnCanvasGroupPointerDown(PointerEventData eventData)
    {
        // Rien à faire ici, tout est bloqué par le CanvasGroup sauf les boutons autorisés
    }

    // Pour que les boutons autorisés restent interactifs, mets-les en dehors de ce GameObject ou
    // mets ce script sur un panel couvrant tout sauf les boutons autorisés (ou utilise un masque).
}*/