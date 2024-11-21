using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PV_Enums;
using UnityEngine.AI;
using PV_Utils;
using LogansAreaManagementSystem;

public class Enemy_BasicFlyer : Base_Enemy_airborn
{
	public Stats_Enemy_basicFlyer MyStats => MGR_BasicFlyer.Instance.Stats_basicFlyer;

	[Header("---------------[[ STATE ]]-----------------")]
	protected Actions_BasicFlyer myCurrentAction;
	public Actions_BasicFlyer MyCurrentAction => myCurrentAction;


	[Header("---------------[[ TRUTH ]]-----------------")]
	/// <summary>Allows selectively turning certain features for this object off in the case of a potentially game-crashing error.</summary>
	protected bool amBugged = false;

	//[Header("---------------[[ ANIMATION ]]-----------------")]
	public static int animID_Flying;
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

        MGR_BasicFlyer.Instance.RegisterBasicFlyer( this );
        area_triggering.RegisterEntity( this );

    }

    protected override void Start()
    {
        Log_MethodStart( $"Start() entered", PV_LogDestination.Hidden, PV_LogFormatting.UnityAPIMethod );
		base.Start();

		CheckIfKosher();

		InitState();

    }

    void FixedUpdate()
    {
		if ( !amPerformingLoopLogic )
		{
			return;
		}

		MoveAlarms();
        calcSpatialValues();

        #region DECIDE STATE---------------------------------------------------
        if (myActionState == EnemyActionState.Patrolling )
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
        else if (myActionState == EnemyActionState.CaughtAGlimpse )
        {
            if( cd_PauseAfterGlimpse <= 0 )
            {
                SwitchState( EnemyActionState.Suspicious );
            }
        }
        else if (myActionState == EnemyActionState.HeardSuspiciousNoise )
        {
            if ( cd_PauseAfterGlimpse <= 0 )
            {
                SwitchState(EnemyActionState.Suspicious);
                cd_InvestigateLocation = Random.Range(5f, 8f);

            }
        }
        else if (myActionState == EnemyActionState.Suspicious )
        {
            if ( cd_Alert > 0 )
            {
                SwitchState( EnemyActionState.Chasing );
            }
            else if ( cd_InvestigateLocation == 0 )
            {
                SwitchState( EnemyActionState.Patrolling );
            }
        }
        else if (myActionState == EnemyActionState.Chasing )
        {
            runningTime_spentChasingPlayer += Time.fixedDeltaTime;

            if ( cd_Alert == 0 )
            {
                SwitchState( EnemyActionState.Patrolling );
            }

            if ( dist_toPlayer < MyStats_Base_NPC.Dist_TriggerEngaging )
            {
                SwitchState( EnemyActionState.Engaging );
            }
        }
        else if (myActionState == EnemyActionState.Engaging )
        {
            //if (!plrVisible || distToPlayer > dist_consideredEngaging)
            if ( dist_toPlayer > MyStats_Base_NPC.Dist_TriggerChasing_fromEngaging )
            {
                SwitchState( EnemyActionState.Chasing );
            }
        }
        else if(myActionState == EnemyActionState.ApproachingToStrike )
        {
            if( cd_ApproachingPlayerBeforeStriking <= 0 )
            {
                SwitchState( EnemyActionState.Engaging );
            }
        }

        #endregion

        if ( myPatrolMode != PatrolMode.Stationary && myActionState == EnemyActionState.Patrolling )
        {
            if ( cd_PatrolWait <= 0f )
            {
                TravelToGoal( MyStats_Base_NPC.dist_RoughlyThere );

                if ( dist_toEndGoal <= MyStats_Base_NPC.dist_RoughlyThere )
                {
                    Log( $"Bug enemy has reached end goal of: '{MyPath.EndGoal}', and is negotiating a patrol wait state..." );
                    GenerateNewPath();
                    Log($"Bug enemy: '{name}', settled on a patrol wait of '{cd_PatrolWait}', and an end goal of: '{MyPath.EndGoal}'...");

                }
            }
        }
        else if ( myActionState == EnemyActionState.CaughtAGlimpse )
        {
            LookToward( MyPath.EndGoal, Vector3.up, MyStats_Base_NPC.Speed_Rotate_fast );
        }
        else if ( myActionState == EnemyActionState.Suspicious )
        {
            TravelToGoal( MyStats_Base_NPC.dist_RoughlyThere );
        }
        else if ( myActionState == EnemyActionState.Chasing )
        {
            TravelToGoal( MyStats_Base_NPC.dist_RoughlyThere );
        }
        else if ( myActionState == EnemyActionState.Engaging )
        {
            TravelToGoal( MyStats_Base_NPC.Dist_TriggerChasing_fromEngaging );

        }
        else if ( myActionState == EnemyActionState.ApproachingToStrike )
        {
            TravelToGoal( loadedAttack.Reach );
            if( myCurrentAction == Actions_BasicFlyer.BasicStrike )
            {
                if( dist_toPlayer < MyStats.Attack_basicStrike.Reach && dot_facingToPlayer > MyStats_Base_NPC.Percentage_consideredRoughlyFacing_threat )
                {
                    Attack();
                }
            }
        }
        else if ( myActionState == EnemyActionState.Attacking )
        {
            /*
            if( myCurrentAction == Actions_BasicFlyer.ChargeAttack )
            {
                if( myAttackState == AttackState.Striking )
                {
                    //rb.MovePosition(trans.position + (trans.forward * MyStats.Speed_charging * Time.fixedDeltaTime)); //todo
                    rb.AddForce(trans.forward * 3f, ForceMode.Force);
                }
                else if( myAttackState == AttackState.Recovering )
                {
                    rb.AddForce( -(rb.velocity.normalized * 300.5f), ForceMode.Force);
                }
            }
            */
        }        
    }

    private void LateUpdate()
    {
        DBG_State = "" +
        //$"{buggedColorString}{nameof(amBugged)}: '{amBugged}'</color>\n" +
        $"{nameof(myEntityState)}: '{myEntityState}'\n" +
        $"{nameof(myActionState)}: '{myActionState}'\n" +
        $"{nameof(myMovementState)}: '{myMovementState}'\n" +
        $"{nameof(myCurrentAction)}: '{myCurrentAction}'\n" +

        $"{nameof(hp)}: '{hp}'\n" +
        "========================================\n" +
        $"{nameof(canSeePlayer)}: '{canSeePlayer}'\n" +
        $"{nameof(dist_toPlayer)}: '{dist_toPlayer}'\n" +
        $"{nameof(dot_facingToPlayer)}: '{dot_facingToPlayer}'\n";

        DBG_spatial = $"dot_facingToPlayer: '{dot_facingToPlayer}'\n" +
            $"dist_toPlayer: '{dist_toPlayer}'\n" +
            $"player current visibility: '{playerScript.CurrentVisibility}'\n" +
            $"dist_toEndGoal: '{dist_toEndGoal}'\n" +
            $"dot_toNextPos: '{dot_facingToNextPos}'\n" +
            $"";
    }

    private void OnTriggerEnter(Collider other)
    {
        if ( other.CompareTag("PV_Area") )
        {
            //print("enemy triggered area");
            LAMS_Area area;
            if ( !other.TryGetComponent(out area) )
            {
                PV_Debug.LogError($"PV ERROR! Enemy triggered something with a tag of 'PV_Area', name: '{other.name}', but couldn't get the PV_Area script component.");
            }
            else
            {
                if( area_triggering != null && area_triggering != area )
                {
                    area_triggering.UnRegisterEntity(this);
                }

                if ( area_triggering == null || area_triggering != area )
                {
                    area_triggering = area;
                    area_triggering.EnteredAction();
                    area_triggering.RegisterEntity(this);
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
		resetAnimations();
	}

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
		float mvSpd = MyStats_Base_NPC.Speed_move_patrol;
		float rtSpd = MyStats_Base_NPC.Speed_Rotate_patrol;
		float calculatedDistanceThresh = MyStats_Base_NPC.dist_RoughlyThere;
		float calculatedAngleThresh = MyStats_Base_NPC.Percentage_consideredRoughlyFacing_patrolPt;

		bool movementShouldBeFast = false; //For Moving and Rotation
		if (myActionState == EnemyActionState.Chasing || myActionState == EnemyActionState.Engaging || myActionState == EnemyActionState.ApproachingToStrike)
		{
			movementShouldBeFast = true;
			mvSpd = MyStats_Base_NPC.Speed_Move_fast;
			rtSpd = MyStats_Base_NPC.Speed_Rotate_fast;
			if (MyPath.AmOnEndGoal)
			{
				calculatedDistanceThresh = endGoalDistThresh;
				calculatedAngleThresh = MyStats_Base_NPC.Percentage_consideredRoughlyFacing_threat;
			}
		}
		#endregion

		travelToDbg += $"distToNextPos: '{dist_toNextPos}', calculatedDistanceThresh: '{calculatedDistanceThresh}'\n" +
			$"dot_toNextPos: '{dot_facingToNextPos}', calculatedAngleThresh: '{calculatedAngleThresh}'\n" +
			//$"{nameof(index_CurrentNavigationPath)}: '{index_CurrentNavigationPath}'\n" +
			$"amonendgoal: '{MyPath.AmOnEndGoal}'\n" +
			$"NextPosition: '{MyPath.CurrentGoal}' \n v_toGoal: '{v_toGoal}'\n";

		CalcSpatialValues_pathing();
		TravelTowards(MyPath.CurrentGoal, calculatedDistanceThresh, mvSpd, Vector3.up, calculatedAngleThresh, rtSpd);

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
			anim.SetBool(animID_Flying, myMovementState == MovementState.FlyingSlow || myMovementState == MovementState.TurninngInPlace); //todo: "hash does not exist"

			//anim.SetBool( animID_Running, myMovementState == MovementState.FlyingFast );

		}
		#endregion

		#region INCREMENT INDEX ---------------------------------------------
		try
		{
			if (dist_toNextPos < calculatedDistanceThresh && !MyPath.AmOnEndGoal)
			{
				// Turn on/off gravity before incrementing index
				Log($"TravelPath() is finding new point. Index_currentPoint: '{MyPath.Index_currentPoint}', current length: '{MyPath.PathPoints.Count}'. " +
					$"TurnOnGravity: '{MyPath.CurrentPathPoint.flag_switchGravityOn}', TurnOffGravity: '{MyPath.CurrentPathPoint.flag_switchGravityOff}'");
				if (MyPath.CurrentPathPoint.flag_switchGravityOn)
				{
					//Debug.LogWarning($"turning on gravity for: '{name}'");
					rb.useGravity = true;
				}
				else if (MyPath.CurrentPathPoint.flag_switchGravityOff )
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

	public override void SwitchState( EnemyActionState state_passed )
	{
		Log_MethodStart( $"SwitchState({state_passed})", PV_LogDestination.Hidden, PV_LogFormatting.UserMethod );
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
			cd_Alert = MyStats_Base_NPC.Duration_alertPursuit;
		}
		else if (state_passed == EnemyActionState.Engaging)
		{
			myActionState = EnemyActionState.Engaging;
			MGR_BugEnemy.numbEngaging++;
			cuTimeSinceLastStrike = MyStats_Base_NPC.Duration_MinWaitBetweenAttacks;
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
				if (dot_facingToPlayer > MyStats_Base_NPC.Percentage_consideredRoughlyFacing_threat && cuTimeSinceLastStrike > MyStats_Base_NPC.Duration_MinWaitBetweenAttacks) //Bare minimum to attack
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
					dbgDecideNextAction += $"{nameof(MGR_BasicFlyer.numbEngaging)}: '{MGR_BasicFlyer.numbEngaging}'\n";
					dbgDecideNextAction += $"{nameof(cd_AggressionBuild)}: '{cd_AggressionBuild}'\n";
					dbgDecideNextAction += $"{nameof(cuTimeSinceLastStrike)}: '{cuTimeSinceLastStrike}'\n";

					likelihood_attack_calculated = ((MGR_BasicFlyer.numbEngaging * 0.2f) + (cuTimeSinceLastStrike * 0.1f) + cd_AggressionBuild) * timeMult_passed;


				}
				dbgDecideNextAction += $"{nameof(likelihood_attack_calculated)}: '{likelihood_attack_calculated}'\n";

				/////////////////-------------GENERATE DECISION----------------------///////////////
				float myNextMove = Random.Range(0f, 100f); //Decides random variable representing next move
				dbgDecideNextAction += $"{nameof(myNextMove)}: '{myNextMove}'\n";
				dbgDecideNextAction += $"{nameof(MyStats_Base_NPC.AggressionLevel)}: '{MyStats_Base_NPC.AggressionLevel}'\n";

				if (myNextMove <= (MyStats_Base_NPC.AggressionLevel + likelihood_attack_calculated))
				{

					//float attackChoice = Random.Range(0f, 100f); //this will be implemented when there are multiple attacks.
					//dbgDecideNextAction += $"{nameof(attackChoice)}: '{attackChoice}'\n";

					dbgDecideNextAction += $"Landed on regular attack!\n";
					loadedAttack = MyStats.Attack_basicStrike;
					SwitchState(EnemyActionState.ApproachingToStrike);
					myCurrentAction = Actions_BasicFlyer.BasicStrike;
					cd_ApproachingPlayerBeforeStriking = MyStats_Base_NPC.Duration_ApproachWithIntentToStrike;

					Log($"{name} Initiating '{myCurrentAction}' attack...");

				}
			}
		}
	}

	#region OFFENSIVE ------------------------------------------------------
	public void Attack()
	{
		PV_Debug.LogWithConsoleConditional("Attack()", true, PV_LogFormatting.UserMethod);
		anim.SetInteger(animID_AttackChoice, loadedAttack.AnimChoiceNumber);
		anim.SetTrigger(animID_Attack);
		cuTimeSinceLastStrike = 0f;
		cd_ApproachingPlayerBeforeStriking = 0f;
		SwitchState(EnemyActionState.Attacking);
		flag_amInTotallyPreoccupyingAnimation = true;
	}

	#endregion

	public override void ActivateMeViaArea()
	{
		Log($"ActivateMeViaArea('{name}')");

		MGR_BasicFlyer.Instance.ActivateBasicFlyer(this);

	}

	public override void DeactivateMeViaArea()
	{
		Log($"DeactivateMeViaArea('{name}')");

		MGR_BasicFlyer.Instance.UnregisterAndDeactivateBasicFlyer(this);
	}

	protected void resetAnimations()
	{
		anim.SetBool(animID_Flying, false);

		flag_amInTotallyPreoccupyingAnimation = false;
		myAttackState = AttackState.None;
	}

	public override void TakeDmg(int amt, float dmgForce, Vector3 damageOriginPosition, RaycastHit rcHit )
	{
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		base.TakeDmg( amt, dmgForce, damageOriginPosition, rcHit );

		//resetAnimations(); //This just set the enemy back to idle.
		if (hp > 0)
		{
			anim.SetInteger(animID_DamageClip, Random.Range(0, 3));
			anim.SetTrigger(animID_TakingDamage);
			flag_amInTotallyPreoccupyingAnimation = true;

			cd_takingDamage = MyStats_Base_NPC.Duration_cd_TakingDamage;
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
			MGR_BasicFlyer.Instance.RegisterDeadBasicFlyer(gameObject);
			enabled = false;
		}

		PV_Environment.Instance.Pool_bloodDamageEffectsA_green.CycleSpawnExact(
			rcHit.point, Quaternion.FromToRotation(Vector3.up, rcHit.normal)
			);
	}

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

		return amKosher;
	}
}