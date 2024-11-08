using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TryRotateTowards : MonoBehaviour
{
    public int choice = 0;

    public Transform otherTransform;
    public Rigidbody rb;

    public float rotSpeed;
    public Vector3 normal = Vector3.up;
    public Vector3 normalGoal = Vector3.up;
    public float speed_normalLerp = 0.5f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if( otherTransform != null )
        {
            print("hay");
            //normal = Vector3.Lerp( normal, normalGoal, speed_normalLerp * Time.fixedDeltaTime );
            normal = Vector3.RotateTowards(normal, normalGoal, speed_normalLerp * Time.fixedDeltaTime, 0f);
            if( choice == 0 )
            {
                rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(PV_Utilities.FlatVect(transform.forward), PV_Utilities.FlatVect(otherTransform.position - transform.position), rotSpeed * Time.fixedDeltaTime, 0.0f)) );
            }
            else if( choice == 1 )
            {
                rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, Vector3.Normalize(otherTransform.position - transform.position), rotSpeed * Time.fixedDeltaTime, 0.0f), normal) );
            }
            else if( choice == 2 )
            {
                //The following causes the sphere to stop moving...
                rb.MoveRotation(Quaternion.LookRotation(Vector3.RotateTowards(PV_Utilities.FlatVect(transform.forward), PV_Utilities.FlatVect(otherTransform.position - transform.position), rotSpeed * Time.fixedDeltaTime, 0.0f)));
                rb.MoveRotation( Quaternion.LookRotation(transform.forward, normal) ); 
            }
            else if ( choice == 3 )
            {
                //The following causes the sphere to stop moving...
                rb.MoveRotation(Quaternion.LookRotation(Vector3.RotateTowards(PV_Utilities.FlatVect(transform.forward), PV_Utilities.FlatVect(otherTransform.position - transform.position), rotSpeed * Time.fixedDeltaTime, 0.0f)));
                rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, transform.forward, rotSpeed * Time.fixedDeltaTime, 0f), normal) );
            }

        }
    }

    public float gizmoLength = 1.2f;
    public Vector3 handlePos = Vector3.zero;

    private void OnDrawGizmos()
    {


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLength);
        Handles.Label(transform.position + (transform.forward * gizmoLength), $"lclFwd(scaled)\n{transform.forward * gizmoLength}");

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * gizmoLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3.Normalize(normalGoal) * gizmoLength) );
    }

    [ContextMenu("call GoForward()")]
    public void GoForward()
    {
        otherTransform.position = transform.position + Vector3.forward;
        //normal = Vector3.up;
        normalGoal = Vector3.up;
    }

    [ContextMenu("call GoBackward()")]
    public void GoBackward()
    {
        otherTransform.position = transform.position + Vector3.back;
        //normal = Vector3.up;
        normalGoal = Vector3.up;
    }

    [ContextMenu("call GoUp()")]
    public void GoUp()
    {
        otherTransform.position = transform.position + Vector3.up;
        //normal = Vector3.back;
        normalGoal = Vector3.back;
    }

    [ContextMenu("call GoDown()")]
    public void GoDown()
    {
        otherTransform.position = transform.position + Vector3.down;
        //normal = Vector3.forward;
        normalGoal = Vector3.forward;
    }

    [ContextMenu("call GoLeft()")]
    public void GoLeft()
    {
        otherTransform.position = transform.position + Vector3.left;
        //normal = Vector3.up;
        normalGoal = Vector3.up;
    }

    [ContextMenu("call GoRight()")]
    public void GoRight()
    {
        otherTransform.position = transform.position + Vector3.right;
        //normal = Vector3.up;
        normalGoal = Vector3.up;
    }
}
