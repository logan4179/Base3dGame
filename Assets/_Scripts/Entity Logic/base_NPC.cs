using LogansAreaManagementSystem;
using LogansFootLogicSystem;
using PV_Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LogansNavPath;
//using pv_archived;

public class base_NPC : Base_living, I_LAMS_Entity
{
	public Stats_base_NPC MyStats_Base_NPC;

	protected FootSystem myFootSystem;
	[SerializeField] protected Transform Trans_visionOrigin;

	[Header("---------------[[ TRAVEL-TO (base_NPC) ]]-----------------")]
	[SerializeField] protected string debugTravel;
	public LNP_Path MyPath;
	[SerializeField] protected List<PV_PatrolPoint> patrolPoints_cached;
	public List<PV_PatrolPoint> PatrolPoints_cached => patrolPoints_cached;
	protected int index_currentlySelectedPatrolPoint = -1;
	/// <summary>The distance from the enemy to his end goal. Doesn't include navmesh path corners in between, just a straight line distance from enemy to end goal.</summary>
	protected float dist_toEndGoal;
	[SerializeField, Tooltip("Reference to an empty object that can help log patrol points.")]
	protected Transform trans_patrolPointGrabber;
	public Transform Trans_patrolPointGrabber => trans_patrolPointGrabber;

	[Header("---------------[[ SPATIAL (base_NPC) ]]-----------------")]
	[TextArea(1, 10), SerializeField] protected string DBG_spatial;
	protected float dot_facingToNextPos;
	/// <summary>Directional vector describing the direction of the goal from my current position.</summary>
	protected Vector3 v_toGoal;
	protected float dist_toNextPos = 0f;

	/// <summary>
	/// Decides if this entity will perform looping logic. This will only report false when a state dictates 
	/// it to, such as paused state or when the entity is dead.
	/// </summary>
	protected bool amPerformingLoopLogic
	{
		get
		{
			return (PV_GameManager.Instance.MyGameState == GameState.Unpaused && myEntityState == EntityState.Alive);
		}
	}

	protected override void Awake()
	{
		base.Awake();

		myFootSystem = trans.Find("FootSphere").GetComponent<FootSystem>();

	}

	protected override void Start()
	{
		base.Start();

		MyPath = new LNP_Path(PV_Environment.Instance.Mask_EnvSolid); //todo: should this be moved into the derived classes so that they can use different masks? IE: the bug might want Mask_EnvSolid, while a typical walking enemy might want Mask_WalkableJumpable?
		myFootSystem.Mask_Walkable = PV_Environment.Instance.Mask_WalkableJumpable;
	}

	protected override void MoveAlarms()
	{
		base.MoveAlarms();

		if (cd_JumpRecoverBuffer > 0f && myFootSystem.MyFootState == LFLS_FootState.Grounded)
		{
			cd_JumpRecoverBuffer -= Time.deltaTime;

			if (cd_JumpRecoverBuffer < 0f)
			{
				cd_JumpRecoverBuffer = 0f;
			}
		}
	}

	public virtual void RegisterPatrolPoint( PV_PatrolPoint pt )
	{

	}

	protected virtual void CalcSpatialValues_pathing()
	{
		if (MyPath.AmValid)
		{
			v_toGoal = Vector3.Normalize( MyPath.CurrentGoal - trans.position );
			dot_facingToNextPos = Vector3.Dot( trans.forward, v_toGoal );
			dist_toNextPos = Vector3.Distance( trans.position, MyPath.CurrentGoal );
			dist_toEndGoal = Vector3.Distance( trans.position, MyPath.EndGoal );
		}
	}

	public virtual void ActivateMeViaArea()
	{

	}

	public virtual void DeactivateMeViaArea()
	{

	}

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

		if (myFootSystem == null)
		{
			PV_Debug.LogError($"{name}.{nameof(myFootSystem)} reference was null.");
			amKosher = false;
		}

		return amKosher;
	}
}
