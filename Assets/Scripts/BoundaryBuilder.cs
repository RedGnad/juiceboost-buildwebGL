/*using UnityEngine;

[ExecuteAlways]
public class BoundaryBuilder : MonoBehaviour
{
    [Header("Marge autour de l'écran")]
    public float padding = 0.5f;

    [Header("Épaisseur des murs")]
    public float thickness = 0.1f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        BuildWalls();
    }

    void OnValidate()
    {
        // Quand on modifie padding/thickness dans l'Inspector
        cam = Camera.main;
        BuildWalls();
    }

    void BuildWalls()
    {
        if (cam == null) return;

        // Supprime d'abord d'éventuels anciens murs
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);

        float vert = cam.orthographicSize;
        float hori = vert * cam.aspect;

        CreateWall("LeftWall",
            cam.transform.position + new Vector3(-hori - padding, 0f, 0f),
            new Vector2(thickness, vert * 2f)
        );

        CreateWall("RightWall",
            cam.transform.position + new Vector3(+hori + padding, 0f, 0f),
            new Vector2(thickness, vert * 2f)
        );

        CreateWall("TopWall",
            cam.transform.position + new Vector3(0f, +vert + padding, 0f),
            new Vector2(hori * 2f, thickness)
        );

        CreateWall("BottomWall",
            cam.transform.position + new Vector3(0f, -vert - padding, 0f),
            new Vector2(hori * 2f, thickness)
        );
    }

    void CreateWall(string name, Vector3 worldPos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        wall.transform.position = worldPos;

        var bc = wall.AddComponent<BoxCollider2D>();
        bc.size = size;
        bc.offset = Vector2.zero;
    }

    // Visualisation dans l'Éditeur
    void OnDrawGizmos()
    {
        if (cam == null) cam = Camera.main;
        float vert = cam.orthographicSize;
        float hori = vert * cam.aspect;

        Gizmos.color = Color.red;

        // Left
        Gizmos.DrawWireCube(
            cam.transform.position + new Vector3(-hori - padding, 0f, 0f),
            new Vector3(thickness, vert * 2f, 1f)
        );
        // Right
        Gizmos.DrawWireCube(
            cam.transform.position + new Vector3(+hori + padding, 0f, 0f),
            new Vector3(thickness, vert * 2f, 1f)
        );
        // Top
        Gizmos.DrawWireCube(
            cam.transform.position + new Vector3(0f, +vert + padding, 0f),
            new Vector3(hori * 2f, thickness, 1f)
        );
        // Bottom
        Gizmos.DrawWireCube(
            cam.transform.position + new Vector3(0f, -vert - padding, 0f),
            new Vector3(hori * 2f, thickness, 1f)
        );
    }
}*/
