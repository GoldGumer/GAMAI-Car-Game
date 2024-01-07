using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Wall_Mover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints()[0])
        {
            transform.position = GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints()[0];
            transform.LookAt(GameObject.FindGameObjectWithTag("Road Generator").GetComponent<PR_Road_Gen>().GetSplinePoints()[1]);
        }
    }
}
