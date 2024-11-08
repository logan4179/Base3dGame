using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Enums;
using PV_Utils;
using LogansReverbManagementSystem;
using LogansThirdPersonCamera;
using LogansFootLogicSystem;
using LogansAreaManagementSystem;

/* TODO
 * Would be nice to have something that checks to see if the reload methods (currently called ReloadMe() and InitiateReload() ) are called during 
 *      the reload animation, because if you redo the reload animation, it will erase animation events that you previously set to it.
 */

/// <summary>Base class for the Player. Inherits from base_living.</summary>
public class PV_Player_Members : Base_living
{
    /// <summary>This allows us to keep track of our character's current speed, even works for when the player is falling through air.</summary>
    //protected float currentSpeed = 0f;
    /// <summary>Shows which direction player is moving on current frame compared to the last frame.</summary>
    protected Vector3 Vect_CurrentMovementDirection;
    /// <summary>Measure of how visilble player is from 0 to 1. The higher the number, the closer the player is to max visibility. Calculated in the player script by taking into account player movement, and the Environment's visibility value.</summary>
    //public float CurrentVisibility => currentVisibility;
    /// <summary>Allows variable changing of player visibilty based on the speed he's moving</summary>
    [HideInInspector] public float VisMult_Movement = 0f;
	/// <summary>Allows variable changing of player visibility based on how illuminated or shadowed he is by the lights in his surroundings</summary>
	[HideInInspector] public float VisMult_Illumination = 1f;
	/// <summary>Multiplier that obscures the player when he enters a "cover" collider (like a bush)</summary>
	[HideInInspector] public float VisMult_Cover = 1f;

    [Header("---------------[[ REFERENCE (INTERNAL) ]]-----------------")]
    [SerializeField] protected Stats_player myStats;
    public Stats_player MyStats => myStats;
	protected FootSystem myFootSystem;

	[Header("---------------[[ REFERENCE (EXTERNAL) ]]-----------------")]
    // Guns ---------------------------------------------------------
    /// <summary>Which gun is currently active/drawn. </summary>
    public Base_gun GunScript_equipped = null;
    /// <summary>Convenient reference to the rifle that always exists on the player whether the gameobject is active or not.</summary>
    [SerializeField] protected Gun_fullyAutomaticRifle gunScript_fullyAutomaticRifle;
    /// <summary>Convenient reference to the pistol that always exists on the player whether the gameobject is active or not.</summary>
    [SerializeField] protected Gun_pistol gunScript_pistol;
    /// <summary>This is a collection of all the guns in order for switching.</summary>
    [SerializeField] protected List<Base_gun> gunSequence = null;

    public Light _light_Flashlight;
    [HideInInspector] public Base_InteractiveObject InteractiveObject_focused = null;
    /// <summary>The reverb zone that should be following us if we're in a trigger.</summary>
    protected ReverbFollower reverbFollower_triggering = null;
    /// <summary>The area that we last triggered, and therefore consider active.</summary>
    public LAMS_Area pvArea_triggering = null;
    
    [Space(5f)]

    [Header("TRUTH")]
    [TextArea(1,5)] public string DBG_state;
    [Tooltip("When true, allows the player to be moved via input. When false, prevents player from being moved via input. Can be set with animation event through corresponding method")] 
    protected bool canMove = true;

    protected float currentVisibility;
    public float CurrentVisibility => currentVisibility;

    /// <summary>
    /// This tells you if the draw state of the gun has been succesfully initiated. It doesn't necessarily relate to whether the gun is "fully drawn" and ready 
    /// to fire, which is decided by the gun when a countdown has finished after initiation of the draw state.
    /// </summary>
    public bool GunIsDrawn
    {
        get 
        { 
            return anim.GetBool(paramHash_GunIsDrawn); 
        }
    }

	protected bool flag_amInHandPreoccupyingAnimation;

    protected bool flag_amCurrentlynAttemptingValidInteraction
    {
        get
        {
            return (
                Input.GetKeyDown(PV_Input.Controls.Keycode_Interact_gamepad) ||
                Input.GetKeyDown(PV_Input.Controls.Keycode_Interact)) &&
                InteractiveObject_focused != null && !GunIsDrawn;
		}
    }

	[Header("ALARMS")]
    private float cdTakeDamage = 0f;
    private float cd_sinceLastFootstep;

