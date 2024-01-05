using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEditor.PlayerSettings;

public class PR_Road_Gen : MonoBehaviour
{
    [SerializeField] GameObject roadShape;
    [SerializeField] int splineLength;
    [SerializeField] int roadPointCount;
    [SerializeField] Material roadMaterial;

    GameObject[] roadArray;
    GameObject roadSpline;
    GameObject roadMesh;

    void Start()
    {
        roadArray = new GameObject[1];
        roadSpline = new GameObject("Road Spline");
        roadSpline.transform.parent = transform;
        roadSpline.AddComponent<SplineContainer>();
        for (int i = 0; i < splineLength; i++)
        {
            roadSpline.GetComponent<SplineContainer>().Spline.Add(new BezierKnot(Vector3.forward * i * 25));
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
        GenerateRoadSegments();
        UpdateMesh();
        DestroyRoadSegments();
    }

    private void UpdateMesh()
    {
        List<Vector3> roadVertexList = roadShape.GetComponent<PR_Road_Segment>().GetVertexList();
        List<int> roadQuadList = roadShape.GetComponent<PR_Road_Segment>().GetQuadList();
        List<int> roadTriangleList = new List<int>();

        for (int i = 0; i < roadQuadList.Count; i += 4)
        {
            roadTriangleList.AddRange(QuadToTris(roadQuadList.ToArray()[i + 3], roadQuadList.ToArray()[i + 2], roadQuadList.ToArray()[i + 1], roadQuadList.ToArray()[i]));
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

                roadArray[i] = Instantiate(roadShape, pos, Quaternion.identity, transform);

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
}
