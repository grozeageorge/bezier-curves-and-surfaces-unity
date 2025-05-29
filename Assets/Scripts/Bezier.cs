using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public GameObject[] points;
    public int segments = 20;

    private LineRenderer curveRenderer;     // for the Bezier curve
    private LineRenderer controlRenderer;   // for the green control polygon

    private void Start()
    {
        // LineRenderer for the Bezier curve
        curveRenderer = gameObject.AddComponent<LineRenderer>();
        curveRenderer.positionCount = segments + 1;
        curveRenderer.widthMultiplier = 0.05f;
        curveRenderer.material = new Material(Shader.Find("Unlit/Color"));
        curveRenderer.material.color = Color.white;

        // LineRenderer for the control polygon
        GameObject controlObject = new GameObject("ControlLines");
        controlObject.transform.parent = this.transform;

        controlRenderer = controlObject.AddComponent<LineRenderer>();
        controlRenderer.positionCount = 0;
        controlRenderer.widthMultiplier = 0.025f;
        controlRenderer.material = new Material(Shader.Find("Unlit/Color"));
        controlRenderer.material.color = Color.green;
        controlRenderer.loop = false;
    }

    private void Update()
    {
        DrawBezierCurve();
        DrawControlPolygon();
    }

    private void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;

        Vector3[] positions = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            positions[i] = points[i].transform.position;

        Gizmos.color = Color.cyan;
        Vector3 lastPos = positions[0];
        float resolution = 1f / Mathf.Max(1, segments);

        for (int i = 1; i <= segments; i++)
        {
            float t = i * resolution;
            Vector3 newPos = DeCasteljau(t, positions)[0];
            Gizmos.DrawLine(lastPos, newPos);
            lastPos = newPos;
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < positions.Length - 1; i++)
            Gizmos.DrawLine(positions[i], positions[i + 1]);
    }

    private void DrawBezierCurve()
    {
        if (points == null || points.Length < 2) return;

        Vector3[] positions = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            positions[i] = points[i].transform.position;

        Vector3[] curvePoints = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            curvePoints[i] = DeCasteljau(t, positions)[0];
        }

        curveRenderer.positionCount = curvePoints.Length;
        curveRenderer.SetPositions(curvePoints);
    }
    void DrawControlPolygon()
    {
        if (points == null || points.Length < 2) return;

        controlRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            controlRenderer.SetPosition(i, points[i].transform.position);
        }
    }

    private Vector3[] DeCasteljau(float t, Vector3[] positions)
    {
        if (positions.Length == 1) return positions;

        Vector3[] newPoints = new Vector3[positions.Length - 1];
        for (int i = 0; i < newPoints.Length; i++)
            newPoints[i] = (1 - t) * positions[i] + t * positions[i + 1];

        return DeCasteljau(t, newPoints);
    }

    public Vector3 GetPointOnCurve(float t)
    {
        Vector3[] controlPoints = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            controlPoints[i] = points[i].transform.position;

        return DeCasteljau(t, controlPoints)[0];
    }

    public Vector3 EvaluateDerivative(float t)
    {
        int n = points.Length - 1;
        Vector3[] derivativePoints = new Vector3[n];

        for (int i = 0; i < n; i++)
            derivativePoints[i] = n * (points[i + 1].transform.position - points[i].transform.position);

        return DeCasteljau(t, derivativePoints)[0];
    }

    public void ElevateDegree()
    {
        int n = points.Length - 1;
        if (n < 1) return;

        GameObject[] newPoints = new GameObject[n + 2];
        newPoints[0] = CreatePoint(points[0].transform.position);

        for (int i = 1; i <= n; i++)
        {
            float alpha = i / (float)(n + 1);
            Vector3 pos = alpha * points[i - 1].transform.position 
                        + (1 - alpha) * points[i].transform.position;
            newPoints[i] = CreatePoint(pos);
        }

        newPoints[n + 1] = CreatePoint(points[n].transform.position);

        foreach (var go in points) DestroyImmediate(go);
        points = newPoints;
    }

    public void LowerDegree()
    {
        int k = points.Length;
        if (k <= 3) return;

        int n = k - 1;
        float[,] M = new float[k, k - 1];

        for (int i = 0; i < k; i++)
        {
            if (i == 0)
                M[i, 0] = 1f;
            else if (i == n)
                M[i, k - 2] = 1f;
            else
            {
                M[i, i - 1] = i / (float)k;
                M[i, i] = 1f - M[i, i - 1];
            }
        }

        float[,] Mt = Transpose(M);
        float[,] MtM = Multiply(Mt, M);
        float[,] MtMInv = InvertMatrix(MtM);
        if (MtMInv == null) return;

        float[,] V = Multiply(MtMInv, Mt);

        float[,] X = new float[k, 1];
        float[,] Y = new float[k, 1];
        float[,] Z = new float[k, 1];
        for (int i = 0; i < k; i++)
        {
            Vector3 pos = points[i].transform.position;
            X[i, 0] = pos.x;
            Y[i, 0] = pos.y;
            Z[i, 0] = pos.z;
        }

        float[,] newX = Multiply(V, X);
        float[,] newY = Multiply(V, Y);
        float[,] newZ = Multiply(V, Z);

        foreach (var go in points) DestroyImmediate(go);

        GameObject[] newPoints = new GameObject[k - 1];
        for (int i = 0; i < k - 1; i++)
        {
            Vector3 pos = new Vector3(newX[i, 0], newY[i, 0], newZ[i, 0]);
            newPoints[i] = CreatePoint(pos);
        }

        points = newPoints;
    }

    private GameObject CreatePoint(Vector3 position)
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        p.transform.position = position;
        p.transform.localScale = Vector3.one * 0.2f;
        p.name = "Control Point";
        p.transform.parent = transform;

        var renderer = p.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial.color = Color.red;

        return p;
    }

    // Matrix helpers

    private float[,] Transpose(float[,] matrix)
    {
        int rows = matrix.GetLength(0), cols = matrix.GetLength(1);
        float[,] result = new float[cols, rows];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[j, i] = matrix[i, j];

        return result;
    }

    private float[,] Multiply(float[,] A, float[,] B)
    {
        int aRows = A.GetLength(0), aCols = A.GetLength(1);
        int bRows = B.GetLength(0), bCols = B.GetLength(1);
        float[,] result = new float[aRows, bCols];

        for (int i = 0; i < aRows; i++)
            for (int j = 0; j < bCols; j++)
                for (int k = 0; k < aCols; k++)
                    result[i, j] += A[i, k] * B[k, j];

        return result;
    }

    private float[,] InvertMatrix(float[,] matrix)
    {
        int n = matrix.GetLength(0);
        float[,] result = new float[n, n];
        float[,] copy = (float[,])matrix.Clone();

        for (int i = 0; i < n; i++)
            result[i, i] = 1f;

        for (int i = 0; i < n; i++)
        {
            float diag = copy[i, i];
            if (Mathf.Abs(diag) < 1e-6f) return null;

            for (int j = 0; j < n; j++)
            {
                copy[i, j] /= diag;
                result[i, j] /= diag;
            }

            for (int k = 0; k < n; k++)
            {
                if (k == i) continue;
                float factor = copy[k, i];
                for (int j = 0; j < n; j++)
                {
                    copy[k, j] -= factor * copy[i, j];
                    result[k, j] -= factor * result[i, j];
                }
            }
        }

        return result;
    }
}
