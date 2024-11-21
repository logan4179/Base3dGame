using LogansFootLogicSystem;
using LogansNavPath;
using PV_DebugUtils;
using PV_Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LNP_Tester : MonoBehaviour
{
	public LNP_Path MyPath;

    [Header("REFERENCE")]
    public Transform Trans_Follow;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private FootSystem myFootSystem;
	private SphereCollider footSystemCollider;

	[Header("STATS")]
	public Stats_base_NPC MyStats_Base_NPC;
    public Stats_DebugLiving MyDebugStats;
    public bool AutoFollow = false;

	[Header("OTHER")]
	private Vector3 v_nrml_calculated;
	protected Vector3 v_toGoal;
	float dot_facingToNextPos;
	float dist_toNextPos;
	float dist_toEndGoal;

	[Header("DEBUG")]
	public string travelToDbg;
	public bool DebugFootSphere = false;

	void Awake()
	{
		//base.Awake();
		footSystemCollider = myFootSystem.GetComponent<SphereCollider>();
	}

	void Start()
    {
        MyPath = new LNP_Path( LayerMask.GetMask("lr_EnvSolid") );
		CreatePath();
    }

    void Update()
    {
		TravelToGoal(0.1f);
    }

    [ContextMenu("z call InitPathObject()")]
    public void InitPathObject()
    {
        Debug.Log($"{nameof(InitPathObject)}()");
        MyPath = new LNP_Path( LayerMask.GetMask("lr_EnvSolid") );
    }

	[ContextMenu("z call CreatePath()")]
    public void CreatePath()
    {
        print($"CreatePath() to '{Trans_Follow.position}'");
        
		InitPathObject();
        MyPath.CalculatePath( transform.position, Trans_Follow.position );
    }

	protected void TravelToGoal( float endGoalDistThresh )
	{
		travelToDbg = string.Empty;
		CalcSpatialValues_pathing();

		try
		{
			//Vector3 v = Vector3.Normalize(MyPath.CurrentGoal - trans.position); //todo: indexoutofrangeexception here - this is older, have since created a cached vector to the goal calculated in the calculateSpatialValues() method, so don't need to do this calc anymore. Using following line for error catching...
			travelToDbg = MyPath.CurrentGoal.ToString(); //todo: indexoutofrangeexception here


		}
		catch (System.Exception e)
		{
			Debug.LogError($"{e.GetType()} exception caught while getting current goal. Exception says: '{e.ToString()}'");
			return;
		}

		travelToDbg += $"distToNextPos: '{dist_toNextPos}', calculatedDistanceThresh: '{MyStats_Base_NPC.dist_RoughlyThere}'\n" +
			$"dot_toNextPos: '{dot_facingToNextPos}', calculatedAngleThresh: '{MyStats_Base_NPC.Percentage_consideredRoughlyFacing_patrolPt}'\n" +
			//$"{nameof(index_CurrentNavigationPath)}: '{index_CurrentNavigationPath}'\n" +
			$"amonendgoal: '{MyPath.AmOnEndGoal}'\n" +
			$"NextPosition: '{MyPath.CurrentGoal}' \n v_toGoal: '{v_toGoal}'\n";


		TravelTowards(
			MyPath.CurrentGoal, MyStats_Base_NPC.dist_RoughlyThere, 
			MyStats_Base_NPC.Speed_move_patrol, 
			MyPath.CurrentPathPoint.V_normal, 
			MyStats_Base_NPC.Percentage_consideredRoughlyFacing_patrolPt, 
			MyStats_Base_NPC.Speed_Rotate_patrol
		);

		#region INCREMENT INDEX ---------------------------------------------
		try
		{
			if ( dist_toNextPos < MyStats_Base_NPC.dist_RoughlyThere && !MyPath.AmOnEndGoal )
			{
				// Turn on/off gravity before incrementing index
				if (MyPath.CurrentPathPoint.flag_switchGravityOn)
				{
					//Debug.LogWarning($"turning on gravity for: '{name}'");
					rb.useGravity = true;
				}
				else if (MyPath.CurrentPathPoint.flag_switchGravityOff)
				{
					//Debug.LogWarning($"turning off gravity for: '{name}'");


					rb.useGravity = false;
				}


				MyPath.Index_currentPoint++;

			}
		}
		catch (System.Exception)
		{
			PV_GameManager.Instance.PauseGameToOptions();
			throw;
		}
		#endregion
	}

	protected virtual void LookToward(Vector3 pos_passed, Vector3 normal_passed, float rotSpeed_passed)
	{
		Vector3 vToPos = Vector3.Normalize( pos_passed - transform.position );
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

		//rb.MoveRotation(q);
	}

	protected void TravelTowards(Vector3 pos, float distThresh, float mvSpd, Vector3 rotNrml, float rotAngThresh, float rtSpd)
	{
		LookToward(pos, rotNrml, rtSpd);

		CalcSpatialValues_pathing();

		if (dist_toNextPos >= distThresh && dot_facingToNextPos >= rotAngThresh)
		{
			travelToDbg += $"State: Moving...\n";
			rb.MovePosition(transform.position + (mvSpd * Time.fixedDeltaTime * v_toGoal));
			CalcSpatialValues_pathing(); //Need to recalculate after moving.
		}
	}

	protected virtual void CalcSpatialValues_pathing()
	{
		if (MyPath.AmValid)
		{
			v_toGoal = Vector3.Normalize(MyPath.CurrentGoal - transform.position);
			dot_facingToNextPos = Vector3.Dot(transform.forward, v_toGoal);
			dist_toNextPos = Vector3.Distance(transform.position, MyPath.CurrentGoal);
			dist_toEndGoal = Vector3.Distance(transform.position, MyPath.EndGoal);
		}
	}

	private void OnDrawGizmos()
	{
		//if ( Application.isPlaying )
		//{
			if ( MyPath.AmValid )
			{
				PV_DebugUtilities.DrawNumberedPath( MyDebugStats, MyPath );
			}
		//}

		if ( Application.isPlaying )
		{
			if ( DebugFootSphere )
			{
				Gizmos.color = MyDebugStats.Color_footSphere;
				Gizmos.DrawSphere(
					myFootSystem.transform.position, footSystemCollider.radius * myFootSystem.transform.localScale.x
				);
			}
		}
	}
}
