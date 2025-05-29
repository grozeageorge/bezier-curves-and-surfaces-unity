using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BezierCurve curve = (BezierCurve)target;

        GUILayout.Space(10);
        GUILayout.Label("Degree Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Elevate Degree"))
        {
            Undo.RecordObject(curve, "Elevate Degree");
            curve.ElevateDegree();
            EditorUtility.SetDirty(curve);
        }

        if (GUILayout.Button("Lower Degree"))
        {
            Undo.RecordObject(curve, "Lower Degree");
            curve.LowerDegree();
            EditorUtility.SetDirty(curve);
        }
    }
}