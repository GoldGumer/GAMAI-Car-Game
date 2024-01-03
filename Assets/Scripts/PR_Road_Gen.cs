using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PR_Road_Gen : MonoBehaviour
{
    [SerializeField] GameObject roadShape;
    [SerializeField] int splineLength;
    GameObject roadSpline;

    // Start is called before the first frame update
    void Start()
    {
        roadSpline = new GameObject("Road Spline");
        roadSpline.transform.parent = transform;
        roadSpline.AddComponent<SplineContainer>();
        for (int i = 0; i < splineLength; i++)
        {
            roadSpline.GetComponent<SplineContainer>().Spline.Add(new BezierKnot(Vector3.forward * i * 10));
        }
        roadSpline.GetComponent<SplineContainer>().Spline.SetTangentMode(TangentMode.AutoSmooth);
    }

    private void OnDrawGizmos()
    {
        if (roadSpline != null)
        {
            for (float i = 0; i < 1; i += 0.001f)
            {
                float3 pos1, tang1, up1, pos2, tang2, up2;
                roadSpline.GetComponent<SplineContainer>().Evaluate(i, out pos1, out tang1, out up1);
                roadSpline.GetComponent<SplineContainer>().Evaluate(i + 0.001f, out pos2, out tang2, out up2);
                foreach (var knot in roadShape.GetComponent<SplineContainer>().Spline.Knots)
                {
                    Gizmos.DrawLine(pos1 + knot.Position, pos2 + knot.Position);
                }
            }
        }
    }
}
