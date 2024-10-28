using PV_Enums;
using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Base_Enemy_terrestrial : Base_enemy
{
	public override Vector3 PlayerFocusPosition
	{
		get { return playerTransform.position + (Vector3.up * playerScript.MyHeight * 0.5f); }
	}

	protected override void Awake()
    {
        base.Awake();

        myMovementMode = EntityMovementMode.TerrestrialMovement;
    }

	protected override void Start()
	{
		base.Start();

		NavMeshHit nmHit;
		if ( !NavMesh.SamplePosition(trans.position, out nmHit, 7f, NavMesh.AllAreas) )
		{
			PV_Debug.LogError($"Enemy: '{name}' couldn't sample (find) the terrain. Disabling script component. Do I need to change the sample distance or set this object closer to the navmesh?");
			enabled = false;
			return;
		}
	}

	protected void LookTowardNext( Vector3 pos_passed, Vector3 normal_passed, float rotSpeed_passed ) //todo: dwf
    {
        DBGLookToward = "";
        Quaternion q = Quaternion.identity;
        Vector3 vRot = Vector3.zero;
        v_nrml_calculated = Vector3.RotateTowards(v_nrml_calculated, normal_passed, rotSpeed_passed * Time.fixedDeltaTime, 0f);

        /*if ( rotChoice == 0 )
        {
            rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(PV_Utilities.FlatVect(trans.forward), PV_Utilities.FlatVect(position_passed - trans.position), rotSpeed_passed * Time.fixedDeltaTime, 0.0f)) );
        }
        else if( rotChoice == 1 )
        {
            v_normal = Vector3.RotateTowards(v_normal, normal_passed, rotSpeed_passed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(trans.forward, Vector3.Normalize(position_passed - trans.position), rotSpeed_passed * Time.fixedDeltaTime, 0.0f), v_normal) );
            Debug.DrawLine( trans.position, trans.position + (v_normal*2f) );
        }
        else if ( rotChoice == 2 ) //just like 1, but using the passed normal straight instead of gradually rotating that normal goal...
        {
            rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(trans.forward, Vector3.Normalize(position_passed - trans.position), rotSpeed_passed * Time.fixedDeltaTime, 0.0f), normal_passed) );
        }*/

        /*if( normal_passed == Vector3.up && trans.up.y >= 0.98f && dot_facingToNextPos < 0f ) 
        {
            rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(PV_Utilities.FlatVect(trans.forward), PV_Utilities.FlatVect(position_passed - trans.position), rotSpeed_passed * Time.fixedDeltaTime, 0.0f)) );
            //Also show here with a gizmo line how it would have rotated.
        }
        else
        {
            //v_normal = Vector3.RotateTowards(v_normal, normal_passed, rotSpeed_passed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(trans.forward, Vector3.Normalize(position_passed - trans.position), rotSpeed_passed * Time.fixedDeltaTime, 0.0f), v_normal) );
            //rb.MoveRotation( Quaternion.LookRotation(Vector3.RotateTowards(, , rotSpeed_passed * Time.fixedDeltaTime, 0.0f), trans.up) );

        }*/

        if ( dot_facingToNextPos < -0.98f && Vector3.Dot(normal_passed, trans.up) > 0.95f )
        {
            //vRot = Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
            vRot = Vector3.RotateTowards( trans.forward, trans.right, rotSpeed_passed * Time.fixedDeltaTime, 0.0f );
            q = Quaternion.LookRotation(vRot, trans.up);
        }
        else
        {
            //vRot = Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
            vRot = Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f);
            q = Quaternion.LookRotation(vRot, v_nrml_calculated);
        }
        q = Quaternion.LookRotation(vRot, v_nrml_calculated);



        Debug.DrawLine(trans.position, trans.position + (v_nrml_calculated * 5f), Color.green);
        Debug.DrawLine(trans.position, trans.position + (vRot * 5f), Color.yellow);

        rb.MoveRotation(q);

        DBGLookToward += $"v_normal: '{v_nrml_calculated}' trans.up: '{trans.up}' \n" +
            $"dot_facingToNextPos: '{dot_facingToNextPos}'\n" +
            $"goal: '{Quaternion.LookRotation(Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f), v_nrml_calculated)}'\n" +
            $"fwd: '{trans.forward}', v_toGoal: '{v_toGoal}'\n" +
            $"trans.up: '{trans.up}' dot up: '{Vector3.Dot(normal_passed, trans.up)}'";
    }

	[ContextMenu("call CapturePatrolPoint()")]
    protected void CapturePatrolPoint()
    {
        if ( trans_patrolPointGrabber == null )
        {
            Debug.LogError("Error! patrolpoint grabber null");
        }

        if ( patrolPoints_cached == null )
        {
            patrolPoints_cached = new List<PV_PatrolPoint>();
        }

        NavMeshHit hit;
        if ( NavMesh.SamplePosition(trans_patrolPointGrabber.position, out hit, 0.5f, NavMesh.AllAreas) )
        {
            PV_PatrolPoint pt = new PV_PatrolPoint();
            pt.V_center = hit.position;

            RaycastHit rcHit;
            if ( PV_Utilities.CrossCast(pt.V_center, 0.5f, out rcHit, PV_Environment.Instance.Mask_EnvSolid, QueryTriggerInteraction.Ignore))
            {
                print($"crosscast succesful, object hit: '{rcHit.transform.name}' normal: '{rcHit.normal}'\n");
                pt.V_normal = rcHit.normal;
            }
            else
            {
                print($"crosscast Not succesful.~~~~~~~~~~~XXXXXXXXX\n");

            }

            patrolPoints_cached.Add(pt);
            Debug.Log($"Captured patrol point at: '{hit.position}', hit.normal: '{hit.normal}'");
        }
        else
        {
            Debug.LogWarning("Failed to capture patrolpoint");
        }
    }

	public override void RegisterPatrolPoint( PV_PatrolPoint pt )
	{
		base.RegisterPatrolPoint(pt);

		if ( patrolPoints_cached == null )
		{
			patrolPoints_cached = new List<PV_PatrolPoint>();
		}

		patrolPoints_cached.Add(pt);

	}

	public override void CalculatePlayerVisibility( float timeMult_passed ) //public so that this can be called from the manager script a certain amount per second.
	{
		base.CalculatePlayerVisibility(timeMult_passed);
		
	}

	public override void UpdatePath(Vector3 v_passed, float sampleMaxDistance)
	{
		MyPath.CalculatePath(trans.position, v_passed, sampleMaxDistance);

		calcSpatialValues(); //This will make sure it has the right values immediately...
	}
}
