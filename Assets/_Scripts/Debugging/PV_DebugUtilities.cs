using LogansFootLogicSystem;
using PV_Enums;
using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PV_DebugUtils
{
	public static class PV_DebugUtilities
	{
		public static void DrawNumberedPath( Stats_DebugLiving stats_dbgLvg, Vector3[] path_passed)
		{
			if (path_passed == null || path_passed.Length <= 0)
			{
				return;
			}

			Handles.color = stats_dbgLvg.Color_PathPt;

			for (int i = 0; i < path_passed.Length; i++)
			{
				//Handles.DrawLine( path_passed[i], path_passed[i]+Vector3.up * 5f, MyStats.Size_PathLineWidth );
				Handles.DrawLine(path_passed[i], path_passed[i] + Vector3.up * stats_dbgLvg.Height_PathVerticalLine, stats_dbgLvg.Width_PathVerticalLine);
				Handles.Label(path_passed[i] + Vector3.up * (stats_dbgLvg.Height_PathVerticalLine * 1.1f), $"{i}\n  {path_passed[i]}");

				if (i < path_passed.Length - 1)
				{
					Vector3 vToNext = path_passed[i + 1] - path_passed[i];
					Handles.ConeHandleCap(0, path_passed[i], Quaternion.LookRotation(vToNext, Vector3.up), stats_dbgLvg.Size_PatrolArrow, EventType.Repaint);
					Handles.DrawDottedLine(path_passed[i] + (Vector3.up * 0.1f), path_passed[i + 1] + (Vector3.up * 0.1f), stats_dbgLvg.Size_DottedLine);
				}

				if (i > 0)
				{
					//Handles.ConeHandleCap( 0, path_passed[i] + (Vector3.up*stats_passed.VerticalOffset_PathCones), 
					//Quaternion.LookRotation(Vector3.down), stats_passed.Size_PathCones, EventType.Repaint);
				}
			}

		}

		public static void DrawNumberedPath( Stats_DebugLiving stats_dbgLvg, PV_Path path_passed )
		{
			if (!path_passed.AmValid)
			{
				return;
			}

			DrawNumberedPath( stats_dbgLvg, path_passed.GetPath() );
		}

		public static void DrawVisonIndicator(Transform trans, Stats_DebugLiving stats_dbgLvg, float dist, bool canSeeTarget, EnemyActionState enemyState, float dotThreshold)
		{
			Color col = stats_dbgLvg.Color_VisionIndicatorLines;
			if (canSeeTarget)
			{
				if (enemyState >= EnemyActionState.Chasing)
				{
					col = Color.red;
				}
				else if (enemyState >= EnemyActionState.Patrolling)
				{
					col = Color.yellow;
				}
			}

			Debug.DrawLine(trans.position, trans.position + (trans.forward * dist), col);
			Debug.DrawLine(
				trans.position, trans.position + (Vector3.Normalize(Quaternion.AngleAxis((1 - dotThreshold) * 360, trans.up) * trans.forward) * dist), col
				);
			Debug.DrawLine(
				trans.position, trans.position + (Vector3.Normalize(Quaternion.AngleAxis(-(1 - dotThreshold) * 360, trans.up) * trans.forward) * dist),
				col
			);
		}
	}

	public class Player_debugInfo
	{
		[HideInInspector] public PV_Player playerScript;
		private Transform trans;
		private CapsuleCollider capsuleCol;
		private MeshRenderer mr_capsuleCol;
		private SphereCollider footSphere;
		private MeshRenderer mr_footSphere;
        public Player_debugInfo( PV_Player plr )
        {
			playerScript = plr;
			trans = playerScript.GetComponent<Transform>();
			capsuleCol = trans.Find("Capsule").GetComponent<CapsuleCollider>();
			mr_capsuleCol = capsuleCol.GetComponent<MeshRenderer>();
			footSphere = trans.Find("FootSphere").GetComponent<SphereCollider>();
			mr_footSphere = footSphere.GetComponent<MeshRenderer>();
        }
        public void DrawDebugVisuals( Stats_DebugLiving stats_dbgLvg )
		{
			Gizmos.DrawSphere( footSphere.transform.position, footSphere.radius );
		}
	}

	[System.Serializable] //todo: do these need to be serialized?
	public abstract class Enemy_debugInfo
	{
		public Base_enemy BaseEnemyReference;
		[HideInInspector] public Transform Trans_patrolPointGrabber;
		[HideInInspector] public List<PV_PatrolPoint> PatrolPoints_cached;

		[HideInInspector] public List<MeshRenderer> debugRenderers;

		[HideInInspector] public SphereCollider _attackColliderSphereCollider;

		public virtual void DrawDebugVisuals( Stats_DebugLiving stats_dbgLvg )
		{
			if ( Application.isPlaying )
			{
				if ( BaseEnemyReference.MyPath != null && BaseEnemyReference.MyPath.PathPoints != null && BaseEnemyReference.MyPath.PathPoints.Count > 0 )
				{
					PV_DebugUtilities.DrawNumberedPath(stats_dbgLvg, BaseEnemyReference.MyPath);
				}
			}

			PV_DebugUtilities.DrawVisonIndicator(
				BaseEnemyReference.transform, 
				stats_dbgLvg, 
				BaseEnemyReference.MyBaseStats.Distance_vision, 
				BaseEnemyReference.CanSeePlayer, 
				BaseEnemyReference.MyActionState, 
				BaseEnemyReference.MyBaseStats.Threshold_VisionRadius
				);

			if ( Trans_patrolPointGrabber != null )
			{
				Gizmos.color = stats_dbgLvg.Color_patrolPointGrabber;
				Gizmos.DrawWireSphere( Trans_patrolPointGrabber.position, stats_dbgLvg.Size_patrolPointGrabber );
			}

			if ( BaseEnemyReference.PatrolPoints_cached != null && BaseEnemyReference.PatrolPoints_cached.Count > 0 )
			{
				Gizmos.color = stats_dbgLvg.Color_cachedPatrolPoints;

				for ( int i = 0; i < BaseEnemyReference.PatrolPoints_cached.Count; i++ )
				{
					Gizmos.DrawSphere(
						BaseEnemyReference.PatrolPoints_cached[i].V_center,
						Mathf.Max(BaseEnemyReference.PatrolPoints_cached[i].Radius, 0.1f)
						);
					Gizmos.DrawLine(
						BaseEnemyReference.PatrolPoints_cached[i].V_center,
						BaseEnemyReference.PatrolPoints_cached[i].V_center + (BaseEnemyReference.PatrolPoints_cached[i].V_normal * 1.5f)
						);
					Handles.Label( BaseEnemyReference.PatrolPoints_cached[i].V_center, $"Pt{i}" );
				}
			}

			if( _attackColliderSphereCollider != null )
			{
				Gizmos.color = BaseEnemyReference.MyAttackCollider.amArmed ? stats_dbgLvg.Color_attackColliders_armed : stats_dbgLvg.Color_attackColliders_unarmed;

				Gizmos.DrawSphere(
					_attackColliderSphereCollider.transform.position,
					_attackColliderSphereCollider.radius * 100f
					);
			}
		}
	}

	[System.Serializable]
	public class Enemy_bug_debugInfo : Enemy_debugInfo
	{
		public Enemy_Bug MyEnemyReference;

        public Enemy_bug_debugInfo( Enemy_Bug bug )
        {
			BaseEnemyReference = bug.GetComponent<Base_enemy>();

			MyEnemyReference = bug;
			Trans_patrolPointGrabber = bug.Trans_patrolPointGrabber;
			PatrolPoints_cached = bug.PatrolPoints_cached;

			debugRenderers = PV_Utilities.GetComponentsInOnlyChildren<MeshRenderer>( bug.gameObject );

			_attackColliderSphereCollider = BaseEnemyReference.MyAttackCollider.GetComponent<SphereCollider>();
		}

		public override void DrawDebugVisuals( Stats_DebugLiving stats_dbgLvg )
		{
			base.DrawDebugVisuals( stats_dbgLvg );


		}
    }

	[System.Serializable]
	public class Enemy_basicFlyer_debugInfo : Enemy_debugInfo
	{
		public Enemy_BasicFlyer MyEnemyReference;

		public Enemy_basicFlyer_debugInfo( Enemy_BasicFlyer flyr )
		{
			BaseEnemyReference = flyr.GetComponent<Base_enemy>();

			MyEnemyReference = flyr;
			Trans_patrolPointGrabber = flyr.Trans_patrolPointGrabber;
			PatrolPoints_cached = flyr.PatrolPoints_cached;

			debugRenderers = PV_Utilities.GetComponentsInOnlyChildren<MeshRenderer>( flyr.gameObject );

			_attackColliderSphereCollider = BaseEnemyReference.MyAttackCollider.GetComponent<SphereCollider>();
		}

		public override void DrawDebugVisuals(Stats_DebugLiving stats_dbgLvg)
		{
			base.DrawDebugVisuals( stats_dbgLvg );


		}
	}


}

