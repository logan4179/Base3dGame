using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Enums;
using LogansReverbManagementSystem;
using LogansFootLogicSystem;
using System.Threading;
using LogansAreaManagementSystem;

public class PV_Player : PV_Player_Members
{
    protected override void Awake()
    {
        base.Awake();

        // Register Me-----------------------------------------
        PV_GameManager.Instance.PlayerScript = this;
        PV_GameManager.Instance.Trans_Player = trans;
        PV_GameManager.Instance.Trans_PlayerPerspective = trans_perspective;
        _light_Flashlight = trans_perspective.Find("Flashlight").GetComponent<Light>();
        myFootSystem = trans.Find("FootSphere").GetComponent<FootSystem>();
        myCollider = trans.Find("Capsule").GetComponent<CapsuleCollider>();
    }

    //[TextArea(1,6)] public string DBG_AnimHashes;
    protected override void Start()
    {
        base.Start();

        vPositionOnLastFrame = trans.position;

        if( myStats == null )
        {
            Debug.LogWarning( $"VERTIGO WARNING! Stats object not found for gun: '{name}'!" );
        }

        #region Get animation hash values --------------------------
        AnimTagHash_Idle = Animator.StringToHash( AnimTag_Idle );
        AnimTagHash_Reloading = Animator.StringToHash( AnimTag_Reloading );
		AnimTagHash_Moving = Animator.StringToHash( AnimTag_Moving );
		AnimTagHash_EquippingWeapon = Animator.StringToHash( AnimTag_EquippingWeapon );
		AnimTagHash_TakingDamage = Animator.StringToHash( AnimTag_TakingDamage );
		AnimTagHash_StationaryAiming = Animator.StringToHash( AnimTag_StationaryAiming );

		paramHash_GunIsDrawn = Animator.StringToHash(myStats.AcString_GunIsDrawn);
		paramHash_TakeDamageTrigger = Animator.StringToHash(myStats.AcString_TakeDamageTrigger);
		paramHash_ReloadTrigger = Animator.StringToHash(myStats.AcString_ReloadTrigger);
		paramHash_WeaponMode = Animator.StringToHash(myStats.AcString_WeaponMode);
		paramHash_EquipWeaponTrigger = Animator.StringToHash(myStats.AcString_EquipWeaponTrigger);
		paramHash_Airborn = Animator.StringToHash(myStats.AcString_Airborn);
		animID_AmTravelling = Animator.StringToHash(myStats.AcString_Travelling);
		paramHash_MoveSpeed_straight = Animator.StringToHash(myStats.AcString_MoveSpeed_straight);
		paramHash_MoveSpeed_side = Animator.StringToHash(myStats.AcString_MoveSpeed_side);
		paramHash_Die = Animator.StringToHash(myStats.AcString_Die);
        #endregion

        //DBG_AnimHashes = $"";

		// EVENTS ---------------------------------------------
        PV_GameManager.Event_OnHorizontalDpadPressed.AddListener( CycleWeapon );
		myFootSystem.Init(trans, trans_perspective, rb, PV_Environment.Instance.Mask_WalkableJumpable);

		InitState();
        CheckIfKosher();
    }


