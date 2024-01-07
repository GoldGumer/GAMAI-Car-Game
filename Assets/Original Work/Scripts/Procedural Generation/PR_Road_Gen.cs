using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PR_Road_Gen : MonoBehaviour
{
    [SerializeField] GameObject roadShapeStart, roadShapeMiddle;
    [SerializeField] int splineLength;
    [SerializeField] int roadPointCount;
    [SerializeField] Material roadMaterial;
    [SerializeField] float randomness;
    [SerializeField] Vector3[] splinePoints;


    GameObject[] roadArray;
    GameObject roadSpline;
    GameObject roadMesh;

    void Start()
    {
        roadArray = new GameObject[1];
        roadSpline = new GameObject("Road Spline");
        roadSpline.transform.parent = transform;
        roadSpline.AddComponent<SplineContainer>();

        if (splinePoints.Length == 0)
        {
            splinePoints = new Vector3[splineLength];
            for (int i = 0; i < splineLength; i++)
            {
                if (i == 0)
                {
                    splinePoints[i] = new Vector3(UnityEngine.Random.Range(-randomness, randomness), UnityEngine.Random.Range(-randomness, randomness), 25 * i);
                }
                else
                {
                    splinePoints[i] = new Vector3(splinePoints[i - 1].x + UnityEngine.Random.Range(-randomness, randomness), splinePoints[i - 1].y + UnityEngine.Random.Range(-randomness, randomness), 25 * i);
                }
            }
        }

        foreach (Vector3 point in splinePoints)
        {
            roadSpline.GetComponent<SplineContainer>().Spline.Add(new BezierKnot(point));
        }

        roadSpline.GetComponent<SplineContainer>().Spline.SetTangentMode(TangentMode.AutoSmooth);

        roadMesh = new GameObject("Road Mesh");
        roadMesh.transform.position = new Vector3(0, 0, 0);
        roadMesh.AddComponent<MeshFilter>();
        roadMesh.AddComponent<MeshRenderer>();
        roadMesh.AddComponent<MeshCollider>();
        roadMesh.AddComponent<Rigidbody>();

        roadMesh.GetComponent<Rigidbody>().useGravity = false;
        roadMesh.GetComponent<Rigidbody>().isKinematic = true;
        roadMesh.GetComponent<MeshFilter>().mesh = new Mesh();
        roadMesh.GetComponent<MeshRenderer>().material = roadMaterial;

        GenerateRoadSegments();
        UpdateMesh();
        DestroyRoadSegments();
    }

    private void Update()
    {

    }

    private void UpdateMesh()
    {
        List<Vector3> roadVertexList = new List<Vector3>();
        List<int> roadTriangleList = new List<int>();
        
        for (int i = 0; i < roadArray.Length; i++)
        {
            roadVertexList.AddRange(roadArray[i].GetComponent<PR_Road_Segment>().GetVertexList());
            List<int> roadQuadList = roadArray[i].GetComponent<PR_Road_Segment>().GetQuadList();
            for (int j = 0; j < roadQuadList.Count; j += 4)
            {
                roadTriangleList.AddRange(QuadToTris(
                    roadArray[i].GetComponent<PR_Road_Segment>().GetVertexList().Count * i + roadQuadList.ToArray()[j + 3],
                    roadArray[i].GetComponent<PR_Road_Segment>().GetVertexList().Count * i + roadQuadList.ToArray()[j + 2],
                    roadArray[i].GetComponent<PR_Road_Segment>().GetVertexList().Count * i + roadQuadList.ToArray()[j + 1],
                    roadArray[i].GetComponent<PR_Road_Segment>().GetVertexList().Count * i + roadQuadList.ToArray()[j]));
            }
        }

        roadMesh.GetComponent<MeshFilter>().mesh.Clear();
        roadMesh.GetComponent<MeshFilter>().mesh.vertices = roadVertexList.ToArray();
        roadMesh.GetComponent<MeshFilter>().mesh.triangles = roadTriangleList.ToArray();
        roadMesh.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        roadMesh.GetComponent<MeshFilter>().mesh.RecalculateTangents();
        roadMesh.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        roadMesh.GetComponent<MeshCollider>().sharedMesh = roadMesh.GetComponent<MeshFilter>().mesh;
    }

    private void GenerateRoadSegments()
    {
        if (roadArray[0] == null)
        {
            roadArray = new GameObject[roadPointCount];
            for (int i = 0; i < roadPointCount; i++)
            {
                float3 pos, tang, up;
                roadSpline.GetComponent<SplineContainer>().Evaluate(((float)i) / ((float)roadPointCount), out pos, out tang, out up);

                if (i == 0)
                {
                    roadArray[i] = Instantiate(roadShapeStart, pos, Quaternion.identity, transform);
                }
                else if (i == roadPointCount - 1)
                {
                    roadArray[i] = Instantiate(roadShapeStart, pos, Quaternion.identity, transform);
                }
                else
                {
                    roadArray[i] = Instantiate(roadShapeMiddle, pos, Quaternion.identity, transform);
                }

                int childCount = roadArray[i].transform.childCount;

                GameObject frontHalf = new GameObject("Front Half");
                frontHalf.transform.position = new Vector3(roadArray[i].transform.position.x, roadArray[i].transform.position.y, roadArray[i].transform.GetChild(0).position.z);
                frontHalf.transform.parent = roadArray[i].transform;

                GameObject endHalf = new GameObject("End Half");
                endHalf.transform.position = new Vector3(roadArray[i].transform.position.x, roadArray[i].transform.position.y, roadArray[i].transform.GetChild(roadArray[i].transform.childCount / 2).position.z);
                endHalf.transform.parent = roadArray[i].transform;

                for (int j = 0; j < childCount; j++)
                {
                    if (j < childCount / 2)
                    {
                        roadArray[i].transform.GetChild(0).parent = frontHalf.transform;
                    }
                    else
                    {
                        roadArray[i].transform.GetChild(0).parent = endHalf.transform;
                    }
                }
            }
            for (int i = 0; i < roadPointCount; i++)
            {
                if (i < roadPointCount - 1)
                {
                    roadArray[i].transform.GetChild(0).LookAt(roadArray[i + 1].transform.GetChild(0).position);
                }
                else
                {
                    roadArray[i].transform.GetChild(0).rotation = roadArray[i - 1].transform.GetChild(0).rotation;
                }
            }
            for (int i = 0; i < roadPointCount; i++)
            {
                if (i < roadPointCount - 1)
                {
                    roadArray[i].transform.GetChild(1).position = roadArray[i + 1].transform.GetChild(0).position;
                    roadArray[i].transform.GetChild(1).rotation = roadArray[i + 1].transform.GetChild(0).rotation;
                }
                else
                {
                    float3 pos, tang, up;
                    roadSpline.GetComponent<SplineContainer>().Evaluate(1.0f, out pos, out tang, out up);
                    roadArray[i].transform.GetChild(1).position = pos;
                    roadArray[i].transform.GetChild(1).rotation = roadArray[i].transform.GetChild(0).rotation;
                }
            }   
        }
    }

    private void DestroyRoadSegments()
    {
        for (int i = 0; i < roadArray.Length; i++)
        {
            GameObject.Destroy(roadArray[i]);
            roadArray[i] = null;
        }
    }

    private List<int> QuadToTris(int a, int b, int c, int d)
    {
        List<int> ints = new List<int>();

        ints.Add(b);
        ints.Add(a);
        ints.Add(d);

        ints.Add(b);
        ints.Add(d);
        ints.Add(c);

        return ints;
    }

    public Vector3[] GetSplinePoints()
    {
        return splinePoints;
    }
}
