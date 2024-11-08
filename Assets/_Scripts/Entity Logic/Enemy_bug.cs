using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PV_Enums;
using UnityEngine.AI;
using PV_Utils;
using LogansAreaManagementSystem;


public class Enemy_Bug : Base_Enemy_terrestrial
{
	//[Header("---------------[[ REFERENCE ]]-----------------")]
	public Stats_Enemy_bug MyStats => MGR_BugEnemy.Instance.Stats_bug;


	[Header("---------------[[ STATE ]]-----------------")]
	protected Actions_bugEnemy myCurrentAction;
	public Actions_bugEnemy MyCurrentAction => myCurrentAction;


	[Header("---------------[[ TRUTH ]]-----------------")]
	/// <summary>Allows selectively turning certain features for this object off in the case of a potentially game-crashing error.</summary>
	protected bool amBugged = false;

	//[Header("---------------[[ ANIMATION ]]-----------------")]
	public static int animID_Walking;
	public static int animID_Running;
	public static int animID_Attack;
	public static int animID_AttackChoice;
	public static int animID_TakingDamage;
	public static int animID_Dying;
	public static int animID_DamageClip;

	[Header("---------------[[ DEBUG ]]-----------------")]
	public bool AllowEditorDebugging = false;


	protected override void Awake()
	{
		Log_MethodStart( $"Awake() entered", PV_LogDestination.Hidden, PV_LogFormatting.UnityAPIMethod );
		base.Awake();

		MGR_BugEnemy.Instance.RegisterBug(this);
	}

	protected override void Start()
	{
		Log_MethodStart( $"Start() entered", PV_LogDestination.Hidden, PV_LogFormatting.UnityAPIMethod );
		base.Start();

		InitState();
	}

