using PV_Enums;
using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Base_Enemy_airborn : Base_enemy
{
	public override Vector3 PlayerFocusPosition
	{
		get { return playerTransform.position + (Vector3.up * playerScript.MyHeight * 0.9f); }
	}

	protected override void Awake()
    {
		base.Awake();

        myMovementMode = EntityMovementMode.Airborn;
    }

	protected override void LookToward(Vector3 pos_passed, Vector3 normal_passed, float rotSpeed_passed)
	{
		rb.MoveRotation(
			Quaternion.LookRotation(Vector3.RotateTowards(PV_Utilities.FlatVect(trans.forward),
			PV_Utilities.FlatVect(pos_passed - trans.position), rotSpeed_passed * Time.fixedDeltaTime, 0.0f))
			);
	}

    [ContextMenu("call CapturePatrolPoint()")]
    protected void CapturePatrolPoint()
    {
        if (trans_patrolPointGrabber == null)
        {
            Debug.LogError("Error! patrolpoint grabber null");
        }

        if (patrolPoints_cached == null)
        {
            patrolPoints_cached = new List<PV_PatrolPoint>();
        }

        PV_PatrolPoint pt = new PV_PatrolPoint();
        pt.V_center = trans_patrolPointGrabber.position;
        patrolPoints_cached.Add(pt);
        Debug.Log($"Captured patrol point at: '{pt.V_center}'");
    }

	public override void CalculatePlayerVisibility(float timeMult_passed) //public so that this can be called from the manager script a certain amount per second.
    {
		base.CalculatePlayerVisibility(timeMult_passed);

	}

	public override void UpdatePath( Vector3 v_passed, float sampleMaxDistance )
	{
		List<Vector3> pointsReturned = area_triggering.AirspaceManager.FindPath(trans.position, v_passed);
		Log($"airspace manager found '{pointsReturned.Count}' points. using these to calculate a PV_Path...");
		MyPath.CalculateAirPath(pointsReturned);

		calcSpatialValues(); //This will make sure it has the right values immediately...
	}
}