    void Update()
    {
		if ( !amPerformingLoopLogic )
		{
			return;
		}

		MoveAlarms();

        animStateInfo_lowerLayer = anim.GetCurrentAnimatorStateInfo(animLayer_lowerBody);
		animStateInfo_upperLayer = anim.GetCurrentAnimatorStateInfo(animLayer_upperBody);
        flag_amInHandPreoccupyingAnimation = (
            animStateInfo_upperLayer.tagHash == AnimTagHash_Reloading || 
            animStateInfo_upperLayer.tagHash == AnimTagHash_EquippingWeapon
            );
        flag_amInTotallyPreoccupyingAnimation = (animStateInfo_lowerLayer.tagHash == AnimTagHash_TakingDamage);

        if( AmTotallyPreoccupied || !PV_Input.Instance.AmReadingInput )
        {
            return;
        }

        if ( GunScript_equipped != null )
        {
            // DRAW/AIM WEAPON -----------------------
            if ( PV_Input.AmHoldingAimInput && GunScript_equipped.CanDraw ) 
            {
                anim.SetBool( paramHash_GunIsDrawn, true );
                GunScript_equipped.BeginDraw();
            }
            else if ( !PV_Input.AmHoldingAimInput && GunIsDrawn )
            {
                anim.SetBool( paramHash_GunIsDrawn, false);
                GunScript_equipped.SetMeToUndrawn();
            }

            if ( GunScript_equipped.AmFullyDrawn )
            {
                //Laser effect
                /*_scr_Gun_current._lr_MuzzlePos.enabled = true;
                _scr_Gun_current._lr_MuzzlePos.SetPosition(0, _scr_Gun_current._lr_MuzzlePos.transform.position );
                _scr_Gun_current._lr_MuzzlePos.SetPosition( 1, (_scr_Gun_current._lr_MuzzlePos.transform.position + _scr_Gun_current._lr_MuzzlePos.transform.forward*distance_laserSight) );
                */

                // FIRE WEAPON ----------------
                //if ( ((Input.GetKeyDown(KeyCode.Mouse0) && Input.GetAxisRaw("RightTrigger") == 0f) || (Input.GetAxisRaw("RightTrigger") != 0f && rightTriggerRawPrev == 0)) && GunScript_equipped.CanPullTrigger)
                if ( PV_Input.AmPressingTriggerInput && GunScript_equipped.CanPullTrigger) //TODO: I was originally using rightTriggerRawPrev in this if check to make sure that I was pulling the trigger for the first time I suppose. Not sure if I need it.
                {
                    //print("!");
                    if ( GunScript_equipped.HasAmmunitionInClip )
                    {
                        GunScript_equipped.FireBullet();
                        //print($"fired: '{Input.GetAxis("RightTrigger")}', rawaxis: '{Input.GetAxisRaw("RightTrigger")}', prev: '{rightTriggerRawPrev}'");

                    }
                    else
                    {
                        //print($"clicked axis: '{Input.GetAxis("RightTrigger")}', rawaxis: '{Input.GetAxisRaw("RightTrigger")}', prev: '{rightTriggerRawPrev}'");

                        GunScript_equipped.Click();
                    }
                }
            }

            // RELOAD WEAPON ----------------
            if ( PV_Input.ReloadIsBeingPressed && !flag_amInHandPreoccupyingAnimation && GunScript_equipped.CanReload )
            {
                InitiateReload();

            }

            // SWITCH FIRE MODE ---------------
            if ( Input.GetKeyDown(PV_Input.Controls.Keycode_SwitchFireMode) || Input.GetKeyDown(PV_Input.Controls.Keycode_SwitchFireMode_gamepad) )
            {
                GunScript_equipped.TryCycleFireMode();
            }
        }

        // FLASHLIGHT TOGGLE -----------
        if ( (Input.GetKeyDown(PV_Input.Controls.Keycode_FlashlightToggle_gamepad) || Input.GetKeyDown(PV_Input.Controls.Keycode_FlashlightToggle)) && !flag_amInHandPreoccupyingAnimation )
        {
            _light_Flashlight.enabled = !_light_Flashlight.enabled;
        }

        #region INTERACT ---------------------
        if ( (Input.GetKeyDown(PV_Input.Controls.Keycode_Interact_gamepad) || Input.GetKeyDown(PV_Input.Controls.Keycode_Interact)) && InteractiveObject_focused != null && !GunIsDrawn )
        {
            if ( InteractiveObject_focused.MyType == InteractiveObjectType.Prompt )
            {
                PV_Debug.Log("Player Initiating interaction with a prompt object...");
                PromptObject promptOb = InteractiveObject_focused.gameObject.GetComponent<PromptObject>();

                if ( promptOb.Index_PromptMessages == -1 ) //First interaction...
                {
                    PV_GameManager.Event_OnPromptEntered.Invoke(promptOb);
                }
                else
                {
                    if ( promptOb.Index_PromptMessages < promptOb.PromptMessages.Count )
                    {
                        PV_GameManager.Event_OnPromptReadNext.Invoke(promptOb);
                    }
                    else
                    {
                        PV_GameManager.Event_OnPromptExited.Invoke(promptOb);
                    }
                }
            }
            else if ( InteractiveObject_focused.MyType == InteractiveObjectType.ItemPickup )
            {
                PV_Debug.Log( "Player Initiating interaction with a pickup object..." );

                PickupObject pickupOb = InteractiveObject_focused.gameObject.GetComponent<PickupObject>();

                AttemptPickup( pickupOb );
            }
            else if ( InteractiveObject_focused.MyType == InteractiveObjectType.StandardDoor )
            {
                Door door = InteractiveObject_focused.gameObject.GetComponent<Door>();
                door.InteractAction();
            }
        }
        #endregion

        // Keyboard weapon select---------------------------
        if ( Input.GetKeyDown(PV_Input.Controls.Keycode_FirstWeapon) && gunScript_pistol.gameObject.activeSelf && !flag_amInHandPreoccupyingAnimation )
        {
            EquipWeapon(gunScript_pistol);
        }
        else if ( Input.GetKeyDown(PV_Input.Controls.Keycode_SecondWeapon) && gunScript_fullyAutomaticRifle.gameObject.activeSelf && !flag_amInHandPreoccupyingAnimation )
        {
			EquipWeapon( gunScript_fullyAutomaticRifle);
        }
	}

