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

    [Header("Sphere")]
    public float SphereRadius = 5f;

    public int sphereStacks = 12;
    
    public int sphereMeridians = 24;
    
    
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
                mesh = GenerateSphere();
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
        var triangles = new List<int>();
        
        // both ends
        float step = Mathf.Deg2Rad * (360f / cylinderMeridians);
        
        for (int i = 0; i < cylinderMeridians; i++)
        {
            vertices.Add(new Vector3(Mathf.Cos(i * step), 0f, Mathf.Sin(i * step)));
            vertices.Add(new Vector3(Mathf.Cos(i * step), cylinderHeight, Mathf.Sin(i * step)));
        }
        
        // add a vertex at the center of each ends of the cylinder
        // penultimate vertex is the bottom center, last one is the top center.
        vertices.Add(new Vector3(0f, 0f, 0f));
        vertices.Add(new Vector3(0f, cylinderHeight, 0f));
        
        //utilities to stores the indices of the centers
        int topCenterIndex = vertices.Count - 1;
        int bottomCenterIndex = vertices.Count - 2;
        
        // indices
        for (int i = 0; i < cylinderMeridians - 1; i++)
        {
            // "body" of the cylinder
            int index = i * 2;
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            
            triangles.Add(index + 1);
            triangles.Add(index + 3);
            triangles.Add(index + 2);
            
            // "closures" of the cylinder
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(bottomCenterIndex);
            
            triangles.Add(index + 1);
            triangles.Add(topCenterIndex);
            triangles.Add(index + 3);
        }
        //last triangles (better to do this compared to a modulo every step)
        int lastIndex = (cylinderMeridians - 1) * 2;
        triangles.Add(lastIndex);
        triangles.Add(lastIndex + 1);
        triangles.Add(0);
        
        triangles.Add(lastIndex + 1);
        triangles.Add(1);
        triangles.Add(0);
        
        // "closures" of the cylinder
        triangles.Add(lastIndex);
        triangles.Add(0);
        triangles.Add(bottomCenterIndex);
            
        triangles.Add(lastIndex + 1);
        triangles.Add(topCenterIndex);
        triangles.Add(1);

        Debug.Log((lastIndex + 1) + " " + topCenterIndex + " " + (lastIndex + 3));
        
        mesh.SetVertices(vertices.ToArray());
        mesh.RecalculateNormals();
        mesh.SetTriangles(triangles.ToArray(), 0);

        return mesh;
    }

    public Mesh GenerateSphere()
    {
        Mesh mesh = new Mesh();

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        
        // both ends
        float stepMeridians = 2f * Mathf.PI / (float)sphereMeridians;
        float stepStacks = Mathf.PI / (float)sphereStacks;

        float x, y, z;
        for(int i = 0; i < sphereStacks; i++)
        {
            y = Mathf.Cos(i * stepStacks);
            float sinPhi = Mathf.Sin(i * stepStacks);
            for(int j = 0; j < sphereMeridians; j++)
            {
                x = Mathf.Cos(j * stepMeridians) * sinPhi;
                z = Mathf.Sin(j * stepMeridians) * sinPhi;
                Vector3 vert = new Vector3(x, y, z);
                vertices.Add(vert * SphereRadius);
                normals.Add(Vector3.zero - vert);
            }
        }

        // bottom/top vertices
        vertices.Add(new Vector3(0, -SphereRadius, 0));
        normals.Add(Vector3.down);
        vertices.Add(new Vector3(0, SphereRadius, 0));
        normals.Add(Vector3.up);

        for (int i = 0; i < sphereStacks - 1; i++)
        {
            int ind = i * sphereMeridians;
            for (int j = 0; j < sphereMeridians; j++)
            {
                int current = ind + j;
                int next = ind + (j + 1) % sphereMeridians;
                triangles.Add(current + sphereMeridians);
                triangles.Add(current);
                triangles.Add(next + sphereMeridians);
               
                triangles.Add(next + sphereMeridians);
                triangles.Add(current);
                triangles.Add(next);
            }
        }

        // top/bottom triangles
        for(int i = 0; i < sphereMeridians; i++)
        {
            triangles.Add(i);
            triangles.Add((i + 1) % sphereMeridians);
            triangles.Add(vertices.Count - 1);
        
            int lastStackBegin = sphereMeridians * (sphereStacks - 1);
            triangles.Add(lastStackBegin + i);
            triangles.Add(lastStackBegin + (i + 1) % sphereMeridians);
            triangles.Add(vertices.Count - 2);
        }

        mesh.SetVertices(vertices.ToArray());
        //mesh.RecalculateNormals();
        mesh.SetNormals(normals.ToArray());
        mesh.SetTriangles(triangles.ToArray(), 0);

        return mesh;   
    }
}