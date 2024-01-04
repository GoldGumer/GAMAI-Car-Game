using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEditor.PlayerSettings;

public class PR_Road_Gen : MonoBehaviour
{
    [SerializeField] GameObject roadShape;
    [SerializeField] int splineLength;
    [SerializeField] int roadPointCount;

    GameObject[] roadArray;
    GameObject roadSpline;
    GameObject roadMesh;

    void Start()
    {
        roadArray = new GameObject[roadPointCount + 1];
        roadSpline = new GameObject("Road Spline");
        roadSpline.transform.parent = transform;
        roadSpline.AddComponent<SplineContainer>();
        for (int i = 0; i < splineLength; i++)
        {
            roadSpline.GetComponent<SplineContainer>().Spline.Add(new BezierKnot(Vector3.forward * i * 25));
        }
        roadSpline.GetComponent<SplineContainer>().Spline.SetTangentMode(TangentMode.AutoSmooth);

        roadMesh = new GameObject("Road Mesh");
        roadMesh.AddComponent<MeshFilter>();
        roadMesh.AddComponent<MeshRenderer>();
        roadMesh.GetComponent<MeshFilter>().mesh = new Mesh();

        UpdateMesh();
    }

    private void Update()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        if (roadArray[0] == null)
        {
            for (int i = 0; i <= roadPointCount; i++)
            {
                float3 pos, tang, up;
                roadSpline.GetComponent<SplineContainer>().Evaluate(((float)i) / ((float)roadPointCount), out pos, out tang, out up);

                roadArray[i] = Instantiate(roadShape, pos, Quaternion.identity, transform);
            }
        }
        for (int i = 0; i <= roadPointCount; i++)
        {
            float3 pos, tang, up;
            roadSpline.GetComponent<SplineContainer>().Evaluate(((float)i) / ((float)roadPointCount), out pos, out tang, out up);

            roadArray[i].transform.position = pos;

            if (i != roadPointCount)
            {
                roadArray[i].transform.LookAt(roadArray[i + 1].transform);
            }
            else
            {
                roadArray[i].transform.rotation = roadArray[i - 1].transform.rotation;
            }
        }
        Vector3[] roadVertexList = new Vector3[roadArray.Length * roadShape.transform.childCount];
        foreach (var roadSegment in roadArray)
        {
            for (int i = 0; i < roadSegment.transform.childCount; i++)
            {
                roadVertexList[i] = roadSegment.transform.GetChild(i).position;
            }
        }
        List<int> roadTriangleList = new List<int>();
        for (int i = 0; i < roadVertexList.Length - 2; i++)
        {
            int a = i;
            int b = i + roadShape.transform.childCount;
            int d = (i + 1) % roadShape.transform.childCount;
            int c = d + roadShape.transform.childCount;

            roadTriangleList.Add(a);
            roadTriangleList.Add(b);
            roadTriangleList.Add(c);
            roadTriangleList.Add(a);
            roadTriangleList.Add(c);
            roadTriangleList.Add(d);
            if (i <= (roadShape.transform.childCount - 2))
            {

            }
            else if (i >= ((roadShape.transform.childCount * roadPointCount) - 2))
            {

            }
            else
            {
                
            }
        }
        roadMesh.GetComponent<MeshFilter>().mesh.Clear();
        roadMesh.GetComponent<MeshFilter>().mesh.vertices = roadVertexList;
        roadMesh.GetComponent<MeshFilter>().mesh.triangles = roadTriangleList.ToArray();
        roadMesh.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }
}
