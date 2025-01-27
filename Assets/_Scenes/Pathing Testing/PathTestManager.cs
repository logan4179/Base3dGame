using LogansFootLogicSystem;
using LogansNavPath;
using PV_DebugUtils;
using PV_ForTesting;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class PathTestManager : MonoBehaviour
{
    public bool DrawAllTests = true;
    public int Index_DebugTestFocus = 0;

    [SerializeField] private TravelTest[] travelTests;

    void Start()
    {
        
    }

    [ContextMenu("z - GetAllTests()")]
	public void GetAllTests()
    {
        travelTests = transform.GetComponentsInChildren<TravelTest>();
    }

	[ContextMenu("z - RunAllTests()")]
	public void RunAllTests()
    {
        print($"{nameof(RunAllTests)}()...");

        foreach( TravelTest travelTest in travelTests )
        {
            //Debug.Log($"foreaching test: '{travelTest.name}'...");
			travelTest.CreatePaths();

            if( travelTest.ExpectedPath.PathPoints == null )
            {
                Debug.LogError( $"was null" );
            }


            if( travelTest.MyPath.PathPoints.Count == travelTest.ExpectedPath.PathPoints.Count )
            {
                for ( int i = 0; i < travelTest.MyPath.PathPoints.Count; i++ )
                {
                    LNP_PathPoint myPathPt = travelTest.MyPath.PathPoints[i];
                    LNP_PathPoint expectedPathPt = travelTest.ExpectedPath.PathPoints[i];

                    if (myPathPt.V_point != expectedPathPt.V_point || 
                        myPathPt.V_normal != expectedPathPt.V_normal || 
                        myPathPt.V_toPrev != expectedPathPt.V_toPrev || 
                        myPathPt.V_toNext != expectedPathPt.V_toNext || 
                        myPathPt.Dist_toPrev != expectedPathPt.Dist_toPrev ||
                        myPathPt.Dist_toNext != expectedPathPt.Dist_toNext)
                    {
                        Debug.LogError( $"travelTest ({travelTest.name}) has different path points than expected" );
                        Debug.DrawLine(travelTest.transform.position, travelTest.transform.position + (Vector3.up * 10f), Color.red, 10f );
                        break;
                    }
                }
            }
            else
            {
				Debug.LogError($"travelTest ({travelTest.name}) has differing amount of pathpoints");
				Debug.DrawLine(travelTest.transform.position, travelTest.transform.position + (Vector3.up * 10f), Color.red, 10f);
			}
            
		}
    }

	private void OnDrawGizmos()
	{
        if ( DrawAllTests )
        {
            foreach ( TravelTest test in travelTests )
            {
                test.DrawMyGizmos();
            }
        }
        else if( Index_DebugTestFocus > -1 )
        {
            travelTests[Index_DebugTestFocus].DrawMyGizmos();
        }
	}
}
