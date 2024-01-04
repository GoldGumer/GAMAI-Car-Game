using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PR_Road_Cross_Section_Gizmo_Wire : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild((i + 1) % transform.childCount).position);
        } 
    }
}
