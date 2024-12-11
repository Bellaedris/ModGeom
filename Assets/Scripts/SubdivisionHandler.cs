using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubdivisionHandler : MonoBehaviour
{

    public GameObject[] controlPoints;
    public int subdivisionLevel;
    
    public void Subdivide()
    {
        List<Vector3> newPoints;
        List<Vector3> points = new List<Vector3>();
        foreach (GameObject point in controlPoints)
            points.Add(point.transform.position);
        
        for (int i = 0; i < subdivisionLevel; i++)
        {
            newPoints = new List<Vector3>();
            for(int j = 0; j < points.Count - 1; j++)
            {
                newPoints.Add(.75f * points[j] + .25f * points[j + 1]);
                newPoints.Add(.25f * points[j] + .75f * points[j + 1]);
            }
            newPoints.Add(.75f * points[^1] + .25f * points[0]);
            newPoints.Add(.25f * points[^1] + .75f * points[0]);

            points = newPoints;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            Debug.DrawLine(points[i], points[i + 1], Color.red, 10f);
        }
        Debug.DrawLine(points[0], points[^1], Color.red, 10f);
    }
    
}
