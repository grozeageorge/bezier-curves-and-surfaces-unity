using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BezierSurface : MonoBehaviour
{
    public Transform[] flatControlPoints; // Assign spheres here
    public int rows = 4; // u direction
    public int cols = 4; // v direction
    public int resolutionU = 20; // Surface resolution
    public int resolutionV = 20;

    private Transform[,] controlPoints;
    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateControlPointGrid();
    }

    void Update()
    {
        UpdateControlPointGrid();
        UpdateSurfaceMesh();
    }

    void UpdateControlPointGrid()
    {
        controlPoints = new Transform[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                controlPoints[i, j] = flatControlPoints[i * cols + j];
    }

    void UpdateSurfaceMesh()
    {
        int vertCountU = resolutionU + 1;
        int vertCountV = resolutionV + 1;

        Vector3[] vertices = new Vector3[vertCountU * vertCountV];
        Vector2[] uvs = new Vector2[vertCountU * vertCountV]; // UVs added
        int[] triangles = new int[resolutionU * resolutionV * 6];

        for (int i = 0; i <= resolutionU; i++)
        {
            float u = i / (float)resolutionU;
            for (int j = 0; j <= resolutionV; j++)
            {
                float v = j / (float)resolutionV;
                int index = i * vertCountV + j;
                vertices[index] = EvaluateBezierSurface(u, v);
                uvs[index] = new Vector2(u, v); // Map UV based on surface coordinates
            }
        }

        int triIndex = 0;
        for (int i = 0; i < resolutionU; i++)
        {
            for (int j = 0; j < resolutionV; j++)
            {
                int a = i * vertCountV + j;
                int b = a + 1;
                int c = a + vertCountV;
                int d = c + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = b;
                triangles[triIndex++] = c;

                triangles[triIndex++] = b;
                triangles[triIndex++] = d;
                triangles[triIndex++] = c;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs; // Assign UVs
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    Vector3 EvaluateBezierSurface(float u, float v)
    {
        Vector3 point = Vector3.zero;

        for (int i = 0; i < rows; i++)
        {
            float Bu = Bernstein(rows-1, i, u);
            for (int j = 0; j < cols; j++)
            {
                float Bv = Bernstein(cols-1, j, v);
                point += Bu * Bv * controlPoints[i, j].position;
            }
        }

        return point;
    }

    float Bernstein(int n, int i, float t)
    {
        return BinomialCoefficient(n, i) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }

    int BinomialCoefficient(int n, int k)
    {
        if (k == 0 || k == n) return 1;
        int res = 1;
        for (int i = 1; i <= k; i++)
        {
            res *= n--;
            res /= i;
        }
        return res;
    }
}
