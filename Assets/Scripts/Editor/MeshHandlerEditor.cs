using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshHandler))]
public class MeshHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshHandler generator = (MeshHandler) target;
        if (GUILayout.Button("Generate"))
            generator.HandleOFFFile();
        if (GUILayout.Button("Simplify"))
            generator.Simplify();
    }

    private void OnInspectorUpdate() 
    {
        MeshHandler generator = (MeshHandler) target;
        if (generator.octreeDepth < 3)
            generator.octreeDepth = 3;
    }
}