    //[Header("[----------ANIMATION----------]")]
    protected int animLayer_lowerBody = 0;
    protected int animLayer_upperBody = 1;
	protected AnimatorStateInfo animStateInfo_lowerLayer;
	protected AnimatorStateInfo animStateInfo_upperLayer;

	protected int animID_AmTravelling;
    /// <summary>
    /// This tells the animator to start and maintain the gun drawn state. It doesn't necessarily relate to whether the gun is "fully drawn" and ready 
    /// to fire, which is decided by the gun when a countdown has finished.
    /// </summary>
    protected int paramHash_GunIsDrawn;
    protected int paramHash_TakeDamageTrigger;
    protected int paramHash_ReloadTrigger;
    protected int paramHash_WeaponMode;
    protected int paramHash_EquipWeaponTrigger;
    protected int paramHash_Airborn;
    protected int paramHash_MoveSpeed_straight;
    protected int paramHash_MoveSpeed_side;
    protected int paramHash_Die;

	public static string AnimTag_Idle = "Idling";
	public static int AnimTagHash_Idle;
	public static string AnimTag_Reloading = "Reloading";
	public static int AnimTagHash_Reloading;
	public static string AnimTag_Moving = "Moving";
    public static int AnimTagHash_Moving;
	public static string AnimTag_EquippingWeapon = "EquippingWeapon";
	public static int AnimTagHash_EquippingWeapon;
	public static string AnimTag_TakingDamage = "TakingDamage";
	public static int AnimTagHash_TakingDamage;
	public static string AnimTag_StationaryAiming = "StationaryAiming";
	public static int AnimTagHash_StationaryAiming;

	[Header("[---------- AUDIO ----------]")]
    /// <summary>Index that determines which footstep plays in the list.</summary>
    protected int index_footStep;
    protected List<AudioClip> loadedFootsteps = null;

    [Header("[---------- PLAYER MOVEMENT ----------]")]
    [SerializeField, TextArea(1,5)] protected string DBG_Movement;
	/// <summary>
	/// Used to create the illusion inertia on the player movement. Though this may seem redundant, as it lerps towards 
	/// the input manager's MoveAxisMultiplier, which itself is the product of lerped values, this can be used to add 
	/// inertia to the player movement that won't be present with the lerped axes values in the input manager, keeping those
	/// axes independent for other things they may concievably be used to do, like steer a ship, or aim a turret, for example...
	/// </summary>
	protected float independentMovementInertia;

    [Header("OTHER")]
    protected Vector3 vPositionOnLastFrame = Vector3.zero;

    /// <summary>
    /// Made this method, which performs certain initializations on start so that I could perform these functions on an as-needed basis in the case of 
    /// re-initializing the player like when I use the dev function Respawn().
    /// </summary>
    public override void InitState()
    {
        base.InitState();

		canMove = true;
        _light_Flashlight.enabled = false;

        if (GunScript_equipped != null)
        {
            //First we need to clear out the gunscript_equipped variable, otherwise EquipWeapon() will return early...
            Base_gun gun = GunScript_equipped;
            GunScript_equipped = null;
            //...then we can call EquipWeapon()...
            EquipWeapon(gun);
        }
    }

    protected override void MoveAlarms()
    {
        base.MoveAlarms();
        flag_amInTotallyPreoccupyingAlarm = false;

        if ( cdTakeDamage > 0f)
        {
            cdTakeDamage -= Time.deltaTime;

            if (cdTakeDamage < 0f)
                cdTakeDamage = 0f;
            else
                flag_amInTotallyPreoccupyingAlarm = true;
        }
        /////////////////////////////////////////////////////////
       if ( cd_sinceLastFootstep > 0f )
        {
            cd_sinceLastFootstep -= Time.deltaTime;

            if (cd_sinceLastFootstep <= 0f)
            {
                cd_sinceLastFootstep = 0f;
            }
        }
        /////////////////////////////////////////////////////////
        
        DBG_Alarms = $"{nameof(cdTakeDamage)}: '{cdTakeDamage}'\n" +
            $"{nameof(cd_sinceLastFootstep)}: '{cd_sinceLastFootstep}'\n" +
			$"{nameof(cd_JumpRecoverBuffer)}: '{cd_JumpRecoverBuffer}'\n" +
		    $"{nameof(flag_amInTotallyPreoccupyingAlarm)}: '{flag_amInTotallyPreoccupyingAlarm}'\n" +
			$"";
        
    }

