using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PR_Road_Segment : MonoBehaviour
{
    [SerializeField] GameObject[] vertices;
    [SerializeField] int[] quadList;

    private void OnDrawGizmos()
    {
        if (quadList != null && vertices.Length >= 4)
        {
            for (int i = 0; i < MathF.Floor(quadList.Length / 4); i++)
            {
                Gizmos.color = Color.white;
                for (int j = 0; j < 4; j++)
                {
                    Gizmos.DrawLine(vertices[quadList[i * 4 + j]].transform.position, vertices[quadList[i * 4 + (j + 1) % 4]].transform.position);
                }
                Gizmos.color = Color.green;
                Gizmos.DrawLine(vertices[quadList[i * 4 + 0]].transform.position, vertices[quadList[i * 4 + 2]].transform.position);
            }
        }
    }

    public List<Vector3> GetVertexList()
    {
        List<Vector3> vertexList = new List<Vector3>();
        foreach (var vertex in vertices)
        {
            vertexList.Add(vertex.transform.position);
        }
        return vertexList;
    }

    public List<int> GetQuadList()
    {
        return quadList.ToList();
    }
}
