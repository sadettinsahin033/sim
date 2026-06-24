using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OvalMirrorSurface : MonoBehaviour
{
    [Header("Oval Boyut")]
    public float width = 0.158f;
    public float height = 0.090f;

    [Header("Kalite")]
    [Range(16, 128)]
    public int segments = 64;

    private void Reset()
    {
        BuildMesh();
    }

    private void OnValidate()
    {
        BuildMesh();
    }

    [ContextMenu("Build Oval Mirror Mesh")]
    public void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Oval Mirror Surface Mesh";

        Vector3[] vertices = new Vector3[segments + 1];
        Vector2[] uvs = new Vector2[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.PI * 2f * i / segments;
            float x = Mathf.Cos(angle) * halfW;
            float y = Mathf.Sin(angle) * halfH;

            vertices[i + 1] = new Vector3(x, y, 0f);

            uvs[i + 1] = new Vector2(
                (x / width) + 0.5f,
                (y / height) + 0.5f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            int triIndex = i * 3;

            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i == segments - 1 ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}