	public override void TakeDmg(int amt, float dmgForce, Vector3 damageOriginPosition, RaycastHit rcHit )
    {
        PV_Debug.LogWithConsoleConditional($"Player.TakeDmg(amt: '{amt}', dmgForce: '{dmgForce}', vDirForce: '{damageOriginPosition}' )", false, PV_LogFormatting.UserMethod);
        base.TakeDmg( amt, dmgForce, damageOriginPosition, rcHit );
        cdTakeDamage = 0.85f;

        if( hp > 0 )
        {
            anim.SetTrigger( paramHash_TakeDamageTrigger );
            flag_amInTotallyPreoccupyingAnimation = true;
        }
        else
        {
            Log($"Player has died.");
            hp = 0;
            myEntityState = EntityState.Dead;
            anim.SetTrigger(paramHash_Die);
            rb.isKinematic = true;
            myCollider.enabled = false;
            MGR_BugEnemy.Instance.HandlePlayerDeath();
        }

        PV_GameManager.Instance.CanvasScript_inGame.updateHP( hp );

        /*PV_Environment.Instance.Pool_bloodDamageEffectsA_red.CycleSpawnAtPosition(
            rcHit.point, Quaternion.FromToRotation(Vector3.up, rcHit.normal)
            );*/

		PV_Environment.Instance.Pool_bloodDamageEffectsA_red.CycleSpawnExact(
	        myCollider.ClosestPoint(damageOriginPosition), Quaternion.FromToRotation(Vector3.up, rcHit.normal)
	        );
	}

    public void Heal( int amt )
    {
        Log_MethodStart( $"Heal('{amt}'). current hp: '{hp}'" );
        hp += amt;
        if( hp > 100 )
        {
            hp = 100;
        }

        PV_GameManager.Instance.CanvasScript_inGame.updateHP(hp);
    }

    /// <summary>
    /// Primarily intended for animation events.
    /// </summary>
    public void SetCanMoveTrue()
    {
        canMove = true;
    }

	public override void SetNotPreoccupied()
	{
		base.SetNotPreoccupied();

        flag_amInHandPreoccupyingAnimation = false;
	}

	public void SetHandsNotPreoccupied() //This is so that I can set this via animation events for an animation that I don't have an alarm for, such as the guard animation.
	{
		PV_Debug.Log($"{name}.{nameof(SetHandsNotPreoccupied)}()", PV_LogFormatting.UserMethod);

		flag_amInHandPreoccupyingAnimation = false;
	}

	public void SayHay()
    {
        print("hay");
    }

    protected void InitiateReload()
    {
        Log_MethodStart($"{gameObject.name}.InitiateReload()");

        anim.SetTrigger( paramHash_ReloadTrigger );
        GunScript_equipped.InitiateReload();
        flag_amInHandPreoccupyingAnimation = true;
    }

    public void SetReloaded() //TODO: need to make this into an animation event to be fired at end of reload animation...
    {
        Log_MethodStart($"{gameObject.name}.SetReloaded()");

        /*string s = $"dbg report: \n";
        foreach (AnimatorClipInfo item in anim.GetCurrentAnimatorClipInfo(0) )
        {
            s += $"\t clip: {item.clip} \n";
        }
        print(s);*/
        //print($"b_Travelling: '{anim.GetBool("b_Travelling")}'. b_DrawGun: '{anim.GetBool(myStats.AcString_GunIsDrawn)}'");

        GunScript_equipped.ReloadMe();
    }

    /// <summary>
    /// Action method to be called when player script attempts to picks up an object. Decided NOT to use the OnPickup event to 
    /// call this because this script (the player script) is what invokes that method anyway.
    /// </summary>
    /// <param name="pickupObject_passed"></param>
    public void AttemptPickup(PickupObject pickupObject_passed)
    {
        Log_MethodStart($"{nameof(AttemptPickup)}");
        print($"player pickup action");
        if ( PV_GameManager.Instance.InventoryScript.CanPickup(pickupObject_passed) )
        {
			PV_GameManager.Instance.InventoryScript.Pickup( pickupObject_passed );

		    if ( pickupObject_passed.MyItemData.GeneralCategory == GeneralItemCategory.ammunition && GunScript_equipped != null )
            {
                Log("updating ammunitiondisplay...");
                GunScript_equipped.UpdateAmmunitionDisplay();
            }
        }
    }

