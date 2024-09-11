using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeGenerator))]
public class ShapeGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShapeGenerator generator = (ShapeGenerator) target;
        if (GUILayout.Button("Generate"))
            generator.GenerateShape();
    }
    
}