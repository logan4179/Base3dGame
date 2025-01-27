
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LogansNavPath
{
    [System.Serializable]
    public class LNP_Path
    {
        
		/// <summary>If the v_toNext.y is greater than this, it will enable turnongravity.</summary>
		[SerializeField] private float slopeHeight_switchGravity;
		/// <summary>The distance within which we consider a patrolpoint to be a 'corner' when compared to the </summary>
		[SerializeField] private float dist_cornerThreshold;
		/// <summary>Highest number the dot product can be as a condition of considering if this point needs a following corner.</summary>
		[SerializeField] private float threshold_cornerDotCheck;

		[HideInInspector] public int Index_currentPoint;
		public List<LNP_PathPoint> PathPoints;

		[HideInInspector] public Vector3 EndGoal => PathPoints[PathPoints.Count-1].V_point;

        [SerializeField, HideInInspector] private int mask_solidEnvironment;

        /// <summary>Tells if this path object has valid data to be used for pathing.</summary>
        public bool AmValid
        {
            get
            {
                return PathPoints != null && PathPoints.Count > 0;
            }
        }

        public LNP_PathPoint CurrentPathPoint
        {
            get
            {
                return PathPoints[Index_currentPoint]; //this sometimes triggers out of range exception
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
                if( AmValid && Index_currentPoint >= PathPoints.Count - 1 )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [SerializeField, TextArea(1, 8)] private string dbgCalculatePath;

		[TextArea(1, 5)] public string dbgAmOnCourse;

		public LNP_Path( int mask_passed )
        {
            mask_solidEnvironment = mask_passed;

            Index_currentPoint = -1;
            PathPoints = new List<LNP_PathPoint>();
            dbgCalculatePath = string.Empty;
            dbgAmOnCourse = string.Empty;
			slopeHeight_switchGravity = 0.35f;
			dist_cornerThreshold = 0.75f;
            threshold_cornerDotCheck = 0f;
        }

		public LNP_Path( LNP_Path path_passed)
		{
			mask_solidEnvironment = path_passed.mask_solidEnvironment;

			Index_currentPoint = -1;
			PathPoints = path_passed.PathPoints;
			dbgCalculatePath = path_passed.dbgCalculatePath;
			dbgAmOnCourse = path_passed.dbgAmOnCourse;
			slopeHeight_switchGravity = path_passed.slopeHeight_switchGravity;
			dist_cornerThreshold = path_passed.dist_cornerThreshold;
			threshold_cornerDotCheck = path_passed.threshold_cornerDotCheck;
		}


		//TODO: this is called by enemyscript.UpdatePath(), which is called by enemyscript.GenerateNewPatroLPoint() (among other places),
		//which calls PV_Environment.FetchRandomVectorWithin... The navmesh methods that these methods rely on return booleans indicating success.
		//It would be ideal if these methods did something with that boolean for cases where these functions are unsuccessful...
		public bool CalculatePath( Vector3 startPos_passed, Vector3 endPos_passed, float sampleMaxDistance = 0.5f )
        {
            dbgCalculatePath = ($"CalculatePath(startPos_passed: '{startPos_passed}', endPos_passed: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}' )\n");
            dbgCalculatePath += $"mask: '{mask_solidEnvironment}'\n";

			#region CREATE INITIAL NAVMESH PATH--------------------------------------
			if ( mask_solidEnvironment == 0 )
            {
                dbgCalculatePath = "Can't calculate a path because no environment mask was provided.";
                Debug.LogError( dbgCalculatePath );
                return false;
            }
            
            NavMeshPath nmpath = new NavMeshPath();
            NavMeshHit nvMshHit = new NavMeshHit();
            if ( NavMesh.SamplePosition(startPos_passed, out nvMshHit, sampleMaxDistance, NavMesh.AllAreas) )
            {
                startPos_passed = nvMshHit.position;
                dbgCalculatePath += $"NavMesh.SamplePosition() hit startpos\n";
            }
            else
            {
                dbgCalculatePath += $"NavMesh.SamplePosition() did NOT hit startpos.\n";
                return false; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
			}

            if ( NavMesh.SamplePosition(endPos_passed, out nvMshHit, sampleMaxDistance, NavMesh.AllAreas) )
            {
                endPos_passed = nvMshHit.position;
                dbgCalculatePath += $"NavMesh.SamplePosition() hit endpos\n";

            }
            else
            {
                dbgCalculatePath += $"NavMesh.SamplePosition() did NOT hit endpos\n";
                return false; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
			}

            if ( !NavMesh.CalculatePath(startPos_passed, endPos_passed, NavMesh.AllAreas, nmpath) )
            {
                dbgCalculatePath += $"Navmesh.CalculatePath() returned false. startPos: '{startPos_passed}', endPos: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}'..\n";

                Debug.LogWarning($"Navmesh.CalculatePath() returned false. startPos: '{startPos_passed}', endPos: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}'.."); //todo: this is getting triggered occasionally...
                return false; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
			}

			//Debug.Log($"path status: '{nmpath.status}'");
			#endregion

			PathPoints = new List<LNP_PathPoint>();

			dbgCalculatePath += ($"Navmesh calculate path found '{nmpath.corners.Length}' points...\n");

			dbgCalculatePath += "GENERATE ALL POINTS -------------------------\n";
			for ( int i = 0; i < nmpath.corners.Length; i++ ) //GENERATE INITIAL POINTS -------------------------------------
			{
				dbgCalculatePath += $"iterating nmpath.corner['{i}'].\n";
				LNP_PathPoint pt = new LNP_PathPoint( nmpath.corners[i], mask_solidEnvironment );
                PathPoints.Add( pt );

				if ( i < nmpath.corners.Length - 1 ) 
				{
                    pt.SetMyNextPoint( nmpath.corners[i + 1] );

                    RaycastHit hitInfo = new RaycastHit();
                    Vector3 castStrt = pt.V_point + (pt.V_normal * 0.15f);
                    dbgCalculatePath += $"Linecasting to next starting at '{castStrt}'...\n";
                    pt.DebugClass += $"Linecasting to next starting at '{castStrt}'...\n";
					if (Physics.Linecast(castStrt, castStrt + Vector3.ProjectOnPlane(pt.V_toNext * pt.Dist_toNext, pt.V_normal), out hitInfo, mask_solidEnvironment)) //Check if a corner point needs to be generated...
					{
                        pt.DebugClass += $"Did get intersection casting to next...\n";
                        LNP_PathPoint intersectedPoint = new LNP_PathPoint (hitInfo.point, mask_solidEnvironment );
						intersectedPoint.DebugClass += $"intialized as intersect point after '{PathPoints.Count-1}'";

						LNP_PathPoint beforeHitPoint = new LNP_PathPoint( pt, intersectedPoint );
                        beforeHitPoint.DebugClass += $"intialized as corner point after '{PathPoints.Count-1}";

						PathPoints.Add( beforeHitPoint );
						PathPoints.Add( intersectedPoint );
                    }

				}
			}

            /*dbgCalculatePath += "SET POINT RELATIONSHIPS -------------------------\n";
			for ( int i = 0; i < PathPoints.Count; i++ ) // SET POINT RELATIONSHIPS -------------------------------------
			{
				dbgCalculatePath += $"Iterating PathPoints['{i}']...\n";
                LNP_PathPoint calculatedPt = PathPoints[i]; //Have to create this temp object and assign at the end because these points are structs...

                if( i == 0 )
                {
					calculatedPt.SetSpatialValues( slopeHeight_switchGravity, PathPoints[i + 1] );
                }
                else if( i == PathPoints.Count - 1 )
                {
					calculatedPt.SetSpatialValues( PathPoints[i - 1], slopeHeight_switchGravity );
                }
                else
                {
					calculatedPt.SetSpatialValues(PathPoints[i - 1], slopeHeight_switchGravity, PathPoints[i + 1]);

                    /*if( calculatedPt.Flag_amCorner ) //this correction block needs to be done in order for the previous point to have the correct nextPt vector...
                    {
                        LNP_PathPoint PreviousPoint = PathPoints[i - 1];
                        PreviousPoint.SetMyNextPoint( calculatedPt.V_point );
                        PathPoints[i - 1] = PreviousPoint;
                    }
                }

                PathPoints[i] = calculatedPt;

                dbgCalculatePath += $"PathPoints[{i}] vToNext: '{PathPoints[i].V_toNext}', v_toPrev: '{PathPoints[i].V_toPrev}'\n";
			}*/

			Index_currentPoint = 0;
            return true; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
        }

		/* //previous...
		public bool CalculatePath(Vector3 startPos_passed, Vector3 endPos_passed, float sampleMaxDistance = 0.5f)
		{
			dbgCalculatePath = ($"CalculatePath(startPos_passed: '{startPos_passed}', endPos_passed: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}' )\n");

			#region CREATE INITIAL NAVMESH PATH--------------------------------------
			if (mask_solidEnvironment == 0)
			{
				dbgCalculatePath = "Can't calculate a path because no environment mask was provided.";
				Debug.LogError(dbgCalculatePath);
				return false;
			}

			NavMeshPath nmpath = new NavMeshPath();
			NavMeshHit hit = new NavMeshHit();
			if (NavMesh.SamplePosition(startPos_passed, out hit, sampleMaxDistance, NavMesh.AllAreas))
			{
				startPos_passed = hit.position;
				dbgCalculatePath += $"NavMesh.SamplePosition() hit startpos\n";
			}
			else
			{
				dbgCalculatePath += $"NavMesh.SamplePosition() did NOT hit startpos.\n";
				return false; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
			}

			if (NavMesh.SamplePosition(endPos_passed, out hit, sampleMaxDistance, NavMesh.AllAreas))
			{
				endPos_passed = hit.position;
				dbgCalculatePath += $"NavMesh.SamplePosition() hit endpos\n";

			}
			else
			{
				dbgCalculatePath += $"NavMesh.SamplePosition() did NOT hit endpos\n";
				return false; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
			}

			if (!NavMesh.CalculatePath(startPos_passed, endPos_passed, NavMesh.AllAreas, nmpath))
			{
				dbgCalculatePath += $"Navmesh.CalculatePath() returned false. startPos: '{startPos_passed}', endPos: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}'..\n";

				Debug.LogWarning($"Navmesh.CalculatePath() returned false. startPos: '{startPos_passed}', endPos: '{endPos_passed}', sampleMaxDistance: '{sampleMaxDistance}'.."); //todo: this is getting triggered occasionally...
				return false; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
			}

			Debug.Log($"path status: '{nmpath.status}'");
			#endregion

			EndGoal = nmpath.corners[nmpath.corners.Length - 1]; //when the above check returns false, this was causing an error to log on this line back before I started returning on false check...
			PathPoints = new List<LNP_PathPoint>();

			dbgCalculatePath += ($"Navmesh calculate path found '{nmpath.corners.Length}' points...\n");

			dbgCalculatePath += "GENERATE ALL POINTS -------------------------\n";
			for (int i = 0; i < nmpath.corners.Length; i++) //GENERATE ALL POINTS -------------------------------------
			{
				dbgCalculatePath += $"iterating nmpath.corner['{i}']. PathPoints.count: '{PathPoints.Count}'...\n";
				LNP_PathPoint pt = new LNP_PathPoint(nmpath.corners[i], mask_solidEnvironment);
				//PathPoints.Add( pt );

				if (i < nmpath.corners.Length - 1 &&
					Physics.Linecast(pt.V_point + (pt.V_normal * 0.05f), nmpath.corners[i + 1], mask_solidEnvironment)
					) //Check if a corner point needs to be generated...
				{
					dbgCalculatePath += $"found need corner...\n";
					LNP_PathPoint afterPt = new LNP_PathPoint(nmpath.corners[i + 1], mask_solidEnvironment);
					LNP_PathPoint crnrPt = new LNP_PathPoint(pt, afterPt);


					//pt.SetSpatialValues(slopeHeight_switchGravity, crnrPt);
                    //crnrPt.SetSpatialValues(pt, slopeHeight_switchGravity, afterPt);
                    //afterPt.SetSpatialValues(crnrPt, slopeHeight_switchGravity);

					PathPoints.Add(pt);
					PathPoints.Add(crnrPt);
					PathPoints.Add(afterPt);

					i++;
				}
				else
				{
					PathPoints.Add(pt);
				}
			}

			dbgCalculatePath += "SET POINT RELATIONSHIPS -------------------------\n";
			for (int i = 0; i < PathPoints.Count; i++) // SET POINT RELATIONSHIPS -------------------------------------
			{
				dbgCalculatePath += $"Iterating PathPoints['{i}']...\n";
				LNP_PathPoint calculatedPt = PathPoints[i]; //Have to create this temp object and assign at the end because these points are structs...

				if (i == 0)
				{
					calculatedPt.SetSpatialValues(slopeHeight_switchGravity, PathPoints[i + 1]);
				}
				else if (i == PathPoints.Count - 1)
				{
					calculatedPt.SetSpatialValues(PathPoints[i - 1], slopeHeight_switchGravity);
				}
				else
				{
					calculatedPt.SetSpatialValues(PathPoints[i - 1], slopeHeight_switchGravity, PathPoints[i + 1]);

					//if( calculatedPt.Flag_amCorner ) //this correction block needs to be done in order for the previous point to have the correct nextPt vector...
                    //{
                        //LNP_PathPoint PreviousPoint = PathPoints[i - 1];
                        //PreviousPoint.SetMyNextPoint( calculatedPt.V_point );
                        //PathPoints[i - 1] = PreviousPoint;
                   // }
				}

				PathPoints[i] = calculatedPt;

				dbgCalculatePath += $"PathPoints[{i}] vToNext: '{PathPoints[i].V_toNext}', v_toPrev: '{PathPoints[i].V_toPrev}'\n";
			}

			Index_currentPoint = 0;
			return true; //todo: returning a boolean is newly added. Make sure this return boolean is being properly used...
		}*/

		public void CalculateAirPath(List<Vector3> positions_passed)
        {
            dbgCalculatePath = ($"CalculateAirPath(positions_passed.count: '{positions_passed.Count}' )\n");

            PathPoints = new List<LNP_PathPoint>();
            dbgCalculatePath += ($"Generating '{positions_passed.Count}' points...\n");

            for ( int i = 0; i < positions_passed.Count; i++ )
            {
                dbgCalculatePath += ($"<b>i: '{i}', point: '{positions_passed[i]}'---------------</b>\n");
                LNP_PathPoint pt = new LNP_PathPoint( positions_passed[i] );
                PathPoints.Add( pt );
            }

            Index_currentPoint = 1;
        }

        private static float dist_checkIfOffCourseBeyondPrev = 0.4f;
        private static float threshold_onCourseAlignment = 0.975f;
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

        public Vector3[] GetPathVectors()
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