using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class tryFacing : MonoBehaviour
{
    public Transform T_Perspective;
    public Transform T_FacingGoal;
    public Vector3 V_Result;

    public int i_mode = 0;  

    void Start()
    {
        
    }

    void Update()
    {
        GetFacing();
    }

    [ContextMenu("GetFacing()")]
    public void GetFacing()
    {
        if (T_FacingGoal == null || T_Perspective == null)
        {
            return;
        }

        if( i_mode == 0 )
        {
            V_Result = T_FacingGoal.position - T_Perspective.position;
        }
        else if ( i_mode == 1 ) 
        { 
            V_Result = T_Perspective.InverseTransformPoint(T_FacingGoal.position);
        }
        else if( i_mode == 2 )
        {
            V_Result = T_Perspective.InverseTransformDirection(T_FacingGoal.position);
        }
    }

    public float gizmoLength = 1.2f;
    private void OnDrawGizmos()
    {
        if (  T_FacingGoal == null || T_Perspective == null)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(T_Perspective.position, T_Perspective.position+T_Perspective.forward * gizmoLength);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(T_Perspective.position, T_Perspective.position + T_Perspective.right * gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(T_Perspective.position, T_Perspective.position + T_Perspective.up * gizmoLength);
    }
}
