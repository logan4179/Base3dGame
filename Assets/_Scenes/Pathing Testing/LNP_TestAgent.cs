using LogansFootLogicSystem;
using LogansNavPath;
using PV_DebugUtils;
using PV_Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

namespace PV_ForTesting
{
	public class LNP_TestAgent : MonoBehaviour
	{
		public TravelTest FocusedTest;

		[Header("REFERENCE")]
		[SerializeField] private Rigidbody rb;
		[SerializeField] private FootSystem myFootSystem;
		private SphereCollider footSystemCollider;
		public NavMeshAgent MyNavMeshAgent;

		[Header("STATS")]
		public Stats_base_NPC MyStats_Base_NPC;
		public Stats_DebugLiving MyDebugStats;
		public bool AutoFollow = false;

		[Header("OTHER")]
		public int Index_CurrentTest = 0;

		private Vector3 v_nrml_calculated;
		protected Vector3 v_toGoal;
		float dot_facingToNextPos;
		float dist_toNextPos;
		float dist_toEndGoal;
		public int TravelMode = 0;

		[Header("DEBUG")]
		public string DBG_Travel;
		public bool DebugFootSphere = false;

		void Awake()
		{
			//base.Awake();
			footSystemCollider = myFootSystem.GetComponent<SphereCollider>();
		}

		void Start()
		{
			myFootSystem.Mask_Walkable = LayerMask.GetMask("lr_EnvSolid");
		}

		void FixedUpdate()
		{
			if ( FocusedTest == null )
			{
				return;
			}

			DBG_Travel = string.Empty;
			CalcSpatialValues_pathing();

			if (TravelMode == 1) //use custom movement with LNP_Path...
			{

				try
				{
					DBG_Travel = FocusedTest.MyPath.CurrentGoal.ToString(); //todo: indexoutofrangeexception here
				}
				catch (System.Exception e)
				{
					Debug.LogError($"{e.GetType()} exception caught while getting current goal. Exception says: '{e.ToString()}'");
					return;
				}

				DBG_Travel += $"distToNextPos: '{dist_toNextPos}', calculatedDistanceThresh: '{MyStats_Base_NPC.dist_RoughlyThere}'\n" +
					$"dot_toNextPos: '{dot_facingToNextPos}', calculatedAngleThresh: '{MyStats_Base_NPC.Percentage_consideredRoughlyFacing_patrolPt}'\n" +
					$"amonendgoal: '{FocusedTest.MyPath.AmOnEndGoal}'\n" +
					$"NextPosition: '{FocusedTest.MyPath.CurrentGoal}' \n v_toGoal: '{v_toGoal}'\n";

				myFootSystem.TravelToward(
					FocusedTest.MyPath.CurrentGoal,
					FocusedTest.MyPath.CurrentPathPoint.V_normal,
					MyStats_Base_NPC.dist_RoughlyThere,
					MyStats_Base_NPC.Speed_move_patrol * Time.fixedDeltaTime,
					MyStats_Base_NPC.Percentage_consideredRoughlyFacing_patrolPt,
					MyStats_Base_NPC.Speed_Rotate_patrol * Time.fixedDeltaTime
				);

				#region INCREMENT INDEX ---------------------------------------------
				try
				{
					if ( dist_toNextPos < MyStats_Base_NPC.dist_RoughlyThere && !FocusedTest.MyPath.AmOnEndGoal )
					{
						// Turn on/off gravity before incrementing index
						if ( FocusedTest.MyPath.CurrentPathPoint.flag_switchGravityOn )
						{
							//Debug.LogWarning($"turning on gravity for: '{name}'");
							rb.useGravity = true;
						}
						else if ( FocusedTest.MyPath.CurrentPathPoint.flag_switchGravityOff )
						{
							//Debug.LogWarning($"turning off gravity for: '{name}'");


							rb.useGravity = false;
						}


						FocusedTest.MyPath.Index_currentPoint++;

					}
				}
				catch (System.Exception)
				{
					PV_GameManager.Instance.PauseGameToOptions();
					throw;
				}
				#endregion
			}
			else if (TravelMode == 2)
			{

			}
		}

		protected virtual void LookToward(Vector3 pos_passed, Vector3 normal_passed, float rotSpeed_passed)
		{
			Vector3 vToPos = Vector3.Normalize(pos_passed - transform.position);
			v_nrml_calculated = Vector3.RotateTowards(
				transform.up, normal_passed, rotSpeed_passed * Time.fixedDeltaTime, 0f
			);
			float dot_facingToPos = Vector3.Dot(transform.forward, vToPos);
			float dot_upToNrml = Vector3.Dot(transform.up, normal_passed);

			Quaternion q = Quaternion.identity;
			Vector3 vRot = Vector3.zero;
			if (dot_facingToPos < -0.98f && dot_upToNrml > 0.95f)
			{
				//vRot = Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
				vRot = Vector3.RotateTowards(transform.forward, transform.right, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
				q = Quaternion.LookRotation(vRot, transform.up);
			}
			else
			{
				//vRot = Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
				vRot = Vector3.RotateTowards(transform.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
				q = Quaternion.LookRotation(vRot, v_nrml_calculated);
			}

			rb.MoveRotation(q);
		}

		protected virtual void CalcSpatialValues_pathing()
		{
			if ( FocusedTest.MyPath.AmValid )
			{
				v_toGoal = Vector3.Normalize( FocusedTest.MyPath.CurrentGoal - transform.position );
				dot_facingToNextPos = Vector3.Dot( transform.forward, v_toGoal );
				dist_toNextPos = Vector3.Distance( transform.position, FocusedTest.MyPath.CurrentGoal );
				dist_toEndGoal = Vector3.Distance( transform.position, FocusedTest.MyPath.EndGoal );
			}
		}

		public void SetMyNavmeshDestination( Vector3 pos )
		{
			TravelMode = 0;
			MyNavMeshAgent.SetDestination( pos );
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				if (DebugFootSphere)
				{
					if (myFootSystem.MyFootState == LFLS_FootState.Grounded)
					{
						Gizmos.color = MyDebugStats.Color_footSphere_grounded;
					}
					else if (myFootSystem.MyFootState == LFLS_FootState.Sliding)
					{
						Gizmos.color = MyDebugStats.Color_footSphere_sliding;
					}
					else if (myFootSystem.MyFootState == LFLS_FootState.Airborn)
					{
						Gizmos.color = MyDebugStats.Color_footSphere_airborn;
					}

					Gizmos.DrawSphere(
						myFootSystem.transform.position, footSystemCollider.radius * myFootSystem.transform.localScale.x
					);
				}
			}
		}
	}
}