using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Voxelizer))]
public class VoxelizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Voxelizer generator = (Voxelizer) target;
        if (GUILayout.Button("Generate"))
            generator.Voxelize();
        if (GUILayout.Button("Clear Voxels"))
        {
            generator.Clear();
        }
    }

    private void OnInspectorUpdate() 
    {
        // Not used here
        // Voxelizer generator = (Voxelizer) target;
    }
}
