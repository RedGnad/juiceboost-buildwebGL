using UnityEngine;

public class MaterialScroller : MonoBehaviour
{
    [Tooltip("Vitesse de défilement de l'offset X")]
    public float scrollSpeed = 0.5f;

    private Material mat;
    private Vector2 offset;

    void Awake()
    {
        mat = GetComponent<Renderer>().material;
        offset = mat.mainTextureOffset;
    }

    void Update()
    {
        float speedMult = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentSpeedMultiplier : 1f;
        offset.x += scrollSpeed * speedMult * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }
}