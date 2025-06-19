using UnityEngine;

public class ParallaxTilingManager : MonoBehaviour
{
    [Header("Défilement")]
    public float scrollSpeed = 0.5f;

    [Header("Segments (quads côte à côte)")]
    public Transform[] segments;

    [Header("Textures & seuils")]
    [Tooltip("textures[0]=Ground1, textures[1]=Ground2, textures[2]=Ground3,…")]
    public Texture2D[] textures;
    [Tooltip("scoreThresholds[i] = score minimal pour passer à textures[i+1]")]
    public float[]   scoreThresholds;

    float segmentWidth;

    void Start()
    {
        var r = segments[0].GetComponent<Renderer>();
        segmentWidth = r.bounds.size.x;

        foreach (var seg in segments)
        {
            var mr = seg.GetComponent<Renderer>();
            mr.material = new Material(mr.sharedMaterial);
            mr.material.mainTexture = textures[0];
        }
    }

    void Update()
    {
        float speedMult = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentSpeedMultiplier : 1f;
        float dx = scrollSpeed * speedMult * Time.deltaTime;
        foreach (var seg in segments)
            seg.Translate(Vector3.left * dx, Space.World);

        float score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0f;
        int idx = 0;
        for (int i = 0; i < scoreThresholds.Length; i++)
            if (score >= scoreThresholds[i])
                idx = i + 1;
            else
                break;

        foreach (var seg in segments)
        {
            if (seg.position.x <= -segmentWidth)
            {
                float maxX = float.MinValue;
                foreach (var s in segments)
                    maxX = Mathf.Max(maxX, s.position.x);

                seg.position = new Vector3(maxX + segmentWidth,
                                           seg.position.y,
                                           seg.position.z);

                var mr = seg.GetComponent<Renderer>();
                mr.material.mainTexture = textures[idx];
            }
        }
    }
}