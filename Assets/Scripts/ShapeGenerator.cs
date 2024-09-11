using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShapeGenerator : MonoBehaviour
{

    public enum Shape
    {
        Plane,
        Cylinder,
        Sphere,
        Cone,
        Pacman
    }

    public Shape shapeToGenerate;
    public Material mat;

    [Header("Plane")] 
    public float width = 5f;
    public float height = 5f;
    public int nx = 20;
    public int ny = 20;
    
    [Header("Cylinder")]
    public float cylinderRadius = 5f;

    public float cylinderHeight = 5f;
    
    public int cylinderMeridians = 12;
    
    
    public void GenerateShape()
    {
        Mesh mesh = new Mesh();
        
        switch (shapeToGenerate)
        {
            case Shape.Plane:
                mesh = GeneratePlane();
                break;
            case Shape.Cylinder:
                mesh = GenerateCylinder();
                break;
            case Shape.Sphere:
                mesh = GeneratePlane();
                break;
            case Shape.Cone:
                mesh = GeneratePlane();
                break;
            case Shape.Pacman:
                mesh = GeneratePlane();
                break;
        }
        
        GameObject shape = new GameObject(shapeToGenerate.ToString());

        MeshFilter filter = shape.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        var renderer = shape.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
    }

    public Mesh GeneratePlane()
    {
        Mesh mesh = new Mesh();
        
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        
        float stepX = width / (float)(nx - 1);
        float stepY = height / (float)(ny - 1);
        
        for(int i = 0; i < nx; i++)
        for (int j = 0; j < ny; j++)
        {
            vertices.Add(new Vector3((float)i * stepX, (float)j * stepY, 0));
            normals.Add(new Vector3(0f, 1f, 0f));
        }

        for(int j = 0; j < nx - 1; j++)
        for (int i = 0; i < ny - 1; i++)
        {
            int index = (j * nx) + i;
            triangles.Add(index);
            triangles.Add(index + nx);
            triangles.Add(index + nx + 1);
           
            triangles.Add(index);
            triangles.Add(index  + nx + 1);
            triangles.Add(index + 1);
        }
        
        mesh.SetVertices(vertices.ToArray());
        mesh.SetNormals(normals.ToArray());
        mesh.SetTriangles(triangles.ToArray(), 0);

        return mesh;
    }

    public Mesh GenerateCylinder()
    {
        Mesh mesh = new Mesh();
        
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        
        // both ends
        float step = Mathf.Deg2Rad * (360f / cylinderMeridians);
        
        // add a vertex at the center of each ends of the cylinder
        vertices.Add(new Vector3(0f, 0f, 0f));
        vertices.Add(new Vector3(0f, cylinderHeight, 0f));
        for (int i = 0; i < cylinderMeridians; i++)
        {
            vertices.Add(new Vector3(Mathf.Cos(i * step), 0f, Mathf.Sin(i * step)));
            Debug.Log(new Vector3(Mathf.Cos(i * step), 0f, Mathf.Sin(i * step)));
            //vertices.Add(new Vector3(Mathf.Cos(i * step), cylinderHeight, Mathf.Sin(i * step)));
        }
        
        // indices
        for (int i = 2; i < cylinderMeridians; i++)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(0);
        }
        
        mesh.SetVertices(vertices.ToArray());
        mesh.RecalculateNormals();
        mesh.SetTriangles(triangles.ToArray(), 0);

        return mesh;
    }
}
