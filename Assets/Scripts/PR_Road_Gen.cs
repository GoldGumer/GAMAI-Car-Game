using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PR_Road_Gen : MonoBehaviour
{
    [SerializeField] GameObject roadShape;
    [SerializeField] int bezierSegments;
    static int arraySize = 6;
    GameObject[] roadPoints = new GameObject[arraySize];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < arraySize; i++)
        {
            roadPoints[i] = new GameObject("Road point " + i);
            roadPoints[i].transform.parent = transform;
            roadPoints[i].transform.position = Vector3.forward * i * 10;
            Instantiate(roadShape, roadPoints[i].transform.position, roadPoints[i].transform.rotation, roadPoints[i].transform);
        }
        
    }

    Vector3[] Bezier(Transform a, Transform b, int numPoints)
    {
        Vector3[] points = new Vector3[numPoints + 1];
        Vector3 c = a.position + a.forward * 5;
        for (int i = 0; i <= numPoints; i++)
        {
            float t = (i / (float)numPoints);
            points[i] = Mathf.Pow(1 - t, 2) * a.position + 2 * (1 - t) * t * c + Mathf.Pow(t, 2) * b.position;
        }
        return points;
    }

    private void OnDrawGizmos()
    {
        if (roadPoints[0] != null)
        {
            
            for (int i = 0; i < arraySize - 1; i++)
            {
                Vector3[] points = Bezier(roadPoints[i].transform, roadPoints[i + 1].transform, bezierSegments);
                for (int j = 0; j < bezierSegments; j++)
                {
                    Gizmos.DrawLine(points[j], points[j + 1]);
                }
            }
            //    for (int j = 0; j < roadShape.transform.childCount; j++)
            //    {
            //        Gizmos.color = Color.magenta;
            //        Transform currentRoadShape = roadPoints[i].transform.GetChild(0);
            //        Transform nextRoadShape = roadPoints[i + 1].transform.GetChild(0);
            //        Gizmos.DrawLine(currentRoadShape.GetChild(j).transform.position, nextRoadShape.GetChild(j).transform.position);
            //    }
        }
    }
}
