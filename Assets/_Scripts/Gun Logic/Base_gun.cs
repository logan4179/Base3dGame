using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using PV_Enums;
using PV_Events;

public abstract class Base_gun : PV_Object
{
    [SerializeField] public Stats_gun MyStats;
    [SerializeField] public ItemData MyItemData;

    [Space(10)]

	//[Header("[----REFERENCE (INTERNAL)----]")]
    protected Animator anim;

	//[Header("[----REFERENCE (EXTERNAL)----]")]
	/// <summary>Empty transform object placed on the muzzle area of the gun for positional reference of the muzzle.</summary>
	protected Transform trans_MuzzlePos;
	/// <summary>This is an actual light object attatched to the gun so that the environment can get illuminated briefly with gunshots.</summary>
	protected Light light_MuzzleFlash;
    protected VisualEffect vfx_MuzzleFlash;
    protected LineRenderer lineRenderer_muzzlePos;
    protected PV_Player playerScript;
    protected Animator anim_player;

    /// <summary>Keeps track of how many shots have been fired in current burst if firemode is threeroundburst.</summary>
    protected int burstCount;
    [Space(10)]

    //[Header("[----------ANIMATION----------]")]
    protected int animID_WeaponDrawn;
    protected int animID_Recoil;

    [Header("TRUTH")]
    /// <summary>If 3 round burst is an option on this gun. Allows selectively allowing this FireMode upon picking up an upgrade</summary>
    protected bool canDoThreeRoundBurst;
    /// <summary>If full auto is an option on this gun. Allows selectively allowing this FireMode upon picking up an upgrade</summary>
    protected bool canDoFullAuto;

    public GunFireMode MyFireMode;

    /// <summary>Determines if gun is currently in the reload animation. Gets turned back to false at the end of the reload animation.</summary>
    public bool AmReloading { get; set; }

    /// <summary>Is true when the gun has fully completed the draw animation and can theoretically be fired. This property does NOT check other, unrelated 
    /// things necessary for firing, such as whether the gun is in the reload state, or if there is ammunition. Those things are checked via separate properties.</summary>
    public bool AmFullyDrawn 
    { 
        get
        {
            return ( PV_Input.AmHoldingAimInput && playerScript.GunIsDrawn && cd_DrawGun <= 0f );
        }
    }

    public bool CanDraw
    {
        get
        {
            return ( !AmFullyDrawn && cd_DrawGun <= 0f && !anim_player.GetBool(playerScript.MyStats.AcString_GunIsDrawn) && !AmReloading );
        }
    }
    public bool HasAmmunitionInClip
    {
        get
        {
            return (count_ammunitionCurrentlyInClip > 0);
        }
    }

