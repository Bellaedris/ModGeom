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
    }

    private void OnInspectorUpdate() 
    {
        MeshHandler generator = (MeshHandler) target;
        
    }
}
