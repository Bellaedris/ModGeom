using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SubdivisionHandler))]
public class SubdivisionHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SubdivisionHandler editor = (SubdivisionHandler) target;
        if (GUILayout.Button("Subdivide"))
            editor.Subdivide();
    }
}
