using UnityEngine;
using System.Collections.Generic;

public class BernsteinPolynomials : MonoBehaviour
{
    [Range(1, 10)] public int degree = 3;
    [Range(0f, 1f)] public float t = 0.5f;

    public GameObject pointPrefab; // assign a small Sphere prefab
    public Material[] lineMaterials;  // assign a material for the LineRenderers

    private List<LineRenderer> curves = new List<LineRenderer>();
    private List<Transform> markers = new List<Transform>();

    private int resolution = 100;
    private float graphWidth = 10f;
    private float graphHeight = 5f;

    void Start()
    {
        GenerateGraph();
    }

    void Update()
    {
        UpdateMarkers();
    }

    void GenerateGraph()
    {
        ClearOldGraph();

        for (int i = 0; i <= degree; i++)
        {
            GameObject lineObj = new GameObject($"Bernstein_{i}");
            lineObj.transform.parent = this.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = resolution + 1;
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.HSVToRGB(i / (float)(degree + 1), 0.8f, 1f);
            lr.material = mat;
            lr.widthMultiplier = 0.05f;
            lr.useWorldSpace = false;
            lr.numCapVertices = 2;

            Vector3[] points = new Vector3[resolution + 1];

            for (int j = 0; j <= resolution; j++)
            {
                float tVal = j / (float)resolution;
                float y = Bernstein(i, degree, tVal);
                float x = tVal * graphWidth;
                float yScaled = y * graphHeight;
                points[j] = new Vector3(x, yScaled, 0);
            }

            lr.SetPositions(points);
            curves.Add(lr);

            // create a small sphere to highlight this curve's value at current t
            GameObject marker = Instantiate(pointPrefab, transform);
            marker.name = $"Marker_{i}";
            marker.transform.localScale = Vector3.one * 0.15f;
            markers.Add(marker.transform);
        }
    }

    void UpdateMarkers()
    {
        for (int i = 0; i <= degree; i++)
        {
            float y = Bernstein(i, degree, t);
            float x = t * graphWidth;
            float yScaled = y * graphHeight;
            markers[i].localPosition = new Vector3(x, yScaled, 0);
        }
    }

    float Bernstein(int i, int n, float t)
    {
        return Binomial(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
    }

    int Binomial(int n, int k)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

    int Factorial(int n)
    {
        if (n <= 1) return 1;
        int result = 1;
        for (int i = 2; i <= n; i++) result *= i;
        return result;
    }

    void ClearOldGraph()
    {
        foreach (var c in curves)
            Destroy(c.gameObject);
        curves.Clear();

        foreach (var m in markers)
            Destroy(m.gameObject);
        markers.Clear();
    }
}
