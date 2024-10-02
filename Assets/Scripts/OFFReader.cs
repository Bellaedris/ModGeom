using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;

public struct OFFMesh
{
    public Mesh _m;
    public Vector3 _centroid;
}

public class OFFReader
{
    public static OFFMesh ReadFile(string filename, bool useFaceNormals)
    {
        OFFMesh ret = new OFFMesh();
        Mesh m = new Mesh();
        Vector3 centroid = Vector3.zero;
        
        using (StreamReader reader = new StreamReader(Application.dataPath + "/Models/" + filename + ".off"))
        {
            //ignore first line
            reader.ReadLine();
            
            //second line holds the number of vertices/faces
            string[] tokens = reader.ReadLine().Split(' ');
            int numVertices = Int32.Parse(tokens[0]);
            int numFaces = Int32.Parse(tokens[1]);

            var vertices = new List<Vector3>(numVertices);
            var triangles = new List<int>(numFaces);
            var normals = new List<Vector3>(numVertices);

            var max = Vector3.zero;
            // from now on, we read all vertices, then all faces
            for (int i = 0; i < numVertices; i++)
            {
                tokens = reader.ReadLine().Split(' ');
                
                var vert = new Vector3(
                        float.Parse(tokens[0], CultureInfo.InvariantCulture.NumberFormat), 
                        float.Parse(tokens[1], CultureInfo.InvariantCulture.NumberFormat), 
                        float.Parse(tokens[2], CultureInfo.InvariantCulture.NumberFormat)
                    );
                // accumulate positions to calculate the barycenter of the vertices
                centroid += vert;
                
                // keep track of the furthest vertex to normalize the model later on
                if (vert.sqrMagnitude > max.sqrMagnitude) 
                    max = vert;
                
                vertices.Add(vert);
                // we add an empty vertex to the normals to initialize them
                normals.Add(Vector3.zero);
            }
            
            // normalize the coordinates
            float maxMagnitude = max.magnitude;
            for (int i = 0; i < numVertices; i++)
                vertices[i] /= maxMagnitude;
            
            for (int i = 0; i < numFaces; i++)
            {
                tokens = reader.ReadLine().Split(' ');
                
                // the 0th element describes the number of element in the primitive. 
                // here, we assume we are only using triangles
                int a = Int32.Parse(tokens[1]);
                int b = Int32.Parse(tokens[2]);
                int c = Int32.Parse(tokens[3]);
                
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
                
                Vector3 faceNormal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                normals[a] += faceNormal;
            }

            // recalculate normals
            for (int i = 0; i < numVertices; i++)
                normals[i].Normalize();
            
            m.SetVertices(vertices);
            if(useFaceNormals)
                m.SetNormals(normals);
            else
                m.RecalculateNormals();
            m.SetTriangles(triangles, 0);

            ret._m = m;
            // barycenter is the mean of all vertex positions
            ret._centroid = centroid / (float)numVertices;
            
            return ret;
        }
    }

    public static void WriteOFF(string filename, ref OFFMesh input)
    {
        using (StreamWriter writer = new StreamWriter(Application.dataPath + "/Models/" + filename + "Written.off"))
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            
            writer.WriteLine("OFF");
            writer.WriteLine($"{input._m.vertexCount} {input._m.GetIndexCount(0) / 3} 0");

            for (int i = 0; i < input._m.vertexCount; i++)
            {
                Vector3 vert = input._m.vertices[i];
                writer.WriteLine($"{vert.x.ToString(nfi)} " +
                                 $"{vert.y.ToString(nfi)} " + 
                                 $"{vert.z.ToString(nfi)}");
            }

            var indices = input._m.GetIndices(0);
            for (int i = 0; i < input._m.GetIndexCount(0); i += 3)
            {
                writer.WriteLine($"3 " +
                                 $"{indices[i]} " +
                                 $"{indices[i + 1]} " + 
                                 $"{indices[i + 2]}");
            }
        }
    }
    
    public static void WriteOBJ(string filename, ref OFFMesh input)
    {
        using (StreamWriter writer = new StreamWriter(Application.dataPath + "/Models/" + filename + "Written.obj"))
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            
            for (int i = 0; i < input._m.vertexCount; i++)
            {
                Vector3 vert = input._m.vertices[i];
                writer.WriteLine("v " +
                                 $"{vert.x.ToString(nfi)} " +
                                 $"{vert.y.ToString(nfi)} " + 
                                 $"{vert.z.ToString(nfi)}");
            }
            
            for (int i = 0; i < input._m.vertexCount; i++)
            {
                Vector3 normal = input._m.normals[i];
                writer.WriteLine("vn " +
                                 $"{normal.x.ToString(nfi)} " +
                                 $"{normal.y.ToString(nfi)} " + 
                                 $"{normal.z.ToString(nfi)}");
            }

            var indices = input._m.GetIndices(0);
            for (int i = 0; i < input._m.GetIndexCount(0); i += 3)
            {
                // apparently Unity wants to have the normals links to the face too, while meshlab does not care. 
                // Since our vertices and normals share the same index, we can write them both at once
                writer.WriteLine($"f " +
                                 $"{indices[i] + 1}//{indices[i] + 1} " +
                                 $"{indices[i + 1] + 1}//{indices[i + 1] + 1} " + 
                                 $"{indices[i + 2] + 1}//{indices[i + 2] + 1}");
            }
        }
    }

    public static void removeTrianglesFromOFF(ref OFFMesh input)
    {
        Mesh m = new Mesh();
        
        m.SetVertices(input._m.vertices);
        m.SetNormals(input._m.normals);
        
        List<int> indices = new List<int>();

        var oldIndices = input._m.GetIndices(0);
        for (int i = 0; i < oldIndices.Length; i += 6)
        {
            indices.Add(oldIndices[i]);
            indices.Add(oldIndices[i + 1]);
            indices.Add(oldIndices[i + 2]);
        }
        m.SetTriangles(indices.ToArray(), 0);

        input._m = m;
    }
}
