using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class TryDot : MonoBehaviour
{
    public Transform other;
    public float MyDot = 0f;
    public Vector3 V_project;

    [TextArea(2, 10)] public string DebugClass;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public float gizmoLength = 1.2f;
    public int choice = 0;
    public Vector3 handlePos = Vector3.zero;
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Gizmos.DrawLine( transform.position, transform.position + transform.forward * gizmoLength);
        Handles.Label( transform.position + (transform.forward * gizmoLength), $"lclFwd(scaled)\n{transform.forward * gizmoLength}" );

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * gizmoLength);

        if ( other )
        {
            Vector3 forward = Vector3.zero;
            Vector3 toOther = Vector3.zero;

            if ( choice == 0 )
            {
                forward = transform.TransformDirection(Vector3.forward);
                toOther = other.position - transform.position;

                MyDot = Vector3.Dot(forward, toOther);
            }
            else if( choice == 1 )
            {
                forward = transform.TransformDirection(Vector3.forward);
                toOther = other.position - transform.position;

                MyDot = Vector3.Dot(forward, toOther.normalized);
            }
            else if (choice == 2)
            {
                MyDot = Vector3.Dot(transform.position, other.position); 
                //Strangely, this does nothing if I move the second sphere, but it does change the dot product if I move the first...
            }
            else if ( choice == 3 )
            {
                MyDot = Vector3.Dot(transform.forward, other.forward); //Dot stays the same unless I rotate one of them...
            }
            else if ( choice  == 4 )
            {
                V_project = Vector3.Project( transform.forward, other.forward );

            }
            else if (choice == 5 )
            {

                V_project = Vector3.Project( transform.position, other.position + other.forward );

            }
            else if (choice == 6 )
            {
                //V_project = Vector3.ProjectOnPlane(transform.forward, other.forward);
                //V_project = Vector3.ProjectOnPlane(transform.right, other.forward);
                V_project = Vector3.ProjectOnPlane(transform.forward, other.right);
            }

            DebugClass = $"forward: '{forward}' - trueFwd: '{transform.forward}'\n" +
                $"toOther: '{toOther}', otherFwd: '{other.forward}'\n" +
                $"MyDot: '{MyDot}'\n" +
                $"V_project: '{V_project}', itp: '{transform.InverseTransformPoint(other.position)}'";

            Handles.Label(transform.position, $"           pos\n{transform.position}");
            //Handles.Label( handlePos, V_project.ToString() );

            if( choice == 3 )
            {
                Handles.Label( transform.position + handlePos, MyDot.ToString() );

            }

            if( choice == 4 || choice == 5 || choice == 6 )
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere( V_project, 0.05f);
                Handles.Label( V_project, $"     V_project\n{V_project}");

            }

        }
    }
}
