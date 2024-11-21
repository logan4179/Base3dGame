using PV_Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace pv_archived
{
	[Serializable]
	public class PV_PathPoint
	{
		public Vector3 V_point;
		public Vector3 V_toNext;
		public Vector3 V_toPrev;
		public Vector3 V_normal;
		/// <summary>The distance to the next patrol point.</summary>
		public float Dist_toNext = -1f;

		[HideInInspector] public bool AmEndPoint = false;
		public bool flag_switchGravityOff = false;
		public bool flag_switchGravityOn = false;

		[TextArea(2, 10)] public string DebugClass;

		// STATS -----------------//////////////////////
		/// <summary>If the v_toNext.y is greater than this, it will enable turnongravity.</summary>
		private float slopeHeight_turnOffGravity = 0.35f;
		/// <summary>The distance within which we consider a patrolpoint to be a 'corner' when compared to the </summary>
		private float dist_cornerThreshold = 0.75f;
		/// <summary>Highest number the dot product can be in order to consider this point as needing a following corner.</summary>
		private float threshold_dotMustBeBelow;

		public bool Init(Vector3[] corners_passed, int index_passed)
		{
			V_point = corners_passed[index_passed];
			bool needsCorner = false;
			DebugClass = string.Empty;

			if (index_passed < corners_passed.Length - 1)
			{
				AmEndPoint = false;
				V_toNext = Vector3.Normalize(corners_passed[index_passed + 1] - V_point); //need to be changed if a corner is needed
				Dist_toNext = Vector3.Distance(V_point, corners_passed[index_passed + 1]); //need to be changed if a corner is needed
			}
			else
			{
				AmEndPoint = true;
			}


			if (index_passed > 0)
			{
				V_toPrev = Vector3.Normalize(corners_passed[index_passed - 1] - V_point);
			}

			#region DETERMINE GRAVITY REQ---------------------
			if (!AmEndPoint)
			{
				//old way...
				/*if ( Mathf.Abs(V_toNext.y) >= slopeHeight_turnOffGravity && Mathf.Abs(V_toPrev.y) < slopeHeight_turnOffGravity )
				{
						TurnOffGravity = true;
				}
				else if( Mathf.Abs(V_toPrev.y) >= slopeHeight_turnOffGravity && Mathf.Abs(V_toNext.y) < slopeHeight_turnOffGravity )
				{
					TurnOnGravity = true;
				}*/

				///trying this new way that turns these booleans on at every point that meet the criteria...
				if (Mathf.Abs(V_toNext.y) >= slopeHeight_turnOffGravity)
				{
					flag_switchGravityOff = true;
				}
				else if (Mathf.Abs(V_toPrev.y) >= slopeHeight_turnOffGravity && Mathf.Abs(V_toNext.y) < slopeHeight_turnOffGravity)
				{
					flag_switchGravityOn = true;
				}
			}
			#endregion

			#region GENERATE NORMALS-----------------------------------------------------------------
			RaycastHit rcHit;
			//Ray ray_normal = new Ray( V_point + (Vector3.up * 2f), Vector3.down );

			DebugClass += "Normal check---------------------------\n";
			DebugClass += $"thresh: '{threshold_dotMustBeBelow}'\n";

			Collider[] hitColliders = Physics.OverlapSphere(V_point, 0.2f, PV_Environment.Instance.Mask_EnvSolid, QueryTriggerInteraction.Ignore);
			if (hitColliders != null)
			{
				DebugClass += ($"hitcolliders not null. length: '{hitColliders.Length}'.\n");
				if (hitColliders.Length > 0)
				{
					DebugClass += $"closest point: '{hitColliders[0].ClosestPoint(V_point)}' on object: '{hitColliders[0].name}', '{Vector3.Distance(V_point, hitColliders[0].ClosestPoint(V_point))}' away.\n";
					DebugClass += ($"hitcolliders Length: '{hitColliders.Length}'. first collider hit: '{hitColliders[0].name}'. pos: '{hitColliders[0].transform.position}'. trying crosscast...\n");
					if (PV_Utilities.CrossCast(V_point, 0.15f, out rcHit, PV_Environment.Instance.Mask_EnvSolid, QueryTriggerInteraction.Ignore))
					{
						DebugClass += ($"crosscast succesful, object hit: '{rcHit.transform.name}' normal: '{rcHit.normal}'\n");
						V_normal = rcHit.normal;
					}
					else
					{
						DebugClass += ($"crosscast Not succesful.~~~~~~~~~~~XXXXXXXXX\n");

					}
				}
				else
				{
					DebugClass += ($"hitcolliders length was: '{hitColliders.Length}'~~~~~~~~~~~XXXXXXXXX\n");
				}
			}
			else
			{
				DebugClass += ($"hitColliders null~~~~~~~~~~~XXXXXXXXX\n");

			}

			#endregion

			DebugClass += "corner check---------------------------\n";

			#region CHECK IF CORNER IS NEEDED----------------------------------------------------
			if (!AmEndPoint && Dist_toNext <= dist_cornerThreshold)
			{
				float CornerDotProduct = Vector3.Dot(V_toNext, V_normal);
				DebugClass += $"Meets initial corner reqs. dot: '{CornerDotProduct}'{(CornerDotProduct < threshold_dotMustBeBelow ? "!!!" : "")}\n";
				if (CornerDotProduct < threshold_dotMustBeBelow && Physics.Linecast(V_point + (V_normal * 0.05f), corners_passed[index_passed + 1], PV_Environment.Instance.Mask_EnvSolid))
				{
					needsCorner = true;
					DebugClass += $"found needs corner.\n";
				}
				else
				{
					needsCorner = false;
					DebugClass += $"found doesn't need corner.\n";
				}
			}
			else
			{
				DebugClass += $"Doesn't meet corner requirments.\n";
			}

			#endregion
			return needsCorner;
		}

		public void InitForAir(List<Vector3> corners_passed, int index_passed)
		{
			V_point = corners_passed[index_passed];
			DebugClass = string.Empty;

			if (index_passed < corners_passed.Count - 1)
			{
				AmEndPoint = false;
				V_toNext = Vector3.Normalize(corners_passed[index_passed + 1] - V_point); //need to be changed if a corner is needed
				Dist_toNext = Vector3.Distance(V_point, corners_passed[index_passed + 1]); //need to be changed if a corner is needed
			}
			else
			{
				AmEndPoint = true;
			}


			if (index_passed > 0)
			{
				V_toPrev = Vector3.Normalize(corners_passed[index_passed - 1] - V_point);
			}
		}

		public void InitAsCorner(PV_PathPoint pt_start, PV_PathPoint pt_end)
		{
			V_normal = Vector3.Normalize((pt_start.V_normal + pt_end.V_normal) / 2f);
			Vector3 v_offset = Vector3.zero;
			//a...
			//v_offset = V_normal;
			//b...
			v_offset = V_normal * 0.4f;

			V_point = ((pt_start.V_point + pt_end.V_point) / 2f) + v_offset;

			V_toNext = Vector3.Normalize(pt_end.V_point - V_point); //need to be changed if a corner is needed
			Dist_toNext = Vector3.Distance(V_point, pt_end.V_point); //need to be changed if a corner is needed

			//correct pt_start...
			pt_start.Dist_toNext = Vector3.Distance(pt_start.V_point, V_point);
			pt_start.V_toNext = Vector3.Normalize(V_point - pt_start.V_point);

			DebugClass = $"V_toNext: '{V_toNext}'\n" +
				//$"V_toPrev: '{V_toPrev}'\n" +
				$"AmEndPoint: '{AmEndPoint}'\n";

		}
	}
}