using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TryDotReceiver : MonoBehaviour
{
    public TryDot otherScript;
    [TextArea(2, 10)] public string DebugClass;

    public float gizmoLength = 1.2f;

    void OnDrawGizmos()
    {
        if( otherScript )
        {
            DebugClass = otherScript.DebugClass;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLength);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * gizmoLength);

        Handles.Label(transform.position, transform.position.ToString());

    }
}
