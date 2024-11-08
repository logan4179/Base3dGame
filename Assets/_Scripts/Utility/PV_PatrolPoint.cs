using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class PV_PatrolPoint
{
    public Vector3 V_center;

    /// <summary>
    /// Radius around the position vector that should be sampled for a random navmesh intersection. <= 0 Means 
    /// that the enemy is only supposed to use the exact position instead of sampling around.
    /// </summary>
    public float Radius;

    public Vector3 V_normal;

    [Header("---------------[[ DEBUG ]]-----------------")]
    [TextArea(1,5)] public string HistoryDebugString = "";

    public PV_PatrolPoint()
    {
        //Debug.Log("patrol point ctor");
    }

    /// <summary>
    /// Returns a randomized position within this point's radius from it's position vector.
    /// </summary>
    /// <param name="fromPos"></param>
    /// <param name="maxSingleMoveDist"></param>
    /// <param name="minSingleMoveDist"></param>
    /// <returns></returns>
    public Vector3 GetVectorWithin( Vector3 fromPos, bool sampleNavMesh = true )
    {
        HistoryDebugString += $"GetVectorWithin(currentPos: '{fromPos}')\n";
        if( Radius <= 0f )
        {
            HistoryDebugString += "Radius too small, returning vposition...\n";
            return V_center;
        }
        else
        {
            Vector3 vRslt = PV_Environment.FetchRandomVectorWithin( V_center, fromPos, Radius );
			HistoryDebugString += $"Radius big enough, returned pt: '{vRslt}'\n";
            return vRslt;
        }
    }
}
