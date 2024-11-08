using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// For grabbing patrol points in the scene. GameObject this script is attached to gets disabled on start
/// </summary>
public class PatrolPointGrabber : MonoBehaviour
{
	[SerializeField] private Base_enemy enemyReference;

	public PV_Environment cachedEnvironmentManagerReference;

	[ContextMenu("call CapturePatrolPoint()")]
	protected void CapturePatrolPoint()
	{
		NavMeshHit hit;
		if ( NavMesh.SamplePosition( transform.position, out hit, 0.5f, NavMesh.AllAreas) )
		{
			PV_PatrolPoint pt = new PV_PatrolPoint();
			pt.V_center = hit.position;

			RaycastHit rcHit;
			if ( PV_Utilities.CrossCast(pt.V_center, 0.5f, out rcHit, cachedEnvironmentManagerReference.Mask_EnvSolid, QueryTriggerInteraction.Ignore) )
			{
				print($"crosscast succesful, object hit: '{rcHit.transform.name}' normal: '{rcHit.normal}'\n");
				pt.V_normal = rcHit.normal;
			}
			else
			{
				print($"crosscast Not succesful.~~~~~~~~~~~XXXXXXXXX\n");

			}

			Debug.Log($"Captured patrol point at: '{hit.position}', hit.normal: '{hit.normal}'");
			enemyReference.RegisterPatrolPoint( pt );
		}
		else
		{
			Debug.LogWarning("Failed to capture patrolpoint");
		}
	}
}
