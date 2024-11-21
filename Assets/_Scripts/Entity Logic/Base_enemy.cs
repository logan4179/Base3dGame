using LogansAreaManagementSystem;
using PV_Enums;
using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Base_enemy : base_NPC
{
    [Header("---------------[[ REFERENCE - INTERNAL (Base_enemy) ]]-----------------")]
   // protected NavMeshAgent nmAgent = null;
	[SerializeField] protected List<Collider> myColliders;
	[SerializeField] protected EnemyAttackCollider myAttackCollider;
    public EnemyAttackCollider MyAttackCollider => myAttackCollider;

    [Header("---------------[[ REFERENCE - EXTERNAL (Base_enemy) ]]-----------------")]
    /// <summary>The area that we're triggering.</summary>
    [SerializeField] protected LAMS_Area area_triggering = null;
	protected PV_Player playerScript;
    protected Transform playerTransform;     

    [Header("---------------[[ STATE (Base_enemy) ]]-----------------")]
	[TextArea(1, 10), SerializeField] protected string DBG_State;
	protected EnemyType MyType = EnemyType.Bug;
	protected EnemyActionState myActionState = EnemyActionState.Patrolling;
    public EnemyActionState MyActionState => myActionState;

    [SerializeField] protected PatrolMode myPatrolMode; //this needs to stay serialized so it can be set through inspector on each enemy
    protected AttackState myAttackState;
    protected MovementState myMovementState;
    public MovementState MyMovementState => myMovementState;
    protected PV_Attack loadedAttack;
    public PV_Attack LoadedAttack => loadedAttack;

    [Header("---------------[[ TRUTH (Base_enemy) ]]-----------------")]
    protected bool canSeePlayer = false;
    public bool CanSeePlayer => canSeePlayer;
    /// <summary>Tracks whether the player is currently moving towards this enemy at this exact moment.</summary>
    protected bool PlrCurrentlyMovingTowardsMe = false;

    [Header("---------------[[ SPATIAL (Base_enemy) ]]-----------------")]
    protected float dist_toPlayer;
    /// <summary>This is used to decide if a player is currently moving basically towards, or basically away from me. </summary>
    protected float playerMovingTowardMeAmt;
    protected float dot_facingToPlayer;
    protected Vector3 v_plrLastSeen, v_toPlrLastSeen = Vector3.zero;

	/// <summary>
	/// This returns the playerposition, as far as this enemy is concerned. The player's position is actually placed at the lowest point of the player 
	/// prefab, but sometimes we don't want the enemy to consider that the player position. IE: flying enemies should actually consider the player's position 
	/// to be higher because we don't want them flying toward the player's feet when chasing.
	/// </summary>
	public virtual Vector3 PlayerFocusPosition
	{
		get { return playerTransform.position; }
	}

	[Header("---------------[[ ALARMS (Base_enemy) ]]-----------------")]
    [Tooltip("Random wait time after patrolling to a location before patrolling to a new point.")]
    protected float cd_PatrolWait = 0;
    /// <summary>Builds to 100 while the player is visible.  If This hits 100, the enemy goes into 
    /// "chasing" and 'cdAlert' gets given a value.</summary>
    protected float cdSuspicionBuild;
    /// <summary>Gets activated when the enemy is sure of the player's presence (IE: when 'cdSuspicionBuild' 
    /// goes past 100 while the player is visible). Only decreases inside 'moveAlarms' if the player is not
    /// curently visible. TODO: Although not implemented yet, this countdown will provide a way to make the
    /// enemy continue looking for the player in a heightened-state mode after losing track of the player. 
    /// Until I implement that, it doesn't serve much of a function other than deciding when to go into chase mode.</summary>
    protected float cd_Alert = 0f;
    /// <summary>Brief pause when the enemy catches a glimpse of the player before patrolling over to investigate the location.</summary>
    protected float cd_PauseAfterGlimpse = 0f;
    /// <summary>Random amount of time enemy will spend looking around the location where he caught a glimpse of the player before returning to his patrol.</summary>
    protected float cd_InvestigateLocation = 0f;
    protected float cuTimeSinceLastStrike = 0f;
    protected float cd_AggressionBuild = 0f;
    protected float cd_ApproachingPlayerBeforeStriking;
    protected float cu_timeSincePlayerLastSeen = 0f;
    protected float cd_takingDamage = 0f;

	[Header("---------------[[ OTHER ]]-----------------")]
	protected float runningTime_spentChasingPlayer = 0f;

	protected override void Awake()
	{
        base.Awake();

		area_triggering.RegisterEntity( this );

		playerScript = PV_GameManager.Instance.PlayerScript;
		playerTransform = PV_GameManager.Instance.Trans_Player;
        trans_patrolPointGrabber.gameObject.SetActive( false );
	}

	protected override void Start()
	{
		base.Start();

        CheckIfKosher();

        flag_amEnemy = true;

		dist_toPlayer = Vector3.Distance(trans.position, PV_GameManager.Instance.Trans_Player.position);

        if( myMovementMode != EntityMovementMode.Stationary )
        {
		    index_currentlySelectedPatrolPoint = -1;
		    GenerateNewPath();
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
	}

	protected override void MoveAlarms()
    {
        base.MoveAlarms();

        if (cd_Alert > 0f && !canSeePlayer)
        {
            cd_Alert -= Time.deltaTime;

            if (cd_Alert <= 0f)
            {
                cd_Alert = 0f;
            }
        }

        if (cd_PatrolWait > 0f)
        {
            cd_PatrolWait -= Time.deltaTime;

            if (cd_PatrolWait <= 0f)
                cd_PatrolWait = 0f;
        }

        if (cd_PauseAfterGlimpse > 0f)
        {
            cd_PauseAfterGlimpse -= Time.deltaTime;

            if (cd_PauseAfterGlimpse <= 0f)
                cd_PauseAfterGlimpse = 0f;
        }

        if (cd_AggressionBuild > 0f && myActionState != EnemyActionState.Engaging)
        {
            cd_AggressionBuild -= Time.deltaTime;

            if (cd_AggressionBuild <= 0f)
                cd_AggressionBuild = 0f;
        }

        if (cd_InvestigateLocation > 0f)
        {
            if ( dist_toEndGoal < 5f)
            {
                cd_InvestigateLocation -= Time.deltaTime;

                if (cd_InvestigateLocation <= 0f)
                    cd_InvestigateLocation = 0f;
            }
            else if (cu_timeSincePlayerLastSeen > 30f) //this is in case of a scenario where the enemy, because of an environmental obstruction, can't investigate the location and doesn't get stuck in this state for too long...
            {
                cd_InvestigateLocation = 0f;
            }
        }

        if (cd_ApproachingPlayerBeforeStriking > 0f)
        {
            cd_ApproachingPlayerBeforeStriking -= Time.deltaTime;

            if (cd_ApproachingPlayerBeforeStriking <= 0f)
            {
                cd_ApproachingPlayerBeforeStriking = 0f;
                SwitchState(EnemyActionState.Engaging);
            }
        }

		if (cd_takingDamage > 0f)
		{
			flag_amInTotallyPreoccupyingAlarm = true;
			cd_takingDamage -= Time.deltaTime;

			if (cd_takingDamage <= 0f)
			{
				cd_takingDamage = 0f;
				rb.constraints = RigidbodyConstraints.FreezeRotationZ;
			}
		}

		if (!canSeePlayer)
        {
            cu_timeSincePlayerLastSeen += Time.deltaTime;

			if (cdSuspicionBuild > 0f)
			{
				cdSuspicionBuild -= Time.deltaTime * 50f; //At cdSuspicionBuild == 100, this should take 2 seconds to go completely down to 0

				if (cdSuspicionBuild < 0f)
					cdSuspicionBuild = 0f;
			}
		}

        if (myActionState == EnemyActionState.Engaging)
            cuTimeSinceLastStrike += Time.deltaTime;

		DBG_Alarms = $"cd_PatrolWait: '{cd_PatrolWait}'\n" +
			$"cd_Alert: '{cd_Alert}'\n" +
			$"cd_takingDamage: '{cd_takingDamage}'\n" +
			$"cd_PauseAfterGlimpse: '{cd_PauseAfterGlimpse}'\n" +
			$"cd_InvestigateLocation: '{cd_InvestigateLocation}'\n" +
			$"cdSuspicionBuild: '{cdSuspicionBuild}'\n" +
			$"cd_AggressionBuild: '{cd_AggressionBuild}'\n" +
			$"cd_ApproachingPlayerBeforeStriking: '{cd_ApproachingPlayerBeforeStriking}'\n" +
			$"cu_timeSincePlayerLastSeen: '{cu_timeSincePlayerLastSeen}'\n" +
			$"cuTimeSinceLastStrike: '{cuTimeSinceLastStrike}'\n" +
			$"";
    }

    public virtual void SwitchState(EnemyActionState state_passed)
    {

    }

	[SerializeField, TextArea(1, 5)] protected string debugVisibility;
	public virtual void CalculatePlayerVisibility(float timeMult_passed ) //public so that this can be called from the manager script a certain amount per second.
    {
        debugVisibility = "";

        bool canSeePlayer_prev = canSeePlayer;

		if ( playerScript.MyEntityState != EntityState.Alive )
		{
			debugVisibility += "player state not set to alive. Returning...";
			return;
		}

		if ( dist_toPlayer < (MyBaseStats.Distance_vision * playerScript.CurrentVisibility) &&
			dot_facingToPlayer > MyBaseStats.Threshold_VisionRadius && 
            !Physics.Linecast(Trans_visionOrigin.position, PlayerFocusPosition, PV_Environment.Instance.Mask_CompletelyOpaque) )
		{
            canSeePlayer = true;
			v_plrLastSeen = PlayerFocusPosition;
			v_toPlrLastSeen = v_plrLastSeen - trans.position;
			cu_timeSincePlayerLastSeen = 0f;

			if ( !canSeePlayer_prev )
			{
				UpdatePath( PlayerFocusPosition, 2f );

				Log( "Just saw player. Updated path" );
			}


			if ( myActionState == EnemyActionState.Patrolling )
			{
				SwitchState( EnemyActionState.CaughtAGlimpse );
			}
			else if ( myActionState == EnemyActionState.CaughtAGlimpse || myActionState == EnemyActionState.Suspicious )
			{
				float visionMultiplier_distance = (MyBaseStats.Distance_vision / dist_toPlayer) * playerScript.CurrentVisibility * timeMult_passed;
				float visionMultiplier_directional = Mathf.Clamp(dot_facingToPlayer, 0f, 1f) * playerScript.CurrentVisibility * timeMult_passed;

				cdSuspicionBuild += (visionMultiplier_distance * MyBaseStats.SuspicionMultiplier_distance) + (visionMultiplier_directional * MyBaseStats.SuspicionMultiplier_direction);
				if ( cdSuspicionBuild > 100f )
				{
					SwitchState( EnemyActionState.Chasing );
					cdSuspicionBuild = 100f;
				}
				else if ( cd_InvestigateLocation <= 0f && cdSuspicionBuild >= 8f )
					cd_InvestigateLocation = Random.Range( 5f, 8f );
			}
		}
		else
		{
            canSeePlayer = false;
		}

		debugVisibility = $"dist_toPlayer: '{dist_toPlayer}' {(dist_toPlayer < (MyBaseStats.Distance_vision * playerScript.CurrentVisibility) ? "!!!" : "XXX")}\n" +
            $"dot_facingToPlayer: '{dot_facingToPlayer}' {(dot_facingToPlayer > MyBaseStats.Threshold_VisionRadius ? "!!!" : "XXX")}\n" +
            $"playerScript.MyEntityState: '{playerScript.MyEntityState}' {(playerScript.MyEntityState == EntityState.Alive ? "!!!" : "XXX")}\n" +
            $"CorrectedPlayerPosition: '{PlayerFocusPosition}'\n" +
            $"{nameof(canSeePlayer)}: '{canSeePlayer}'";
	}

	/*
	public void calculatePlayerVisibility(float timeMult_passed) //public so that this can be called from the manager script a certain amount per second. //OLD
    {
        debugVisibility = $"dist_toPlayer: '{dist_toPlayer}' {(dist_toPlayer < (MyBaseStats.Distance_vision * playerScript.CurrentVisibility) ? "!!!" : "XXX")}\n" +
            $"dot_facingToPlayer: '{dot_facingToPlayer}' {(dot_facingToPlayer > MyBaseStats.Threshold_VisionRadius ? "!!!" : "XXX")}\n" +
            $"playerScript.MyEntityState: '{playerScript.MyEntityState}' {(playerScript.MyEntityState == EntityState.Alive ? "!!!" : "XXX")}";

        if ( playerScript.MyEntityState == EntityState.Alive && dist_toPlayer < (MyBaseStats.Distance_vision * playerScript.CurrentVisibility) && dot_facingToPlayer > MyBaseStats.Threshold_VisionRadius
            && !Physics.Linecast(Trans_visionOrigin.position, playerTransform.position + (Vector3.up * playerScript.MyHeight), PV_Environment.Instance.Mask_CompletelyOpaque) )
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(playerTransform.position, out hit, 10f, NavMesh.AllAreas);
            v_plrLastSeen = hit.position;
            v_toPlrLastSeen = v_plrLastSeen - trans.position;
            cu_timeSincePlayerLastSeen = 0f;

            if (!CanSeePlayer)
            {
                CanSeePlayer = true;
                UpdatePath(playerTransform.position, 2f);

                LogHistoric("Just saw player", PV_LogFormatting.Standard);
            }

            if ( MyActionState == EnemyActionState.Patrolling )
            {
                SwitchState(EnemyActionState.CaughtAGlimpse);
            }
            else if (MyActionState == EnemyActionState.CaughtAGlimpse || MyActionState == EnemyActionState.Suspicious)
            {
                float visionMultiplier_distance = (MyBaseStats.Distance_vision / dist_toPlayer) * playerScript.CurrentVisibility * timeMult_passed;
                float visionMultiplier_directional = Mathf.Clamp(dot_facingToPlayer, 0f, 1f) * playerScript.CurrentVisibility * timeMult_passed;

                cdSuspicionBuild += (visionMultiplier_distance * MyBaseStats.SuspicionMultiplier_distance) + (visionMultiplier_directional * MyBaseStats.SuspicionMultiplier_direction);
                if (cdSuspicionBuild > 100f)
                {
                    SwitchState(EnemyActionState.Chasing);
                    cdSuspicionBuild = 100f;
                }
                else if (cd_InvestigateLocation <= 0f && cdSuspicionBuild >= 8f)
                    cd_InvestigateLocation = Random.Range(5f, 8f);
            }
        }
        else
        {
            CanSeePlayer = false;
            if (cdSuspicionBuild > 0f)
            {
                cdSuspicionBuild -= Time.deltaTime * 50f; //At cdSuspicionBuild == 100, this should take 2 seconds to go completely down to 0

                if (cdSuspicionBuild < 0f)
                    cdSuspicionBuild = 0f;
            }
        }
    }
    */

	public virtual void calcSpatialValues()
    {
        dot_facingToPlayer = Vector3.Dot(trans.forward, Vector3.Normalize(PV_GameManager.Instance.Trans_Player.position - trans.position));
        float oldDistToPlr = dist_toPlayer;
        dist_toPlayer = Vector3.Distance( trans.position, PV_GameManager.Instance.Trans_Player.position );
        playerMovingTowardMeAmt = Mathf.Lerp(playerMovingTowardMeAmt, oldDistToPlr - dist_toPlayer, 30f * Time.deltaTime);
        PlrCurrentlyMovingTowardsMe = playerMovingTowardMeAmt > 0 ? true : false;

        CalcSpatialValues_pathing();
    }

    protected override void CalcSpatialValues_pathing()
    {
        base.CalcSpatialValues_pathing();

    }

    public void EmulateEnvironmentalSound(Vector3 pos_passed, float soundProjectDistance_passed)
    {
        Log_MethodStart($"EmulateEnvironmentalSound(pos_passed: '{pos_passed}', soundProjectDistance_passed: '{soundProjectDistance_passed}')");

        float distToSoundOrigin = Vector3.Distance( Trans_visionOrigin.position, pos_passed );
        float dist_soundHeard_calculated = distToSoundOrigin;

		if ( distToSoundOrigin <= (soundProjectDistance_passed * MyBaseStats.HearingMultiplier) )
		{
			Log($"sound distance was '{distToSoundOrigin}', which was within calculated threshold distance: '{soundProjectDistance_passed * MyBaseStats.HearingMultiplier}'. Investigating if it should actually be heard...");

			if ( Physics.Raycast(pos_passed, Vector3.Normalize(Trans_visionOrigin.position - pos_passed), distToSoundOrigin, PV_Environment.Instance.Mask_MufflesSound) )
			{
				Log($"sound is behind muffling object. Determining if this enemy is close enough (within {soundProjectDistance_passed * MyBaseStats.HearingMultiplier * 0.6f} meters)...");
				
                dist_soundHeard_calculated = soundProjectDistance_passed * MyBaseStats.HearingMultiplier * 0.6f;
			}

			if( dist_soundHeard_calculated <= (soundProjectDistance_passed * MyBaseStats.HearingMultiplier) )
			{
				if ( myActionState == EnemyActionState.Patrolling )
				{
					SwitchState( EnemyActionState.HeardSuspiciousNoise );
				}
				UpdatePath( pos_passed, soundProjectDistance_passed );
				Log($"decided that the sound was indeed heard.");
                Debug.DrawLine( trans.position, trans.position + (Vector3.up * 2f), Color.white, 1f );
			}
			else
			{
				Log($"\t decided that the sound was too muffled to be heard.");
			}
		}
		else
		{
			//PV_Debug.Log($"LogSound() decided that the sound was NOT heard for this enemy.");
			Log($"the sound was completely out of range in order to be heard...");
		}




        /*
		if ( distToSoundOrigin <= (soundProjectDistance_passed * MyBaseStats.HearingMultiplier) )
        {
            LogHistoric($"sound distance was '{distToSoundOrigin}', which was within calculated threshold distance: '{soundProjectDistance_passed * MyBaseStats.HearingMultiplier}'. Investigating if it should actually be heard...");

            if ( Physics.Raycast(pos_passed, Vector3.Normalize(Trans_visionOrigin.position - pos_passed), distToSoundOrigin, PV_Environment.Instance.Mask_MufflesSound) )
            {
                LogHistoric($"sound is behind muffling object. Determining if this enemy is close enough (within {soundProjectDistance_passed * MyBaseStats.HearingMultiplier * 0.6f} meters)...");

                if ( distToSoundOrigin <= soundProjectDistance_passed * MyBaseStats.HearingMultiplier * 0.6f )
                {
                    if ( myActionState == EnemyActionState.Patrolling )
                    {
                        SwitchState( EnemyActionState.HeardSuspiciousNoise );
                    }
                    UpdatePath(pos_passed, soundProjectDistance_passed);
                    LogHistoric($"decided that the sound was indeed heard through the muffling object for this enemy.");
                }
                else
                {
                    LogHistoric($"\t decided that the sound was too muffled to be heard.");
                }
            }
            else
            {
                if (myActionState == EnemyActionState.Patrolling)
                {
                    SwitchState(EnemyActionState.HeardSuspiciousNoise);
                }

                LogHistoric($"decided that the sound was heard for this enemy. Creating path to sound...");

                NavMeshHit hit;

                if (NavMesh.SamplePosition(pos_passed, out hit, soundProjectDistance_passed, NavMesh.AllAreas))
                {
                    UpdatePath(hit.position, soundProjectDistance_passed);
                    LogHistoric($"Succesfully created path to end destination: '{hit.position}'");
                }
                else
                {
                    LogHistoric($"For some reason, couldn't get succesful navmesh hit with supplied position of: '{pos_passed}'");
                }
            }
        }
        else
        {
            //PV_Debug.Log($"LogSound() decided that the sound was NOT heard for this enemy.");
            LogHistoric($"the sound was completely out of range in order to be heard...");
        }*/
    }

	[TextArea(2, 5)] public string DBGLookToward;
	/// <summary>
	/// Keeps up with a lerped normal value so that the normal can be transitioned rather than abroptly changed with the next path point.
	/// </summary>
	[SerializeField] protected Vector3 v_nrml_calculated = Vector3.up;
	protected virtual void LookToward(Vector3 pos_passed, Vector3 normal_passed, float rotSpeed_passed )
	{
		DBGLookToward = "";
		Quaternion q = Quaternion.identity;
		Vector3 vRot = Vector3.zero;
		Vector3 vToPos = Vector3.Normalize(pos_passed - trans.position);
		v_nrml_calculated = Vector3.RotateTowards(trans.up, normal_passed, rotSpeed_passed * Time.fixedDeltaTime, 0f);
		float dot_facingToPos = Vector3.Dot(trans.forward, vToPos);
		float dot_upToNrml = Vector3.Dot(trans.up, normal_passed);

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

		if ( dot_facingToPos < -0.98f && dot_upToNrml > 0.95f )
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
		//q = Quaternion.LookRotation(vRot, v_nrml_calculated);

		//Debug.DrawLine(trans.position, trans.position + (v_nrml_calculated * 5f), Color.green);
		//Debug.DrawLine(trans.position, trans.position + (vRot * 5f), Color.yellow);

		rb.MoveRotation(q);

		DBGLookToward += $"v_normal: '{v_nrml_calculated}' trans.up: '{trans.up}' \n" +
			$"dot_facingToPos: '{dot_facingToPos}'\n" +
			$"goal: '{Quaternion.LookRotation(Vector3.RotateTowards(trans.forward, v_toGoal, rotSpeed_passed * Time.fixedDeltaTime, 0.0f), v_nrml_calculated)}'\n" +
			$"fwd: '{trans.forward}', v_toGoal: '{v_toGoal}'\n" +
			$"trans.up: '{trans.up}' dot up: '{Vector3.Dot(normal_passed, trans.up)}'";
	}

	[TextArea(1, 10), SerializeField] protected string travelToDbg;
	/// <summary>
	/// Rotates toward and travels to a supplied position based on supplied rules.
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="distThresh"></param>
	/// <param name="mvSpd"></param>
	/// <param name="rotNrml"></param>
	/// <param name="rotAngThresh"></param>
	/// <param name="rtSpd"></param>
	/// <returns></returns>
	protected void TravelTowards( Vector3 pos, float distThresh, float mvSpd, Vector3 rotNrml, float rotAngThresh, float rtSpd )
	{
		LookToward( pos, rotNrml, rtSpd );

		CalcSpatialValues_pathing();

		if ( dist_toNextPos >= distThresh && dot_facingToNextPos >= rotAngThresh )
		{
			travelToDbg += $"State: Moving...\n";
			rb.MovePosition(trans.position + (mvSpd * Time.fixedDeltaTime * v_toGoal));
			CalcSpatialValues_pathing(); //Need to recalculate after moving.
		}
	}

	#region PATHING ------------------------------------------------------------------------------------
	/// <summary>
	/// Generates a new patrol point and path to that point that the enemy can patrol to.
	/// </summary>
	/// <param name="generateWait">If left true, will generate a time the enemy will wait before it starts patrolling.</param>
	public void GenerateNewPath( bool generateWait = true )
    {
        Log_MethodStart($"GenerateNewPath()");
        int randomIndex = Random.Range(0, patrolPoints_cached.Count);
        Log($"generated randomindex: '{randomIndex}' out of '{patrolPoints_cached.Count}' cached points. This point corresponds to position: '{patrolPoints_cached[randomIndex].V_center}' with raduis: '{patrolPoints_cached[randomIndex].Radius}'");
        if ( randomIndex == index_currentlySelectedPatrolPoint )
        {
            randomIndex = PV_Utilities.GetLoopedIndex( patrolPoints_cached.Count, randomIndex + 1 );
            Log($"decided against this index. looped randomindex to: '{randomIndex}'");
        }
        Log($"original index: '{index_currentlySelectedPatrolPoint}', updating to: '{randomIndex}'...");
        index_currentlySelectedPatrolPoint = randomIndex;
        
        UpdatePath(
            patrolPoints_cached[randomIndex].GetVectorWithin(
                trans.position, myMovementMode == EntityMovementMode.TerrestrialMovement
                ),
            patrolPoints_cached[randomIndex].Radius
            );

        if (generateWait)
        {
            cd_PatrolWait = Random.Range(MyStats_Base_NPC.Duration_PatrolWait_min, MyStats_Base_NPC.Duration_PatrolWait_max);
        }
    }

    public virtual void UpdatePath(Vector3 v_passed, float sampleMaxDistance)
    {
        Log_MethodStart($"UpdatePath()");

    }

    public void CheckPath()
    {
        if ( !MyPath.AmOnCourse(trans.position) )
        {
            Debug.LogWarning($"CheckPath() bug: '{name}' found off course going towards: '{MyPath.CurrentGoal}'. Dumping debug string...");
            print( MyPath.dbgAmOnCourse );
            UpdatePath( MyPath.EndGoal, 0.5f );
        }
    }
	#endregion

	public void ArmDamageCollider()
	{
		PV_Debug.Log("ArmDamageCollider()", PV_LogFormatting.UserMethod);

		myAttackCollider.ArmMe();
	}

	public void DisArmDamageCollider()
	{
		PV_Debug.Log($"{nameof(DisArmDamageCollider)}()", PV_LogFormatting.UserMethod);

		myAttackCollider.DisarmMe();
	}

	/// <summary>
	/// Primarily intended to be called via an animation event.
	/// </summary>
	public virtual void EndOfAttackAnimationCleanup() //todo: just found out this can be interrupted if it's an animation event...
	{
		Log_MethodStart($"{nameof(EndOfAttackAnimationCleanup)}()");

		if (playerScript.MyEntityState == EntityState.Alive)
		{
			SwitchState(EnemyActionState.Engaging);
		}
		myAttackCollider.DisarmMe();
		flag_amInTotallyPreoccupyingAnimation = false;
		myAttackState = AttackState.None;
		rb.constraints = RigidbodyConstraints.None;

	}

	public override void TakeDmg(int amt, float dmgForce, Vector3 damageOriginPosition, RaycastHit rcHit)
	{
		base.TakeDmg( amt, dmgForce, damageOriginPosition, rcHit );
	}

	/// <summary>
	/// Performs final death logic. Set via animation clip event
	/// </summary>
	protected virtual void MakeDead()
    {
        Log_MethodStart("makedead()");
        myEntityState = EntityState.Dead;
        enabled = false;
    }

	public void HandlePlayerDeath()
	{
		PV_Debug.Log( $"HandlePlayerDeath()", PV_LogFormatting.UserMethod, PV_LogDestination.ConsoleAndMomentaryLogger );

		InitState();
		GenerateNewPath();
	}

	#region HELPERS -------------------------------/////////////////////////
	[SerializeField] Transform t_sendGoal;
    [ContextMenu("call SendToGoal()")]
    protected void SendToGoal()
    {
        if (t_sendGoal != null)
        {
            UpdatePath(t_sendGoal.position, 1f);

        }
    }

    public Vector3 vBackwards = Vector3.back;
    [ContextMenu("call SendBackwards()")]
    protected void SendBackwards()
    {
        print($"sending to: '{trans.TransformPoint(vBackwards * 3.5f)}'");
        UpdatePath(trans.TransformPoint(vBackwards * 3.5f), 1f);


    }
	#endregion

	[ContextMenu("z call CheckIfKosher()")]
	/// <summary>
	/// Use this method to log an error alterting yourself if something is potentially not set correctly on Awake().
	/// </summary>
	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

		if (playerScript == null)
		{
			PV_Debug.LogError($"{nameof(Enemy_Bug)}.{nameof(playerScript)} reference was null.");
			amKosher = false;
		}

		if (playerTransform == null)
		{
			PV_Debug.LogError($"{nameof(Enemy_Bug)}.{nameof(playerTransform)} reference was null.");
			amKosher = false;
		}

		if (MyBaseStats == null)
		{
			PV_Debug.LogError($"{nameof(Enemy_Bug)}.{nameof(MyBaseStats)} reference was null.");
			amKosher = false;
		}
		else
		{
			if (MyBaseStats.Distance_vision <= 0)
			{
				PV_Debug.LogError($"Flyer enemy: '{name}'has a Distance_vision of: '{MyBaseStats.Distance_vision}'.");
				amKosher = false;
			}
			if (MyBaseStats.Threshold_VisionRadius < 0 || MyBaseStats.Threshold_VisionRadius > 0.9f)
			{
				PV_Debug.LogError($"Flyer enemy: '{name}'has a Distance_vision of: '{MyBaseStats.Distance_vision}'.");
				amKosher = false;
			}
		}

		if (myPatrolMode == PatrolMode.Stationary && myActionState == EnemyActionState.Patrolling)
		{
			PV_Debug.LogError($"Enemy '{name}', has a Patrol mode of stationary, but an enemystate of patrolling.");
			amKosher = false;
		}

		if (myPatrolMode != PatrolMode.Stationary)
		{
			if (patrolPoints_cached == null || patrolPoints_cached.Count <= 0)
			{
				PV_Debug.LogError($"Enemy: '{name}'has a patrolmode of: '{myPatrolMode}', but not enough patrolpoints setup...");
				amKosher = false;
			}
		}
		else if (myPatrolMode == PatrolMode.Stationary && (patrolPoints_cached == null || patrolPoints_cached.Count > 0))
		{
			PV_Debug.LogError($"Enemy: '{name}'has a patrolmode of: '{myPatrolMode}', but has patrol points cached. Should the patrol mode be changed?");
			amKosher = false;
		}


		if ( Trans_visionOrigin == null )
		{
			PV_Debug.LogError($"{name}.{nameof(Trans_visionOrigin)} reference was null.");
			amKosher = false;
		}

		return amKosher;
	}

}
