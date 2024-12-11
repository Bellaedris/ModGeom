using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParametricHandler : MonoBehaviour
{

    public enum CurveType
    {
        Hermit,
        Bezier
    }

    public CurveType type;
    [Header("Hermit curve")] 
    public GameObject hermitP0;
    public GameObject hermitP1;
    public Vector3 hermitV0;
    public Vector3 hermitV1;
    public int hermitPoints;
    
    public GameObject[] controlPoints;
    public int subdivisionLevel;
    
    public void GenerateCurve()
    {
        switch (type)
        {
            case CurveType.Hermit:
                GenerateHermit(hermitP0.transform.position, hermitP1.transform.position, hermitV0, hermitV1, hermitPoints);
                break;
            case CurveType.Bezier:
                GenerateBezier(controlPoints, subdivisionLevel);
                break;
        }
    }

    private void GenerateHermit(Vector3 u1, Vector3 u2, Vector3 v1, Vector3 v2, int subdivisionNumber)
    {
        List<Vector3> points = new List<Vector3>();
        
        Matrix4x4 B = new Matrix4x4(
            new Vector4(u1.x, u1.y, u1.z, 1f),
            new Vector4(u2.x, u2.y, u2.z, 1f),
            new Vector4(v1.x, v1.y, v1.z, 1f),
            new Vector4(v2.x, v2.y, v2.z, 1f)
        );
        float step = 1f / (float)subdivisionNumber;
        for (int i = 0; i < subdivisionNumber; i++)
        {
            float u = step * i;
            Vector4 vecU = new Vector4(u * u * u, u * u, u, 1f);
            Matrix4x4 M = new Matrix4x4(
                new Vector4(2f, -2f, 1f, 1f),
                new Vector4(-3f, 3f, -2f, -1f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(1f, 0f, 0f, 0f)
            );
            
            points.Add(B * M * vecU);
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            Debug.DrawLine(points[i], points[i + 1], Color.yellow);
        }
    }

    void GenerateBezier(GameObject[] control, int subdivisionNumber)
    {
        List<Vector3> points = new List<Vector3>();
        
        float step = 1f / subdivisionNumber;
        int n = control.Length - 1;
        for (int i = 0; i <= subdivisionNumber; i++)
        {
            float t = i * step;

            Vector3 res = Vector3.zero;
            for (int j = 0; j <= n; j++)
            {
                // calculate the bernstein polynomial
                res += BinomialCoef(j, n) 
                       * Mathf.Pow(t, j) 
                       * Mathf.Pow(1 - t, n - j)
                       * control[j].transform.position ;
            }
            
            points.Add(res);
        }
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            Debug.DrawLine(points[i], points[i + 1], Color.yellow, 5f);
        }
    }

    float BinomialCoef(int i, int n)
    {
        return Fact(n) / (Fact(i) * Fact(n - i));
    }

    float Fact(int n)
    {
        float res = 1f;
        for (int i = 1; i <= n; i++)
        {
            res *= i;
        }

        return res;
    }
    
}
