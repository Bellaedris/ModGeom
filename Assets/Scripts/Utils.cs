using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Mesh CreateMeshFromBounds(Bounds bounds)
    {
        Mesh mesh = new Mesh();

        // Get the 8 corner points of the bounding box
        Vector3[] vertices = new Vector3[8];
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        vertices[0] = new Vector3(min.x, min.y, min.z); // Bottom left near
        vertices[1] = new Vector3(max.x, min.y, min.z); // Bottom right near
        vertices[2] = new Vector3(max.x, max.y, min.z); // Top right near
        vertices[3] = new Vector3(min.x, max.y, min.z); // Top left near
        vertices[4] = new Vector3(min.x, min.y, max.z); // Bottom left far
        vertices[5] = new Vector3(max.x, min.y, max.z); // Bottom right far
        vertices[6] = new Vector3(max.x, max.y, max.z); // Top right far
        vertices[7] = new Vector3(min.x, max.y, max.z); // Top left far
        
        // Define the triangles (two triangles per face, 12 triangles total)
        int[] triangles = {
            // Front face
            0, 2, 1,
            0, 3, 2,
            // Back face
            4, 5, 6,
            4, 6, 7,
            // Left face
            0, 7, 3,
            0, 4, 7,
            // Right face
            1, 2, 6,
            1, 6, 5,
            // Top face
            3, 7, 6,
            3, 6, 2,
            // Bottom face
            0, 1, 5,
            0, 5, 4
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        return mesh;
    }

    public static float Normalize01(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}