	[SerializeField, Space(5f), TextArea(1, 10)] public string DBG_Visibility;

    private void LateUpdate()
    {
        if (!amPerformingLoopLogic)
        {
            return;
        }

        anim.SetBool( paramHash_Airborn, myFootSystem.MyFootState != LFLS_FootState.Grounded );

        if( Time.deltaTime > 0f ) //doing this if-check to avoid a NaN error that gets thrown because for some reason, it still thinks the manager's gamestate is 'unpaused' after hitting escape, while tine.timescale is considered 0, causing a divide by zero problem.
		{
		    //VisMult_Cover = Mathf.Clamp( 1f - (0.04f * totalCoverTriggering_Lvl1) - (0.2f * totalCoversTriggering_Lvl2), 0.01f, 1f ); //TODO: how do I do this now that I've taken away a lot of these values?
		    CurrentSpeed = (Vector3.Distance(trans.position, vPositionOnLastFrame) / Time.deltaTime);
		    VisMult_Movement = 1f + (CurrentSpeed / 20f);
		    float visibilityLerpGoal = Mathf.Clamp( 
                PV_Environment.Instance.MyStats.GeneralEnvironmentalVisibility * VisMult_Movement * VisMult_Illumination * VisMult_Cover, 0.05f, 1f
                );
            float prevVisibility = currentVisibility;
            currentVisibility = Mathf.Lerp(CurrentVisibility, visibilityLerpGoal, myStats.visLerpSpeed * Time.deltaTime);

		    DBG_Visibility = $"CurrentSpeed: '{CurrentSpeed.ToString("#.##")}'\n" +
                $"VisMult_Movement: '{VisMult_Movement.ToString("#.##")}'\n" +
		        $"{nameof(visibilityLerpGoal)}: '{visibilityLerpGoal.ToString("#.##")}'\n" +
			    $"{nameof(currentVisibility)}: '{currentVisibility}', {nameof(prevVisibility)}: '{prevVisibility}'\n" +
                $"";
        }

		#region MOVEMENT --------------------//////////////////////////
		DBG_Movement = "";
		//This line needs to be outside of the following if check so that it correctly goes back to zero when the control is not in use...
		independentMovementInertia = Mathf.Lerp(
			independentMovementInertia, PV_Input.MoveAxisMagnitude, myStats.LerpSpeed_IndependentMoveInertia * Time.deltaTime
			);

		bool runCondition = !anim.GetBool(myStats.AcString_GunIsDrawn) && !GunScript_equipped.AmReloading;

		if (myFootSystem.MyFootState == LFLS_FootState.Grounded && PV_Input.V_LerpedMoveAxes_calculated.magnitude > 0.05f && canMove)
		{

			anim.SetBool(animID_AmTravelling, true);

			anim.SetFloat(
				paramHash_MoveSpeed_straight, PV_Input.Val_Axis_vertical_lerped * (runCondition ? 1f : 0.1f) * independentMovementInertia
				);
			anim.SetFloat(
				paramHash_MoveSpeed_side, PV_Input.Val_Axis_horizontal_lerped * (runCondition ? 1f : 0.1f) * independentMovementInertia
				);
		}
		else
		{
			anim.SetBool(animID_AmTravelling, false);

			anim.SetFloat(paramHash_MoveSpeed_straight, 0f);
			anim.SetFloat(paramHash_MoveSpeed_side, 0f);
		}

		myFootSystem.UpdateValues(
			(runCondition ? myStats.moveSpeed_run : myStats.MoveSpeed_walk),
			PV_Input.Val_Axis_vertical_lerped,
			PV_Input.Val_Axis_horizontal_lerped,
			-PV_Input.Val_Axis_RHorizontal_lerped * PV_Input.LookSensitivity
			);

		Vect_CurrentMovementDirection = trans.position - vPositionOnLastFrame;
		vPositionOnLastFrame = trans.position;
		#endregion
		DBG_Movement = $"{nameof(independentMovementInertia)}: '{independentMovementInertia}'\n" +
	        $"{nameof(runCondition)}: '{runCondition}'\n";

		DBG_state = $"{nameof(AmTotallyPreoccupied)}: '{AmTotallyPreoccupied}'\n" +
			$"{nameof(flag_amInTotallyPreoccupyingAlarm)}: '{flag_amInTotallyPreoccupyingAlarm}'\n" +
			$"{nameof(flag_amInTotallyPreoccupyingAnimation)}: '{flag_amInTotallyPreoccupyingAnimation}'\n" +
			$"{nameof(flag_amInHandPreoccupyingAnimation)}: '{flag_amInHandPreoccupyingAnimation}'\n" +
			$"";

		if ( reverbFollower_triggering != null )
        {
            reverbFollower_triggering.UpdateMe( trans.position + Vector3.up * height );
        }

		if ( PV_Input.AmHoldingAimInput )
		{
			if ( GunScript_equipped.AmFullyDrawn && PV_GameManager.Instance.M_ThirdPersonCamera.CurrentConfigIndex != 1 )
			{
				PV_GameManager.Instance.M_ThirdPersonCamera.ChangeConfiguration( 1 );
			}
		}
		else if ( PV_GameManager.Instance.M_ThirdPersonCamera.CurrentConfigIndex != 0 )
		{
			PV_GameManager.Instance.M_ThirdPersonCamera.ChangeConfiguration( 0 );
		}

		PV_GameManager.Instance.M_ThirdPersonCamera.UpdateCamera(
			PV_Input.Val_Axis_RHorizontal_lerped * PV_Input.LookSensitivity,
			-PV_Input.Val_Axis_RVertical_lerped * PV_Input.LookSensitivity, Time.deltaTime
			);
	}