    protected void CycleWeapon( PV_Directions direction_passed )
    {
        PV_Debug.Log( $"CycleWeapon: '{direction_passed}', current index: '{gunSequence.IndexOf(GunScript_equipped)}'", PV_LogFormatting.UserMethod );

        int mySpot = gunSequence.IndexOf(GunScript_equipped);
        int mySign = direction_passed == PV_Directions.right ? 1 : -1;
 
        for ( int i = 1; i < gunSequence.Count; i++ )
        {
            int checkSpot = PV_Utilities.GetLoopedIndex(gunSequence.Count, mySpot + (mySign*i) );

            if ( gunSequence[checkSpot] != GunScript_equipped && gunSequence[checkSpot].gameObject.activeSelf )
            {
                EquipWeapon( gunSequence[checkSpot] );
                print($"CycleWeapon() Found new gun: '{GunScript_equipped}'");
                return;
            }
        }
    }

    /// <summary>
    /// Unequips current weapon and equips the passed weapon.
    /// </summary>
    /// <param name="gun_passed"></param>
    protected void EquipWeapon( Base_gun gun_passed )
    {
        Log_MethodStart($"EquipWeapon('{gun_passed}')");
        if( GunScript_equipped == gun_passed )
        {
            LogWarning($"EquipWeapon() was passed a gun ('{gun_passed}' that appears to already be equipped! Returning early...)");
            return;
        }

        if( GunScript_equipped != null )
        {
            GunScript_equipped.UnequipMe();
        }

        GunScript_equipped = gun_passed;
        GunScript_equipped.EquipMe();
        anim.SetInteger( paramHash_WeaponMode, GunScript_equipped.MyAnimatorWeaponMode );
        anim.SetTrigger( paramHash_EquipWeaponTrigger );
        flag_amInHandPreoccupyingAnimation = true;
    }

    public void CreateRecoil( Vector3 v_recoil )
    {
        //print($"player.CreateRecoil('{v_recoil}')");
        trans.Rotate( Vector3.up, v_recoil.x * 20f ); //I bias the horizontal kick bc if I don't, it's too subtle...
		PV_GameManager.Instance.M_ThirdPersonCamera.CreateRecoil(v_recoil);
    }

    protected void ConstrainWeaponToHand()
    {
        GunScript_equipped.ConstrainToHand();
    }

    protected void ConstrainWeaponToHolster()
    {
        GunScript_equipped.ConstrainToHolster();
    }

    public void TryLoadFootstep( string tagString_passed )
    {
        PV_Debug.Log( $"TryLoadFootstep('{tagString_passed}')", PV_LogFormatting.Standard );

        if ( tagString_passed == PV_Environment.Instance.MyStats.Tag_ConcreteGround && (loadedFootsteps == null || (loadedFootsteps != null && loadedFootsteps != myStats.AudioClips_Footstep_ConcreteA)) )
        {
            loadedFootsteps = myStats.AudioClips_Footstep_ConcreteA;
            index_footStep = 0;
            PV_Debug.Log( $"Concrete steps loaded", PV_LogFormatting.Standard );
        }
    }

    public void PlayFootstep() // currently called via animation event.
    {
        if( cd_sinceLastFootstep > 0f )
        {
            return;
        }

        if( loadedFootsteps != null )
        {
            AudioSource.PlayClipAtPoint( loadedFootsteps[index_footStep], trans.position, independentMovementInertia );
            index_footStep = PV_Utilities.GetLoopedIndex( loadedFootsteps.Count, index_footStep + 1 );
            cd_sinceLastFootstep = 0.2f;
            PV_AudioManager.EmulateEnvironmentalSound( trans.position, 50f * independentMovementInertia );
        }
        else
        {
            PV_Debug.LogError($"PV ERROR! PlayFootstep() couldn't run because the loadedFootsteps list is null. Returning early...");
        }
        //AddStepForce();
    }

    #region DEBUG HELPERS-----------------------------------------
    public float dbgBlastForce = 100f;
    [ContextMenu("z call BlastMe()")]
    public void BlastMe() //for testing sudden physics movements.
    {
        rb.AddForce(Vector3.right * dbgBlastForce, ForceMode.Impulse);
    }

    [ContextMenu("z ForceDeath()")]
    public void ForceDeath()
    {
        TakeDmg( 110, 0f, Vector3.zero, new RaycastHit() );
    }

    public void Respawn()
    {
        hp = 100;
        PV_GameManager.Instance.CanvasScript_inGame.updateHP(hp);

        anim.Rebind();
        anim.Update(0f);
        InitState();
    }

    [ContextMenu("z SayHP()")]
    public void SayHP()
    {
        print(hp);
    }

    #endregion
}