	void FixedUpdate()
	{
		if (!amPerformingLoopLogic)
		{
			return;
		}


		MoveAlarms();
		calcSpatialValues();


		#region DECIDE ACTION STATE---------------------------------------------------
		if (myActionState == EnemyActionState.Patrolling)
		{
			/*
			if ( cdAlert == 0 && cdSuspicionBuild > 0 )
			{
				myState = EnemyStates.Suspicious;
				DebugCanonString += $"state changed to: '{myState}'";
			}
			else if ( cdAlert > 0 )
			{
				myState = EnemyStates.Chasing;
				DebugCanonString += $"state changed to: '{myState}'";
			}
			*/
		}
		else if (myActionState == EnemyActionState.CaughtAGlimpse)
		{
			if (cd_PauseAfterGlimpse <= 0)
			{
				SwitchState(EnemyActionState.Suspicious);
			}
		}
		else if (myActionState == EnemyActionState.HeardSuspiciousNoise)
		{
			if (cd_PauseAfterGlimpse <= 0)
			{
				SwitchState(EnemyActionState.Suspicious);
				cd_InvestigateLocation = Random.Range(5f, 8f);


			}
		}
		else if (myActionState == EnemyActionState.Suspicious)
		{
			if (cd_Alert > 0)
			{
				SwitchState(EnemyActionState.Chasing);
			}
			else if (cd_InvestigateLocation == 0)
			{
				SwitchState(EnemyActionState.Patrolling);
			}
		}
		else if (myActionState == EnemyActionState.Chasing)
		{
			runningTime_spentChasingPlayer += Time.fixedDeltaTime;


			if (cd_Alert == 0)
			{
				SwitchState(EnemyActionState.Patrolling);
			}


			if (dist_toPlayer < MyStats_Base_Enemy.Dist_TriggerEngaging)
			{
				SwitchState(EnemyActionState.Engaging);
			}
		}
		else if (myActionState == EnemyActionState.Engaging)
		{
			//if (!plrVisible || distToPlayer > dist_consideredEngaging)
			if (dist_toPlayer > MyStats_Base_Enemy.Dist_TriggerChasing_fromEngaging)
			{
				SwitchState(EnemyActionState.Chasing);
			}
		}
		else if (myActionState == EnemyActionState.ApproachingToStrike)
		{
			if (cd_ApproachingPlayerBeforeStriking <= 0)
			{
				SwitchState(EnemyActionState.Engaging);
			}
		}


		#endregion


		if (myPatrolMode != PatrolMode.Stationary && myActionState == EnemyActionState.Patrolling)
		{
			if (cd_PatrolWait <= 0f)
			{
				TravelToGoal(MyStats_Base_Enemy.dist_RoughlyThere);


				if (dist_toEndGoal <= MyStats_Base_Enemy.dist_RoughlyThere)
				{
					Log($"Bug enemy has reached end goal, and is negotiating a patrol wait state...");
					GenerateNewPath();
					Log($"Bug enemy: '{name}', settled on a patrol wait of '{cd_PatrolWait}', and an end goal of: '{MyPath.EndGoal}'...");


				}
			}
		}
		else if (myActionState == EnemyActionState.CaughtAGlimpse)
		{
			//LookTowardNext( MyPath.EndGoal, MyPath.CurrentPathPoint.V_normal, MyStats_Base_Enemy.Speed_Rotate_fast );
			LookToward(MyPath.EndGoal, MyPath.CurrentPathPoint.V_normal, MyStats_Base_Enemy.Speed_Rotate_fast);


		}
		else if (myActionState == EnemyActionState.Suspicious)
		{
			TravelToGoal(MyStats_Base_Enemy.dist_RoughlyThere);
		}
		else if (myActionState == EnemyActionState.Chasing)
		{
			TravelToGoal(MyStats_Base_Enemy.dist_RoughlyThere);
		}
		else if (myActionState == EnemyActionState.Engaging)
		{
			TravelToGoal(MyStats_Base_Enemy.Dist_TriggerChasing_fromEngaging);
		}
		else if (myActionState == EnemyActionState.ApproachingToStrike)
		{
			TravelToGoal(loadedAttack.Reach);


			if (myCurrentAction == Actions_bugEnemy.BasicStrike)
			{
				if (dist_toPlayer < MyStats.Attack_basicStrike.Reach && dot_facingToPlayer > MyStats_Base_Enemy.Percentage_consideredRoughlyFacing_threat)
				{
					Attack();
				}
			}
		}
		else if (myActionState == EnemyActionState.Attacking)
		{
			if (myCurrentAction == Actions_bugEnemy.ChargeAttack)
			{
				if (myAttackState == AttackState.MidStrike)
				{
					//rb.MovePosition(trans.position + (trans.forward * MyStats.Speed_charging * Time.fixedDeltaTime)); //todo
					rb.AddForce(trans.forward * 3f, ForceMode.Force);
				}
				else if (myAttackState == AttackState.Recovering)
				{
					rb.AddForce(-(rb.velocity.normalized * 300.5f), ForceMode.Force);
				}
			}
		}
	}


#if UNITY_EDITOR
	private void LateUpdate()
	{
		DBG_State = "" +
		//$"{buggedColorString}{nameof(amBugged)}: '{amBugged}'</color>\n" +
		$"{nameof(MyType)}: '{MyType}'\n" +
		$"{nameof(myEntityState)}: '{myEntityState}'\n" +
		$"{nameof(myActionState)}: '{myActionState}'\n" +
		$"{nameof(myMovementState)}: '{myMovementState}'\n" +
		$"{nameof(myCurrentAction)}: '{myCurrentAction}'\n" +
		$"foot state: '{myFootSystem.MyFootState}'\n" +
		$"{nameof(hp)}: '{hp}'\n" +
		$"{nameof(loadedAttack)}: '{loadedAttack}'";


		DBG_spatial = $"{nameof(canSeePlayer)}: '{canSeePlayer}'\n" +
			 $"dot_facingToPlayer: '{dot_facingToPlayer}'\n" +
			$"dist_toPlayer: '{dist_toPlayer}'\n" +
			 $"{nameof(PlrCurrentlyMovingTowardsMe)}: '{PlrCurrentlyMovingTowardsMe}'\n" +
			 $"dist_toEndGoal: '{dist_toEndGoal}'\n" +
			$"dot_toNextPos: '{dot_facingToNextPos}'\n" +
			$"{nameof(v_toGoal)}: '{v_toGoal}'";


		debugTravel = $"{nameof(index_currentlySelectedPatrolPoint)}: '{index_currentlySelectedPatrolPoint}'\n" +
			$"dist_toEndGoal: '{dist_toEndGoal}'\n" +
			$"";
	}
#endif

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(PV_GameManager.Tag_area))
		{
			//print("enemy triggered area");
			LAMS_Area area;
			if (!other.TryGetComponent(out area))
			{
				PV_Debug.LogError($"PV ERROR! Enemy triggered something with a tag of 'PV_Area', name: '{other.name}', but couldn't get the PV_Area script component.");
			}
			else
			{
				if (area_triggering != null && area_triggering != area)
				{
					area_triggering.UnRegisterEntity(this);
				}


				if (area_triggering == null || area_triggering != area)
				{
					area_triggering = area;
					area.EnteredAction();
					area.RegisterEntity(this);
				}
			}
		}
	}

	public override void InitState()
	{
		base.InitState();


		if (myPatrolMode != PatrolMode.Stationary)
		{
			SwitchState(EnemyActionState.Patrolling);
		}
		myMovementState = MovementState.Idle;
		runningTime_spentChasingPlayer = 0f;
		cd_PauseAfterGlimpse = 0f;
		cd_Alert = 0f;
		cd_InvestigateLocation = 0f;
		cd_AggressionBuild = 0f;
		v_nrml_calculated = trans.up;
		resetAnimations();
	}

	/// <summary>
	/// An intelligent, "managed" movement method that calculates the appropriate parameters for movement such as level of haste, before
	/// using these parameters by passing them to a lower-level movement method, then handles animation for movement.
	/// </summary>
	/// <param name="endGoalDistThresh">This parameter tells how close the entity should be to the end goal. This is so that a different distance can be dilineated
	/// for the end goal of a path vs the points on the path.</param>
	protected void TravelToGoal(float endGoalDistThresh)
	{
		try
		{
			//Vector3 v = Vector3.Normalize(MyPath.CurrentGoal - trans.position); //todo: indexoutofrangeexception here - this is older, have since created a cached vector to the goal calculated in the calculateSpatialValues() method, so don't need to do this calc anymore. Using following line for error catching...
			travelToDbg = MyPath.CurrentGoal.ToString(); //todo: indexoutofrangeexception here


		}
		catch (System.Exception)
		{
			PV_GameManager.Instance.PauseGameToOptions();
			throw;
		}


		travelToDbg = string.Empty;


		#region CALCULATIONS-------------------------------------------------------------
		float calculatedMvSpd = MyStats_Base_Enemy.Speed_move_patrol;
		float calculatedRotSpd = MyStats_Base_Enemy.Speed_Rotate_patrol;
		float calculatedDistanceThresh = MyStats_Base_Enemy.dist_RoughlyThere;
		float calculatedAngleThresh = MyStats_Base_Enemy.Percentage_consideredRoughlyFacing_patrolPt;


		bool movementShouldBeFast = false; //For Moving and Rotation
		if (myActionState == EnemyActionState.Chasing || myActionState == EnemyActionState.Engaging || myActionState == EnemyActionState.ApproachingToStrike)
		{
			movementShouldBeFast = true;
			calculatedMvSpd = MyStats_Base_Enemy.Speed_Move_fast;
			calculatedRotSpd = MyStats_Base_Enemy.Speed_Rotate_fast;
			if (MyPath.AmOnEndGoal)
			{
				calculatedDistanceThresh = endGoalDistThresh;
				calculatedAngleThresh = MyStats_Base_Enemy.Percentage_consideredRoughlyFacing_threat;
			}
		}
		#endregion


		travelToDbg += $"distToNextPos: '{dist_toNextPos}', calculatedDistanceThresh: '{calculatedDistanceThresh}'\n" +
			$"dot_toNextPos: '{dot_facingToNextPos}', calculatedAngleThresh: '{calculatedAngleThresh}'\n" +
			//$"{nameof(index_CurrentNavigationPath)}: '{index_CurrentNavigationPath}'\n" +
			$"amonendgoal: '{MyPath.AmOnEndGoal}'\n" +
			$"NextPosition: '{MyPath.CurrentGoal}' \n v_toGoal: '{v_toGoal}'\n";


		CalcSpatialValues_pathing();
		TravelTowards(MyPath.CurrentGoal, calculatedDistanceThresh, calculatedMvSpd, MyPath.CurrentPathPoint.V_normal, calculatedAngleThresh, calculatedRotSpd);


		#region SET MOVEMENT STATE --------------------------------------------
		MovementState mvmtState_before = myMovementState;


		if (dist_toNextPos >= calculatedDistanceThresh && dot_facingToNextPos >= calculatedAngleThresh)
		{
			travelToDbg += $"State: Moving...\n";
			myMovementState = movementShouldBeFast ? MovementState.Running : MovementState.Walking;
			CalcSpatialValues_pathing();


		}
		else if (dist_toNextPos < calculatedDistanceThresh)
		{
			travelToDbg += $"State: Idle...\n";
			myMovementState = MovementState.Idle;
		}
		else if (dot_facingToNextPos < calculatedAngleThresh)
		{
			travelToDbg += $"State: turning in place...\n";


			myMovementState = MovementState.TurninngInPlace;
			CalcSpatialValues_pathing();
		}
		#endregion


		#region HANDLE ANIMATIONS-------------------------------------------
		if (myMovementState != mvmtState_before)
		{
			anim.SetBool(animID_Walking, myMovementState == MovementState.Walking || myMovementState == MovementState.TurninngInPlace);
			anim.SetBool(animID_Running, myMovementState == MovementState.Running);


		}
		#endregion


		#region INCREMENT INDEX ---------------------------------------------
		try
		{
			if (dist_toNextPos < calculatedDistanceThresh && !MyPath.AmOnEndGoal)
			{
				// Turn on/off gravity before incrementing index
				Log($"TravelPath() is finding new point. Index_currentPoint: '{MyPath.Index_currentPoint}', current length: '{MyPath.PathPoints.Count}'. " +
					$"TurnOnGravity: '{MyPath.CurrentPathPoint.TurnOnGravity}', TurnOffGravity: '{MyPath.CurrentPathPoint.TurnOffGravity}'");
				if (MyPath.CurrentPathPoint.TurnOnGravity)
				{
					//Debug.LogWarning($"turning on gravity for: '{name}'");
					rb.useGravity = true;
				}
				else if (MyPath.CurrentPathPoint.TurnOffGravity)
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

	public override void SwitchState(EnemyActionState state_passed)
	{
		Log_MethodStart($"SwitchState({state_passed})");
		EnemyActionState prevState = myActionState;
		if (state_passed == EnemyActionState.Patrolling)
		{
			myActionState = EnemyActionState.Patrolling;
			runningTime_spentChasingPlayer = 0f;
		}
		else if (state_passed == EnemyActionState.CaughtAGlimpse)
		{
			myActionState = EnemyActionState.CaughtAGlimpse;
			cd_PauseAfterGlimpse = Random.Range(2.2f, 4.2f) - Mathf.Min(cdSuspicionBuild * 0.5f, 0f); // subtracting the right part of this makes it so that our pauseAfterGlimpse is shorter if our cdSuspicionBuild is higher
		}
		else if (state_passed == EnemyActionState.Suspicious)
		{
			myActionState = EnemyActionState.Suspicious;
		}
		else if (state_passed == EnemyActionState.HeardSuspiciousNoise)
		{
			myActionState = EnemyActionState.HeardSuspiciousNoise;
			cd_PauseAfterGlimpse = Random.Range(1f, 2f);
		}
		else if (state_passed == EnemyActionState.Chasing)
		{
			if (myActionState == EnemyActionState.Engaging)
			{
				MGR_BugEnemy.numbEngaging--;
			}

			myActionState = EnemyActionState.Chasing;
			cd_Alert = MyStats_Base_Enemy.Duration_alertPursuit;
		}
		else if (state_passed == EnemyActionState.Engaging)
		{
			myActionState = EnemyActionState.Engaging;
			MGR_BugEnemy.numbEngaging++;
			cuTimeSinceLastStrike = MyStats_Base_Enemy.Duration_MinWaitBetweenAttacks;
		}
		else if (state_passed == EnemyActionState.ApproachingToStrike)
		{
			myActionState = EnemyActionState.ApproachingToStrike;
		}
		else if (state_passed == EnemyActionState.Attacking)
		{
			myActionState = EnemyActionState.Attacking;
		}

		Log($"Bug enemy: '{name}' state switched to '{myActionState}' from previous state of: '{prevState}'");
	}

	[TextArea(1, 10), SerializeField] protected string dbgDecideNextAction;
	public void DecideNextAction(float timeMult_passed)
	{
		if (!AmTotallyPreoccupied && myActionState == EnemyActionState.Engaging)
		{
			//LogHistoric( $"DecideNextAction()", PV_LogFormatting.UserMethod );

			if (cd_ApproachingPlayerBeforeStriking <= 0f)
			{
				dbgDecideNextAction = "";
				/////////--------GENERATE "LIKELIHOOD VALUES------------------------/////////////
				// ATTACK--------
				float likelihood_attack_calculated = 0f;
				if (dot_facingToPlayer > MyStats_Base_Enemy.Percentage_consideredRoughlyFacing_threat && cuTimeSinceLastStrike > MyStats_Base_Enemy.Duration_MinWaitBetweenAttacks) //Bare minimum to attack
				{//TODO: if enemy gets too close to player, he won't do anything, and it seems because of the above check 'angToPlrAbs < roughlyFacing*2f', angToPlrAbs evaluates to too large
					dbgDecideNextAction += "bare minimum conditions for attack met...\n";
					if (cd_AggressionBuild < 7f) //The idea here is that this value will only count up to 7
					{
						if (PlrCurrentlyMovingTowardsMe)
							cd_AggressionBuild += 0.1f;

						cd_AggressionBuild += (runningTime_spentChasingPlayer / 30f);

						if (cd_AggressionBuild > 7f)
							cd_AggressionBuild = 7f;
					}
					dbgDecideNextAction += $"{nameof(MGR_BugEnemy.numbEngaging)}: '{MGR_BugEnemy.numbEngaging}'\n";
					dbgDecideNextAction += $"{nameof(cd_AggressionBuild)}: '{cd_AggressionBuild}'\n";
					dbgDecideNextAction += $"{nameof(cuTimeSinceLastStrike)}: '{cuTimeSinceLastStrike}'\n";

					likelihood_attack_calculated = ((MGR_BugEnemy.numbEngaging * 0.2f) + (cuTimeSinceLastStrike * 0.1f) + cd_AggressionBuild) * timeMult_passed;

				}
				dbgDecideNextAction += $"{nameof(likelihood_attack_calculated)}: '{likelihood_attack_calculated}'\n";

				/////////////////-------------GENERATE DECISION----------------------///////////////
				float myNextMove = Random.Range(0f, 100f); //Decides random variable representing next move
				dbgDecideNextAction += $"{nameof(myNextMove)}: '{myNextMove}'\n";
				dbgDecideNextAction += $"{nameof(MyStats_Base_Enemy.AggressionLevel)}: '{MyStats_Base_Enemy.AggressionLevel}'\n";

				if (myNextMove <= (MyStats_Base_Enemy.AggressionLevel + likelihood_attack_calculated))
				{
					float attackChoice = Random.Range(0f, 100f);
					dbgDecideNextAction += $"{nameof(attackChoice)}: '{attackChoice}'\n";

					float likelihood_charge = MyStats.Attack_charge.Likelihood + (Mathf.Clamp(dist_toPlayer / 10, 0f, 1f) * 10f);
					dbgDecideNextAction += $"{nameof(likelihood_charge)}: '{likelihood_charge}'\n";

					if (attackChoice <= likelihood_charge)
					{
						Log($"Landed on charge attack!");
						loadedAttack = MyStats.Attack_charge;
						Attack();
						myCurrentAction = Actions_bugEnemy.ChargeAttack;
						//cd_ApproachingPlayerBeforeStriking = MyStats.Duration_ApproachWithIntentToStrike;
						Log($"{name} Initiating attack...");
					}
					else //if (attackChoice <= (MyStats.Attack_basicStrike.Likelihood * (1f / dist_toPlayer)))
					{
						Log($"Landed on regular attack!");
						loadedAttack = MyStats.Attack_basicStrike;
						SwitchState(EnemyActionState.ApproachingToStrike);
						myCurrentAction = Actions_bugEnemy.BasicStrike;
						cd_ApproachingPlayerBeforeStriking = MyStats_Base_Enemy.Duration_ApproachWithIntentToStrike;
						Log($"{name} Initiating attack...");
					}
				}
			}
		}
	}

	#region OFFENSIVE ------------------------------------------------------
	public void Attack()
	{
		Log_MethodStart("Attack()");
		anim.SetInteger(animID_AttackChoice, loadedAttack.AnimChoiceNumber);
		anim.SetTrigger(animID_Attack);
		cuTimeSinceLastStrike = 0f;
		cd_ApproachingPlayerBeforeStriking = 0f;
		SwitchState(EnemyActionState.Attacking);
		flag_amInTotallyPreoccupyingAnimation = true;
	}

	/// <summary>
	/// Starts the actually damaging part of the charge animation. Gets called via animation event after a certain amount through charge animation.
	/// </summary>
	protected void StartCharge()
	{
		PV_Debug.LogWithConsoleConditional("StartCharge()", true, PV_LogFormatting.UserMethod);
		myAttackState = AttackState.MidStrike;
		ArmDamageCollider();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		rb.AddRelativeForce(Vector3.forward * MyStats.Speed_charging, ForceMode.VelocityChange);
		//Time.timeScale = 0.1f;
	}

	/// <summary>Triggers the stop of the forward movement of a charge. This method is currently necessary in addition to the
	/// EndOfAttackAnimEvent because I want to stop the movement independently of stoping the attack colliders.</summary>
	protected void EndCharge()
	{
		PV_Debug.LogWithConsoleConditional("EndCharge()", true, PV_LogFormatting.UserMethod);
		myAttackState = AttackState.None;
		//Time.timeScale = 1f;
	}
	#endregion

	protected void resetAnimations()
	{
		anim.SetBool(animID_Walking, false);
		anim.SetBool(animID_Running, false);

		flag_amInTotallyPreoccupyingAnimation = false;
		myAttackState = AttackState.None;
	}

	public override void TakeDmg(int amt, float dmgForce, Vector3 damageOriginPosition, RaycastHit rcHit )
	{
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		base.TakeDmg( amt, dmgForce, damageOriginPosition, rcHit );

		if (hp > 0)
		{
			anim.SetInteger(animID_DamageClip, Random.Range(0, 3));
			anim.SetTrigger(animID_TakingDamage);
			flag_amInTotallyPreoccupyingAnimation = true;

			cd_takingDamage = MyStats_Base_Enemy.Duration_cd_TakingDamage;
			if (myActionState < EnemyActionState.Chasing && !canSeePlayer) //If we were hit without being aware of the player...
			{
				SwitchState(EnemyActionState.Suspicious);
				UpdatePath(playerTransform.position, 5f);
				v_plrLastSeen = MyPath.EndGoal;

				cd_InvestigateLocation = Random.Range(20f, 30f);
			}
		}
		else
		{
			Log($"{name} has died!");
			myEntityState = EntityState.Dead;
			anim.SetTrigger(animID_Dying);
			if (myColliders != null && myColliders.Count > 0)
			{
				foreach (Collider col in myColliders)
				{
					col.enabled = false;
				}
			}
			rb.isKinematic = true;
			rb.detectCollisions = false;
			MGR_BugEnemy.Instance.RegisterDeadBug(gameObject);
			enabled = false;
		}

		PV_Environment.Instance.Pool_bloodDamageEffectsA_purple.CycleSpawnExact( 
			rcHit.point + (Vector3.Normalize(rcHit.point - trans.position) * 0.2f), Quaternion.FromToRotation(Vector3.up, rcHit.normal)
			);

		if( myFootSystem.MyFootState == LogansFootLogicSystem.LFLS_FootState.Grounded || myFootSystem.MyFootState == LogansFootLogicSystem.LFLS_FootState.Sliding )
		{
			PV_Environment.Instance.Pool_bloodSpotA_purple.CycleSpawn( 
				myFootSystem.CurrentGroundPos + (myFootSystem.CurrentGroundNormal * 0.05f), -myFootSystem.CurrentGroundNormal );
		}
	}

	public override void ActivateMeViaArea()
	{
		Log($"ActivateMeViaArea('{name}')");

		MGR_BugEnemy.Instance.ActivateBug(this);
	}
	public override void DeactivateMeViaArea()
	{
		Log($"DeactivateMeViaArea('{name}')");

		MGR_BugEnemy.Instance.DeactivateBug(this);
	}

	/// <summary>
	/// This is intended only for use in edit mode. If in play mode, go through the Manager classes to do this.
	/// </summary>
	[ContextMenu("z call RemoveMeFromScene()")]
	public void RemoveMeFromScene()
	{
		if (!Application.isEditor)
		{
			PV_Debug.LogError("This method is NOT intended for runtime removal. Use the manager classes for that instead.");
			return;
		}

		try
		{
			PV_SceneDebugger scnDbgr = GameObject.FindGameObjectWithTag(
				PV_GameManager.Tag_SceneDebugger).GetComponent<PV_SceneDebugger>();
			scnDbgr.RemoveBugFromSceneViaEditor(this);
		}
		catch (System.Exception)
		{
			PV_Debug.LogWarning($"Couldn't find the SceneDebugger by tag. Returning early...");
			return;
		}
		PV_Debug.Log("Now destroying object...");

		DestroyImmediate(gameObject);
	}

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

		return amKosher;
	}
}