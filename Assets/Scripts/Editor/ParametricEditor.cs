using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParametricHandler))]
public class ParametricEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ParametricHandler generator = (ParametricHandler) target;
        if (GUILayout.Button("Generate"))
            generator.GenerateCurve();
    }

    private void OnInspectorUpdate() 
    {
        // Not used here
        ParametricHandler generator = (ParametricHandler) target;
    }
}
