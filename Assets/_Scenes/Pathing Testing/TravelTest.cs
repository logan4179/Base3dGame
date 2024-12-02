using LogansNavPath;
using PV_DebugUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace PV_ForTesting
{
	/// <summary>
	/// Class for testing travel paths/trajectories for both my custom LNP_Path class as well as navmesh agent pathing.
	/// The way I've been using this is to place it on a sphere that's placed somewhere in the world where I want to 
	/// test movement to and setting up the serializedfields. Set up multiple spheres with these on them as components 
	/// to test multiple different path scenarios.
	/// </summary>
    [System.Serializable]
    public class TravelTest : MonoBehaviour
    {
        public LNP_Path MyPath;

		public NavMeshPath NmPath;
		public List<Vector3> SampledPoints = new List<Vector3>();

        [Header("REFERENCE (EXTERNAL)")]
        public LNP_TestAgent MyAgent;
		public Stats_DebugLiving MyStats;

		[Header("DEBUG")]
		[SerializeField] private string DBG_CreatePath;

		public void Init()
        {
            InitPathObject();
        }

		[ContextMenu("z call InitPathObject()")]
		public void InitPathObject()
		{
			Debug.Log($"{nameof(TravelTest)}.{nameof(InitPathObject)}()");
			MyPath = new LNP_Path(LayerMask.GetMask("lr_EnvSolid"));
		}

		[ContextMenu("z call CreatePaths()")]
		public void CreatePaths()
		{
			DBG_CreatePath = $"{nameof(TravelTest)}.{nameof(CreatePaths)}() to '{transform.position}'\n";
			Debug.Log( DBG_CreatePath );

			InitPathObject();
			MyPath.CalculatePath( MyAgent.transform.position, transform.position );

			CreateNavMeshPath();
		}

		[ContextMenu("z call TravelToMeViaNavmeshAgent()")]
		public void TravelToMeViaNavmeshAgent()
		{
			DBG_CreatePath = $"{nameof(TravelToMeViaNavmeshAgent)}()\n";
			CreatePaths();

			MyAgent.TravelMode = 0;
			//MyAgent.MyNavMeshAgent.SetPath( NmPath );
			MyAgent.MyNavMeshAgent.SetDestination( transform.position );	
		}

		public void CreateNavMeshPath()
		{
			DBG_CreatePath += "{nameof(CreateNavMeshPath)}() to '{Trans_Follow.position}'...";

			SampledPoints = new List<Vector3>();

			NavMeshHit hit = new NavMeshHit();
			Vector3 startPos = MyAgent.transform.position;
			Vector3 endPos = transform.position;

			if ( NavMesh.SamplePosition(MyAgent.transform.position, out hit, 2f, NavMesh.AllAreas) )
			{
				startPos = hit.position;
			}
			else
			{
				Debug.LogError($"NavMesh.SamplePosition() did NOT hit startpos.\n");
				return;
			}

			if ( NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas) )
			{
				endPos = hit.position;
			}
			else
			{
				Debug.LogError($"NavMesh.SamplePosition() did NOT hit endpos.\n");
				return;
			}

			NmPath = new NavMeshPath();
			if ( !NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, NmPath) )
			{
				Debug.LogWarning($"Navmesh.CalculatePath() returned false. startPos: '{startPos}', endPos: '{endPos}'...");
				return;
			}

			for ( int i = 0; i < NmPath.corners.Length; i++ )
			{
				LNP_PathPoint pt = new LNP_PathPoint(NmPath.corners[i], LayerMask.GetMask("lr_EnvSolid"));
				SampledPoints.Add(pt.V_point);
			}

			DBG_CreatePath += $"end of {nameof(CreateNavMeshPath)}(). Path status: '{NmPath.status}'";
		}

		public bool AmDrawingGizmos = true;
		public bool AmDrawingLnpPath = true;
		public bool AmDrawingNavmeshPath = true;

		private void OnDrawGizmos()
		{	
			if( !AmDrawingGizmos )
			{
				return;
			}

			if ( AmDrawingLnpPath && MyPath.AmValid )
			{
				PV_DebugUtilities.DrawNumberedPath( MyStats, MyPath );
			}

			if ( AmDrawingNavmeshPath && SampledPoints != null && SampledPoints.Count > 0 )
			{
				Gizmos.color = Color.red;
				Vector3 vUp = (Vector3.up * 0.5f) + (Vector3.forward * 0.2f);
				for ( int i = 0;i < SampledPoints.Count; i++ )
				{
					Gizmos.DrawLine( SampledPoints[i], SampledPoints[i] + vUp );
					Handles.Label( SampledPoints[i] + vUp, $"nm{i}" );

					if( i < SampledPoints.Count - 1 )
					{
						Handles.color = Color.red;
						Gizmos.DrawLine(SampledPoints[i], SampledPoints[i+1]);

						//Handles.DrawDottedLine( SampledPoints[i], SampledPoints[i + 1], MyStats.Size_DottedLine );
					}
				}
			}
		}
	}
}