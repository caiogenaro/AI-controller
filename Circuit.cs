using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public GameObject[] waypoints;

    private void OnDrawGizmos()
    {
        DrawGizmo(false);
    }
    private void OnDrawGizmosSelected()
    {
        DrawGizmo(true);
    }
    void DrawGizmo(bool selected)
    {
        if (selected == false)
        {
            return;
        }
        else
        {
            Vector3 prev = waypoints[0].transform.position;
            for (int i = 1; i < waypoints.Length; i++)
            {
                Vector3 next = waypoints[i].transform.position;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
            Gizmos.DrawLine(prev, waypoints[0].transform.position);            
        }
    }

}
