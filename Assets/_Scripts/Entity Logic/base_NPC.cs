using LogansAreaManagementSystem;
using LogansFootLogicSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class base_NPC : Base_living, I_LAMS_Entity
{
	protected FootSystem myFootSystem;
	[SerializeField] protected Transform Trans_visionOrigin;

	[Header("---------------[[ TRAVEL-TO (base_NPC) ]]-----------------")]
	[SerializeField] protected string debugTravel;
	public PV_Path MyPath = null;
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

	protected override void Awake()
	{
		base.Awake();

		myFootSystem = trans.Find("FootSphere").GetComponent<FootSystem>();

	}

	protected override void Start()
	{
		base.Start();

		myFootSystem.Init(trans, trans_perspective, rb, PV_Environment.Instance.Mask_WalkableJumpable);
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
			v_toGoal = Vector3.Normalize(MyPath.CurrentGoal - trans.position);
			dot_facingToNextPos = Vector3.Dot(trans.forward, v_toGoal);
			dist_toNextPos = Vector3.Distance(trans.position, MyPath.CurrentGoal);
			dist_toEndGoal = Vector3.Distance(trans.position, MyPath.EndGoal);
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
