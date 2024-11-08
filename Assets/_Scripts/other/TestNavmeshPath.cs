using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TestNavmeshPath : MonoBehaviour
{
    [SerializeField] private Transform trans_startPath;
    [SerializeField] private Stats_DebugLiving nmStats;

    public PV_Path MyPath;

    private void Awake()
    {
        
    }

    [ContextMenu("call UpdatePath()")]
    private void UpdatePath()
    {
        print($"UpdatePath(). start: '{trans_startPath.position}', end: '{transform.position}'-----------------------");
        MyPath.CalculatePath( trans_startPath.position, transform.position );

    }

    [ContextMenu("call DeInitPath()")]
    public void DeInitPath()
    {
        MyPath = new PV_Path();
    }

    private void OnDrawGizmos()
    {
        if ( MyPath == null || MyPath.PathPoints == null || nmStats == null || trans_startPath == null )
        {
            return;
        }

        //not using the debug class so that I can do this while not in play mode.
        Handles.color = nmStats.Color_PathPt;

        for ( int i = 0; i < MyPath.PathPoints.Count; i++ )
        {
            Handles.DrawLine( MyPath.PathPoints[i].V_point, MyPath.PathPoints[i].V_point + (MyPath.PathPoints[i].V_normal * nmStats.Height_PathVerticalLine), nmStats.Width_PathVerticalLine );
            Handles.Label( MyPath.PathPoints[i].V_point + (MyPath.PathPoints[i].V_normal * nmStats.Height_PathVerticalLine * 1.02f), $"\t{i}\n{MyPath.PathPoints[i].V_point}\n" );

            if ( i < MyPath.PathPoints.Count - 1 )
            {
                Vector3 vToNext = MyPath.PathPoints[i + 1].V_point - MyPath.PathPoints[i].V_point;
                Handles.ConeHandleCap( 0, MyPath.PathPoints[i].V_point, Quaternion.LookRotation(vToNext, Vector3.up), nmStats.Size_PatrolArrow, EventType.Repaint );
                Handles.DrawDottedLine( MyPath.PathPoints[i].V_point + (Vector3.up * 0.1f), MyPath.PathPoints[i + 1].V_point + (Vector3.up * 0.1f), nmStats.Size_DottedLine );
            }

            Handles.color = nmStats.Color_PathPt;
        }
    }
}