    /// <summary>Tells if the gun can even listen for trigger input from the player.</summary>
    public bool CanPullTrigger
    {
        get
        {
            //if( playerScript.anim.GetBool(playerScript.MyStats.AcString_GunIsDrawn) && !AmReloading ) //trying next line instead. Dws
            if ( AmFullyDrawn && !AmReloading)
            {
                if ( MyFireMode == GunFireMode.SemiAutomatic || MyFireMode == GunFireMode.FullyAutomatic )
                {
                    return true;
                }
                else if ( MyFireMode == GunFireMode.ThreeRoundBurst && burstCount == 0 )
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool CanReload
    {
        get 
        { 
            return ( PV_GameManager.Instance.InventoryScript.ExistsInInventory(MyStats.itemName_myAmmo) && count_ammunitionCurrentlyInClip < MyStats.count_singleClipCapacity && !AmReloading );
        }
    }



    [Header("ALARMS")]
    [SerializeField] protected string debugAlarms;
    protected float cd_FireFullAuto;
    protected float cd_Light_MuzzleFlash, cd_Burst, cd_DrawGun = 0f;

    [Header("OTHER")]
    [SerializeField, TextArea(1,5)] protected string debugState = "";
    /// <summary>Keeps track of how many bullets are currently in the clip.</summary>
    protected int count_ammunitionCurrentlyInClip;
    [SerializeField] protected int animatorWeaponMode;
    /// <summary>Corresponds to the animation mode parameter in the player's animator that allows the animation controller to determine
    /// which weapon is equipped. </summary>
    public int MyAnimatorWeaponMode => animatorWeaponMode;
    /// <summary>Calculated from 1f/MyStats.FireRate so that this division doesn't have to be done every time a bullet fires.</summary>
    protected float fireRate_calculated;

    [Header("DEBUG")]
	[SerializeField] protected bool amDebuggingScript;
	[SerializeField, Tooltip("Makes the script generate the debug strings. Costs performance, but allows you to diagnose issues.")] 
    protected bool doDebugStrings = false;

	protected virtual void Awake()
	{
        anim = GetComponent<Animator>();

        trans_MuzzlePos = transform.Find("MuzzlePos");
		light_MuzzleFlash = trans_MuzzlePos.Find("Light_MuzzleFlash").GetComponent<Light>();
		vfx_MuzzleFlash = trans_MuzzlePos.Find("fx_MuzzleFlash/vfxGraph_MuzzleFlash").GetComponent<VisualEffect>();
        lineRenderer_muzzlePos = trans_MuzzlePos.GetComponent<LineRenderer>();

        playerScript = PV_GameManager.Instance.PlayerScript;
        anim_player = playerScript.GetComponent<Animator>();
	}

	protected void BaseInit()
    {
        Log("BaseInit() start");
        light_MuzzleFlash.enabled = false;
        lineRenderer_muzzlePos.enabled = false;

        animID_WeaponDrawn = Animator.StringToHash( MyStats.AcString_WeaponDrawn );
        animID_Recoil = Animator.StringToHash( MyStats.AcString_Recoil );
        Log("BaseInit() end");

    }

    public virtual void FireBullet()
    {
        vfx_MuzzleFlash.Play();
        light_MuzzleFlash.enabled = true;
        cd_Light_MuzzleFlash = MyStats.duration_Light_MuzzleFlash_On;

        RaycastHit rcHit_Bullet;
        Ray ray_bullet = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        if (amDebuggingScript) Debug.DrawRay(ray_bullet.origin, ray_bullet.direction * MyStats.dist_BulletRaycast, Color.white, 1f);

        if ( Physics.Raycast(ray_bullet, out rcHit_Bullet, MyStats.dist_BulletRaycast, PV_Environment.Instance.Mask_Shootable) )
        {
            int lyr = rcHit_Bullet.transform.gameObject.layer;

            if ( lyr == PV_Environment.Instance.Layer_Solid_Environmental )
            {
				GameObject go_gunshot = PV_Environment.Instance.Pool_gunshotDecalsA.CycleSpawn( rcHit_Bullet.point, -rcHit_Bullet.normal );
				PV_Environment.Instance.Pool_bulletImpact_concrete_A.CycleSpawn( rcHit_Bullet.point, Vector3.right );

			}
			else if( lyr == PV_Environment.Instance.Layer_Living )
            {
                Log("Fired bullet and hit living", amDebuggingScript ? PV_LogDestination.Everywhere : PV_LogDestination.Hidden );

                Base_living living_hit = rcHit_Bullet.transform.gameObject.GetComponent<Base_living>();

                if ( living_hit.Flag_amEnemy )
                {
                    living_hit.TakeDmg (MyStats.Amount_Damage, MyStats.Force_Damage, rcHit_Bullet.point, rcHit_Bullet );
				}
            }
        }
        AudioSource.PlayClipAtPoint(MyStats.audioClip_fireBullet, trans_MuzzlePos.position);
        PV_AudioManager.EmulateEnvironmentalSound( trans_MuzzlePos.position, 70f );
        count_ammunitionCurrentlyInClip--;
        UpdateAmmunitionDisplay();

        int hSign = Random.Range(0, 2);
        Vector3 vRecoil = new Vector3( 
            Random.Range(0.1f, 0.5f) * (hSign == 1 ? 1 : -1),
            Random.Range(0.2f, 0.3f),
            0f
        );
        anim.SetTrigger( animID_Recoil );
        playerScript.CreateRecoil( vRecoil.normalized * MyStats.Amount_Recoil );
    }

    public void Click()
    {
        PV_Debug.LogWithConsoleConditional( $"Click()", amDebuggingScript, PV_LogFormatting.UserMethod );

        AudioSource.PlayClipAtPoint(MyStats.audioClip_click, trans_MuzzlePos.position);
    }

    /// <summary>
    /// Gets called indirectly via player script's animation event at the start of the animation.
    /// </summary>
    public virtual void InitiateReload()
    {
		Log_MethodStart($"Base.InitiateReload(), '{anim_player.GetCurrentAnimatorClipInfo(1)[0].clip.name}'",amDebuggingScript ? PV_LogDestination.Everywhere : PV_LogDestination.Hidden, PV_LogFormatting.UserMethod );
		AudioSource.PlayClipAtPoint(MyStats.audioClip_reload, trans_MuzzlePos.position);
		AmReloading = true;
	}

    /// <summary>
    /// Gets called indirectly via player script's animation event. This gets called at the end of the reload animation and actually does the reloading.
    /// </summary>
    public void ReloadMe()
    {
		Log_MethodStart($"Base.ReloadMe()", PV_LogDestination.Hidden, PV_LogFormatting.UserMethod);
        count_ammunitionCurrentlyInClip += PV_GameManager.Instance.InventoryScript.ConsumeItemByAmount(MyStats.itemName_myAmmo, MyStats.count_singleClipCapacity - count_ammunitionCurrentlyInClip);
        UpdateAmmunitionDisplay();
        AmReloading = false;
    }

    public void UpdateAmmunitionDisplay()
    {
		Log_MethodStart($"UpdateAmmunitionDisplay(totalAmmunition_inClip: '{count_ammunitionCurrentlyInClip}')", PV_LogDestination.Hidden, PV_LogFormatting.UserMethod);
        PV_GameManager.Instance.CanvasScript_inGame._txt_Ammunition.text = $"<color=red>{count_ammunitionCurrentlyInClip}/{PV_GameManager.Instance.InventoryScript.TotalAmountInInventory(MyStats.itemName_myAmmo)}</color>";

        Log($"end. totalAmmunition_inClip: '{count_ammunitionCurrentlyInClip}'. txt: '{PV_GameManager.Instance.CanvasScript_inGame._txt_Ammunition.text}'" );
    }
    /// <summary>Should be called whenever the player selects/equips this gun in-game.</summary>
    public void EquipMe()
    {
		Log_MethodStart( $"Base.EquipMe('{name}'). Finding existing slot with my ammo with itemName of: '{MyStats.itemName_myAmmo}'", PV_LogDestination.Hidden, PV_LogFormatting.UserMethod );
        playerScript.GunScript_equipped = this;
        UpdateAmmunitionDisplay();
        PV_GameManager.Instance.CanvasScript_inGame.SetFireModeGraphic(MyFireMode);
        PV_GameManager.Event_OnGunChanged.Invoke(this);
    }

    public abstract void ConstrainToHand();


    /// <summary>This starts the draw. SetMeToDrawn() then gets called during the draw weapon alarm end.</summary>
    public virtual void BeginDraw()
    {
        Log_MethodStart( "Base.BeginDraw()" );

        cd_DrawGun = MyStats.duration_cd_DrawGun;
    }

    public abstract void SetMeToDrawn();

    public void UnequipMe()
    {
		Log_MethodStart( $"Base.UnequipMe('{name}')" );

        PV_GameManager.Instance.CanvasScript_inGame._txt_Ammunition.text = $"";
        ConstrainToHolster();
    }
    public abstract void ConstrainToHolster();


    public abstract void SetMeToUndrawn();

    /// <summary>
    /// Attempts to cycle the firemode of the gun. Doesn't do anything if the 'canDo...' booleans don't permit changing the firemode.
    /// </summary>
    public void TryCycleFireMode()
    {
        Log_MethodStart($"CycleFireMode()");

        GunFireMode startingMode = MyFireMode;
        if (MyFireMode == GunFireMode.SemiAutomatic)
        {
            if (canDoThreeRoundBurst)
            {
                MyFireMode = GunFireMode.ThreeRoundBurst;
            }
            else if (canDoFullAuto)
            {
                MyFireMode = GunFireMode.FullyAutomatic;
            }
        }
        else if (MyFireMode == GunFireMode.ThreeRoundBurst)
        {
            if (canDoFullAuto)
            {
                MyFireMode = GunFireMode.FullyAutomatic;
            }
            else
            {
                MyFireMode = GunFireMode.SemiAutomatic;
            }
        }
        else if (MyFireMode == GunFireMode.FullyAutomatic)
        {
            MyFireMode = GunFireMode.SemiAutomatic;
        }

        if (MyFireMode != startingMode)
        {
            AudioSource.PlayClipAtPoint(MyStats.audioClip_switchFireMode, trans_MuzzlePos.position);
            PV_GameManager.Instance.CanvasScript_inGame.SetFireModeGraphic( MyFireMode );
        }
        else
        {
            print($"No 'canDo...' variables enabled currently on this gun.  Can't cycle fire mode...");
        }

        Log($"CycleFireMode() end. firemode now set to: '{MyFireMode}'");
    }

    protected void OnCycleFireMode_action(Base_gun gunScript_passed) //TODO: Is this necessary, or even being used? Couldn't I just rely on CycleFireMode()?
    {
        Log_MethodStart($"Base.OnCycleFireMode_action()", PV_LogDestination.Hidden, PV_LogFormatting.UserMethod);
        AudioSource.PlayClipAtPoint(MyStats.audioClip_switchFireMode, trans_MuzzlePos.position);
        Log($"fire mode changed to: '{MyFireMode}'");
    }

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

		if ( MyStats == null )
        {
            PV_Debug.LogError( $"{name}.{nameof(MyStats)} reference was null." );
            amKosher = false;
        }

		if ( MyItemData == null )
		{
			PV_Debug.LogError($"{name}.{nameof(MyItemData)} reference was null.");
			amKosher = false;
		}

		if ( anim == null )
		{
			PV_Debug.LogError($"{name}.{nameof(anim)} reference was null.");
			amKosher = false;
		}

		if ( trans_MuzzlePos == null )
		{
			PV_Debug.LogError($"{name}.{nameof(trans_MuzzlePos)} reference was null.");
			amKosher = false;
		}

		if ( light_MuzzleFlash == null )
		{
			PV_Debug.LogError($"{name}.{nameof(light_MuzzleFlash)} reference was null.");
			amKosher = false;
		}

		if ( vfx_MuzzleFlash == null )
		{
			PV_Debug.LogError($"{name}.{nameof(vfx_MuzzleFlash)} reference was null.");
			amKosher = false;
		}

		if ( lineRenderer_muzzlePos == null )
		{
			PV_Debug.LogError($"{name}.{nameof(lineRenderer_muzzlePos)} reference was null.");
			amKosher = false;
		}

		if ( playerScript == null )
		{
			PV_Debug.LogError($"{name}.{nameof(playerScript)} reference was null.");
			amKosher = false;
		}

		if ( anim_player == null )
		{
			PV_Debug.LogError($"{name}.{nameof(anim_player)} reference was null.");
			amKosher = false;
		}

		return amKosher;
	}
}