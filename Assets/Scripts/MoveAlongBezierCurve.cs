using UnityEngine;

/// <summary>
/// Moves a GameObject smoothly along an array of Bezier curves.
/// </summary>
public class MoveAlongBezier : MonoBehaviour
{
    [Header("Bezier Curve Path")]
    public BezierCurve[] bezierCurves;

    [Header("Movement Settings")]
    public float speed = 0.2f;

    private float t = 0f;
    private int currentCurveIndex = 0;

    void Update()
    {
        if (bezierCurves == null || bezierCurves.Length == 0) return;

        BezierCurve currentCurve = bezierCurves[currentCurveIndex];

        t += Time.deltaTime * speed;

        if (t > 1f)
        {
            t = 0f;
            currentCurveIndex = (currentCurveIndex + 1) % bezierCurves.Length;
            currentCurve = bezierCurves[currentCurveIndex];
        }

        MoveAlongCurve(currentCurve);
    }

    private void MoveAlongCurve(BezierCurve curve)
    {
        Vector3 position = curve.GetPointOnCurve(t);
        transform.position = position;

        Vector3[] controlPositions = GetControlPositions(curve);
        Vector3 tangent = curve.EvaluateDerivative(t).normalized;

        if (tangent != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(tangent);
    }

    private Vector3[] GetControlPositions(BezierCurve curve)
    {
        GameObject[] controlPoints = curve.points;
        Vector3[] positions = new Vector3[controlPoints.Length];

        for (int i = 0; i < controlPoints.Length; i++)
            positions[i] = controlPoints[i].transform.position;

        return positions;
    }
}