    private void OnTriggerEnter(Collider other)
    {
        string colTag = other.tag;

        if ( other.CompareTag(PV_GameManager.Tag_ReverbFollower) )
        {
            PV_Debug.Log( $"ReverbFollower OnTriggerEnter with player, ob: '{other.name}'", PV_LogFormatting.Standard );
            if ( !other.TryGetComponent(out reverbFollower_triggering) )
            {
                reverbFollower_triggering = other.transform.parent.GetComponent<ReverbFollower>();
            }
        }
        else if( other.CompareTag(PV_GameManager.Tag_area) )
        {
            LogHistoric( $"triggered area: '{other.name}'" );
            LAMS_Area area;
            if( !other.TryGetComponent(out area) )
            {
                PV_Debug.LogError($"PV ERROR! Player triggered something with a tag of 'PV_Area', name: '{other.name}, but couldn't get the PV_Area script component.");
            }
            else
            {
                if( pvArea_triggering == null || pvArea_triggering != area )
                {
                    pvArea_triggering = area;
                    LAMS_Manager.Instance.ChangeActiveArea( area );
                }
            }
        }
    }

    public string DBG_Trigger;
    private void OnTriggerStay(Collider other )
    {
        string colTag = other.tag;  
        float colRadius, colDistance;

        if( colTag == "illuminationCollider" )
        {
            colRadius = other.gameObject.GetComponent<SphereCollider>().radius;
            colDistance = Vector3.Distance(trans.position, other.gameObject.transform.position);

            VisMult_Illumination = 1f + (0.3f * (colRadius / colDistance));
        }
        else if( colTag == "shadowCollider")
        {
            VisMult_Illumination = 0.5f;
            
        }

        DBG_Trigger = colTag;
    }

    private void OnTriggerExit(Collider other)
    {
        if ( other.CompareTag(PV_GameManager.Tag_ReverbFollower) )
        {
            PV_Debug.Log( "ReverbFollower OnTriggerExit with player", PV_LogFormatting.Standard );
            reverbFollower_triggering = null;
        }

        DBG_Trigger = "";
    }

    private void OnCollisionEnter(Collision collision)
    {
        PV_Debug.Log($"collided with: '{collision.transform.gameObject.name}'", PV_LogFormatting.Standard );

        if( collision.transform.gameObject.layer == PV_Environment.Instance.Layer_Solid_Environmental )
        {
            TryLoadFootstep(collision.transform.tag);
        }


    }

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

        if( myCollider == null )
        {
			PV_Debug.LogError($"{name}.{nameof(myCollider)} reference was null.");
			amKosher = false;
		}

		if ( myFootSystem == null )
		{
			PV_Debug.LogError( $"{name}.{nameof(myFootSystem)} reference was null." );
			amKosher = false;
		}

		if ( trans_perspective == null )
		{
			PV_Debug.LogError( $"{name}.{nameof(trans_perspective)} reference was null." );
			amKosher = false;
		}

		return amKosher;
	}
}