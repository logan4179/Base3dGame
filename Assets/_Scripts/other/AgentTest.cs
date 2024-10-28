using PV_DebugUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentTest : MonoBehaviour
{
    public Transform Trans_Destination;
    public NavMeshAgent NmAgent;
    NavMeshPath navMeshPath;

    void Start()
    {
        navMeshPath = new NavMeshPath();
    }

    void Update()
    {
        
    }

    [ContextMenu("call SetDestination()")]
    public void SetDestination()
    {
        print($"SetDestination()");
        if( Trans_Destination == null )
        {
            Debug.LogError($"error, destination transform not set!");
            return;
        }

        NavMeshPath navMeshPath = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, Trans_Destination.position, NavMesh.AllAreas, navMeshPath );
        NmAgent.SetPath( navMeshPath );

    }

    public string dbgPath;

    private void OnDrawGizmos()
    {
        if( !Application.isPlaying )
        {
            return;
        }

        if( navMeshPath == null || navMeshPath.corners == null || NmAgent.path.corners.Length == 0 )
        {
            dbgPath = $"dbgreturn. path null: '{navMeshPath == null}, corners null: '{NmAgent.path.corners == null}', len: '{NmAgent.path.corners.Length}'";
            return;
        }
        else
        {
            dbgPath = $"DOIN IT! path null: '{navMeshPath == null}, corners null: '{NmAgent.path.corners == null}', len: '{NmAgent.path.corners.Length}'";

        }

		//PV_DebugUtilities.DrawNumberedPath( NmAgent.path.corners );
    }
}
