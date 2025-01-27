using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UIElements;

namespace LogansNavPath
{
    public static class LNPS_Utils
    {
		/// <summary>
		/// Casts multiple times in a cross formation around origin. If any of the casts finds a hit, it stops the operation immediately and returns true.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		/// <param name="layerMask"></param>
		/// <returns></returns>
		public static bool CrossCast(Vector3 origin, float radius, out RaycastHit hitInfo, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			if (Physics.Linecast(origin + (Vector3.up * radius), origin + (Vector3.down * radius), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			else if (Physics.Linecast(origin + (Vector3.down * radius), origin + (Vector3.up * radius), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}

			if (Physics.Linecast(origin + (Vector3.right * radius), origin + (Vector3.left * radius), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			else if (Physics.Linecast(origin + (Vector3.left * radius), origin + (Vector3.right * radius), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}

			if (Physics.Linecast(origin + (Vector3.forward * radius), origin + (Vector3.back * radius), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			else if (Physics.Linecast(origin + (Vector3.back * radius), origin + (Vector3.forward * radius), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			return false;
		}

		public static bool CrossCast( Vector3 origin, Vector3 end, float extendCastDist, out RaycastHit hitInfo, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
		{
			Vector3 vExtend = Vector3.Normalize( end - origin) * extendCastDist;
			Debug.Log($"vExtend: '{vExtend}'");

			if ( Physics.Linecast(origin - vExtend, end + vExtend, out hitInfo, layerMask, queryTriggerInteraction) )
			{
				Debug.Log("crosscast immediately made hit");
				return true;
			}
			else if ( Physics.Linecast(end + vExtend, origin - vExtend, out hitInfo, layerMask, queryTriggerInteraction) )
			{
				Debug.Log("crosscast immediately made reverse hit");

				return true;
			}

			Debug.Log($"crosscast wasn't immediately succesful for pt: '{origin}'");


			float dist = Vector3.Distance( origin, end );

			if (Physics.Linecast(origin + (Vector3.up * dist), origin + (Vector3.down * dist), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			else if (Physics.Linecast(origin + (Vector3.down * dist), origin + (Vector3.up * dist), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}

			if (Physics.Linecast(origin + (Vector3.right * dist), origin + (Vector3.left * dist), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			else if (Physics.Linecast(origin + (Vector3.left * dist), origin + (Vector3.right * dist), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}

			if (Physics.Linecast(origin + (Vector3.forward * dist), origin + (Vector3.back * dist), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}
			else if (Physics.Linecast(origin + (Vector3.back * dist), origin + (Vector3.forward * dist), out hitInfo, layerMask, queryTriggerInteraction))
			{
				return true;
			}

			Debug.LogWarning($"crosscast ultimately failed for pt: '{origin}'");
			return false;
		}
	
		public static Vector3 CreateCornerPathPoint( LNP_PathPoint startPt, LNP_PathPoint endPt )
		{
			Vector3 resultPt = Vector3.zero;

			/*
			Vector3 v_starPtTToEndPt = (endPt.V_point - startPt.V_point);
			Vector3 v_endPtToStartPt = -v_starPtTToEndPt;

			Vector3 v_startPtToEndPt_onPtNormal = Vector3.ProjectOnPlane(v_starPtTToEndPt, startPt.V_normal);
			Vector3 v_endPtToStartPt_onEndPtNormal = Vector3.ProjectOnPlane(v_endPtToStartPt, endPt.V_normal);
			Vector3 v_startPtToEndPt_onEndPtNormal = Vector3.ProjectOnPlane(v_starPtTToEndPt, endPt.V_normal);

			//Vector3 v_ptToIntersectedPt_onPtNormal_cross = Vector3.Cross(pt.V_normal, v_ptToIntersectedPt_onPtNormal); //for if I want the cross vector...
			Vector3 v_try = (startPt.V_point + v_startPtToEndPt_onPtNormal) - endPt.V_point;

			Vector3 vectorParam = v_starPtTToEndPt;
			Vector3 normalParam = v_startPtToEndPt_onPtNormal;

			Vector3 v_projected = Vector3.ProjectOnPlane(vectorParam, normalParam);
			Debug.DrawLine(startPt.V_point, startPt.V_point + vectorParam, Color.red, 4f);
			Debug.DrawLine(startPt.V_point, startPt.V_point + normalParam, Color.green, 4f);
			Debug.DrawLine(endPt.V_point, endPt.V_point + v_try, Color.blue, 4f);

			float angleA = 90f - Vector3.Angle( startPt.V_normal, v_starPtTToEndPt );
			float angleB = 90f - Vector3.Angle( endPt.V_normal, v_endPtToStartPt );
			float unknownAngle = 180f - angleA - angleB;
			*/


			//https://math.libretexts.org/Bookshelves/Algebra/Algebra_and_Trigonometry_1e_(OpenStax)/10%3A_Further_Applications_of_Trigonometry/10.01%3A_Non-right_Triangles_-_Law_of_Sines
			
			Vector3 v_starPtTToEndPt = (endPt.V_point - startPt.V_point);
			Vector3 v_endPtToStartPt = -v_starPtTToEndPt;
			float dist_hypotenuse = v_endPtToStartPt.magnitude;
			float angleA = 90f - Vector3.Angle(startPt.V_normal, v_starPtTToEndPt.normalized);
			float angleB = 90f - Vector3.Angle(endPt.V_normal, v_endPtToStartPt.normalized);
			float angle_opposingHypotenuse = 180f - angleA - angleB;

			//note: need to convert to radians in the following, as opposed to degrees...
			float distA = Mathf.Sin(Mathf.Deg2Rad * angleB) * (dist_hypotenuse/Mathf.Sin(Mathf.Deg2Rad * angle_opposingHypotenuse)); //This is a re-ordered algebraic equation based on trigonometry

			resultPt = startPt.V_point + Vector3.ProjectOnPlane(v_starPtTToEndPt, startPt.V_normal).normalized * distA;



			/*
			Debug.Log($"CreateCornerPathPoint()----------\n" +
				$"{nameof(dist_hypotenuse)}: '{dist_hypotenuse}', {nameof(angleA)}: '{angleA}', {nameof(angleB)}, '{angleB}'\n" +
				$"{nameof(angle_opposingHypotenuse)}: '{angle_opposingHypotenuse}', {nameof(distA)}: '{distA}\n" +
				$"" +
				$"{nameof(resultPt)}: '{resultPt}'");



			Debug.Log($"math report...\n" + $"angleB: '{angleB}', sin(angleB): '{Mathf.Sin(angleB)}'\n" + 
				$"angle_opposingHypotenuse: '{angle_opposingHypotenuse}', sin(angle_opposingHypotenuse): '{angle_opposingHypotenuse}'");
			*/

			return resultPt;
		}
	}
}