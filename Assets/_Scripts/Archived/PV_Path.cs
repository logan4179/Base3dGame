using pv_archived;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace pv_archived
{
	[System.Serializable]
	public class PV_Path
	{
		public int Index_currentPoint = -1;
		public List<PV_PathPoint> PathPoints;

		public Vector3 EndGoal;

		/// <summary>Tells if this path object has valid data to be used for pathing.</summary>
		public bool AmValid
		{
			get
			{
				return PathPoints != null && PathPoints.Count > 0;
			}
		}

		public PV_PathPoint CurrentPathPoint
		{
			get
			{
				if (PathPoints == null || PathPoints.Count == 0)
				{
					return null;
				}
				else
				{
					return PathPoints[Index_currentPoint]; //this sometimes triggers out of range exception
				}
			}
		}

		public Vector3 CurrentGoal
		{
			get
			{
				return PathPoints[Index_currentPoint].V_point; //todo: indexoutofrangeexception here
			}
		}

		public bool AmOnEndGoal
		{
			get
			{
				/*if( PatrolPoints == null || PatrolPoints.Count <= 0 || Index_currentPoint < (PatrolPoints.Count -1) ) //old way. This wasn't necessarily bad, but I think the new way makes more sense...
				{
					return false;
				}
				else
				{
					return true;
				}
				 */
				/*
				if ( PatrolPoints == null || PatrolPoints.Count <= 0 || Index_currentPoint >= (PatrolPoints.Count - 1)) //old way. This wasn't necessarily bad, but I think the new way makes more sense...
				{
					return true;
				}
				else
				{
					return false;
				}
				*/
				if (CurrentPathPoint != null && CurrentPathPoint.AmEndPoint)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[SerializeField, TextArea(1, 5)] private string dbgHistory;


		//TODO: this is called by enemyscript.UpdatePath(), which is called by enemyscript.GenerateNewPatroLPoint() (among other places),
		//which calls PV_Environment.FetchRandomVectorWithin... The navmesh methods that these methods rely on return booleans indicating success.
		//It would be ideal if these methods did something with that boolean for cases where these functions are unsuccessful...
		public void CalculatePath(Vector3 startPos_passed, Vector3 endPos_passed, float sampleMaxDistance = 0.5f)
		{
			dbgHistory += ($"CalculatePath(startPos_passed: '{startPos_passed}', endPos_passed: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}' )\n");
			NavMeshPath nmpath = new NavMeshPath();
			NavMeshHit hit = new NavMeshHit();
			if (NavMesh.SamplePosition(startPos_passed, out hit, sampleMaxDistance, NavMesh.AllAreas))
			{
				startPos_passed = hit.position;
				dbgHistory += $"NavMesh.SamplePosition() hit startpos\n";
			}
			else
			{
				dbgHistory += $"NavMesh.SamplePosition() did NOT hit startpos.\n";
			}

			if (NavMesh.SamplePosition(endPos_passed, out hit, sampleMaxDistance, NavMesh.AllAreas))
			{
				endPos_passed = hit.position;
				dbgHistory += $"NavMesh.SamplePosition() hit endpos\n";

			}
			else
			{
				dbgHistory += $"NavMesh.SamplePosition() did NOT hit endpos\n";
			}

			if (!NavMesh.CalculatePath(startPos_passed, endPos_passed, NavMesh.AllAreas, nmpath))
			{
				dbgHistory += $"Navmesh.CalculatePath() returned false. startPos: '{startPos_passed}', endPos: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}'..\n";

				Debug.LogWarning($"Navmesh.CalculatePath() returned false. startPos: '{startPos_passed}', endPos: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}'.."); //todo: this is getting triggered occasionally...
			}

			EndGoal = nmpath.corners[nmpath.corners.Length - 1]; //when the above check returns false, this is causing an error to log...
			PathPoints = new List<PV_PathPoint>();
			dbgHistory += ($"Generating '{nmpath.corners.Length}' points...\n");

			for (int i = 0; i < nmpath.corners.Length; i++)
			{
				dbgHistory += ($"<b>i: '{i}', point: '{nmpath.corners[i]}'---------------</b>\n");
				PV_PathPoint pt = new PV_PathPoint();
				bool needsCorner = pt.Init(nmpath.corners, i);

				PathPoints.Add(pt);

				if (needsCorner)
				{
					PV_PathPoint crnrPt = new PV_PathPoint();
					PV_PathPoint endPt = new PV_PathPoint();
					endPt.Init(nmpath.corners, i + 1);

					crnrPt.InitAsCorner(pt, endPt);
					PathPoints.Add(crnrPt);
					PathPoints.Add(endPt);
					i++;

				}
			}

			Index_currentPoint = 1;
		}

		public void CalculateAirPath(List<Vector3> positions_passed)
		{
			dbgHistory += ($"CalculateAirPath(positions_passed.count: '{positions_passed.Count}' )\n");

			EndGoal = positions_passed[positions_passed.Count - 1]; //when the above check returns false, this is causing an error to log...
			PathPoints = new List<PV_PathPoint>();
			dbgHistory += ($"Generating '{positions_passed.Count}' points...\n");

			for (int i = 0; i < positions_passed.Count; i++)
			{
				dbgHistory += ($"<b>i: '{i}', point: '{positions_passed[i]}'---------------</b>\n");
				PV_PathPoint pt = new PV_PathPoint();
				pt.InitForAir(positions_passed, i);
				PathPoints.Add(pt);
			}

			Index_currentPoint = 1;
		}

		private static float dist_checkIfOffCourseBeyondPrev = 0.4f;
		private static float threshold_onCourseAlignment = 0.975f;
		[TextArea(1, 5)] public string dbgAmOnCourse;
		public bool AmOnCourse(Vector3 pos_passed)
		{
			if (Index_currentPoint == 0)
			{
				if (Vector3.Distance(pos_passed, PathPoints[Index_currentPoint].V_point) <= 0.25f)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				float distToPrev = Vector3.Distance(pos_passed, PathPoints[Index_currentPoint - 1].V_point);
				Vector3 v_prevToPos = Vector3.Normalize(pos_passed - PathPoints[Index_currentPoint - 1].V_point);
				float myDot = Vector3.Dot(v_prevToPos, PathPoints[Index_currentPoint - 1].V_toNext);
				dbgAmOnCourse = $"distToPrev: '{distToPrev}', DOT: '{myDot}', v_prevToPos: '{v_prevToPos}', V_toNext: '{CurrentPathPoint.V_toNext}'";
				return (distToPrev < dist_checkIfOffCourseBeyondPrev || myDot > threshold_onCourseAlignment);
			}
		}

		public Vector3[] GetPath()
		{
			Vector3[] myPath = new Vector3[0];
			if (AmValid)
			{
				myPath = new Vector3[PathPoints.Count];
				for (int i = 0; i < PathPoints.Count; i++)
				{
					myPath[i] = PathPoints[i].V_point;
				}
			}

			return myPath;
		}
	}
}