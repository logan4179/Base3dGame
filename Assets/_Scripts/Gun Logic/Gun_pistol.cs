using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using PV_Enums;

public class Gun_pistol : Base_gun
{
    void Start()
    {
        Log_MethodStart("Gun_pistol Start() begin", PV_LogDestination.Hidden, PV_LogFormatting.UnityAPIMethod);
        CheckIfKosher();
        BaseInit();

        fireRate_calculated = 1f / MyStats.fireRatePerSecond;
        burstCount = 0;

        //The following is just forcing the setup of the gun that I currently want for testing. Won't keep these lines for final game...
        canDoThreeRoundBurst = false;
        canDoFullAuto = true;
        MyFireMode = GunFireMode.SemiAutomatic;

    }

    void Update()
    {
        moveAlarms();
    }


#if UNITY_EDITOR
	private void LateUpdate()
	{
		if (doDebugStrings)
		{
			debugState = $"count_ammunitionCurrentlyInClip: '{count_ammunitionCurrentlyInClip}'\n" +
				$"fireRate_calculated: '{fireRate_calculated}'";

		}
		else if (debugState != string.Empty)
		{
			debugState = string.Empty;
		}
	}

#endif
	protected void moveAlarms()
    {
        if ( cd_FireFullAuto > 0f )
        {
            cd_FireFullAuto -= Time.deltaTime;

            if ( cd_FireFullAuto <= 0f )
            {
                cd_FireFullAuto = 0f;

                if ( PV_Input.AmHoldingTriggerInput && HasAmmunitionInClip && MyFireMode == GunFireMode.FullyAutomatic && MyStats.fireRatePerSecond > 0 )
                {
                    FireBullet();
                }
            }
        }
        /////////////////////////////////////////////////////////
        if( cd_Burst > 0f )
        {
            cd_Burst -= Time.deltaTime;

            if( cd_Burst <= 0f )
            {
                //print($"burst at 0");
                cd_Burst = 0f;
                burstCount++;

                if( HasAmmunitionInClip && MyStats.fireRatePerSecond > 0 )
                {
                    FireBullet();

                    if( burstCount == 2 ) //set to 2 instead of 3 because the first shot isn't really part of the burst, and happens in the player script.
                    {
                        burstCount = 0;
                        cd_Burst = 0f;
                    }
                }
                else
                {
                    burstCount = 0;
                }
            }
        }
        /////////////////////////////////////////////////////////
        if ( cd_DrawGun > 0f )
        {
            cd_DrawGun -= Time.deltaTime;

            if (cd_DrawGun <= 0f)
            {
                if ( Input.GetKey(KeyCode.Mouse1) || Input.GetAxis("LeftTrigger") > 0f )
                {
                    //playerScript.anim.SetBool("bGunDrawn",true); //todo: dwf
                    SetMeToDrawn();
                }

                cd_DrawGun = 0f;
            }
        }
        /////////////////////////////////////////////////////////
        if ( cd_Light_MuzzleFlash > 0f )
        {
            cd_Light_MuzzleFlash -= Time.deltaTime;

            if( cd_Light_MuzzleFlash <= 0 )
            {
                cd_Light_MuzzleFlash = 0f;
                light_MuzzleFlash.enabled = false;
            }
        }
		/////////////////////////////////////////////////////////
		
#if UNITY_EDITOR
		if (doDebugStrings)
		{
			debugAlarms = $"cd_FireFullAuto: '{cd_FireFullAuto}'\n" +
				$"cd_Light_MuzzleFlash: '{cd_Light_MuzzleFlash}\n" +
				$"cd_Burst: '{cd_Burst}\n" +
				$"cd_DrawGun: '{cd_DrawGun}\n" +
				"";
		}
#endif
	}

	public override void FireBullet()
    {
        Log_MethodStart($"FireBullet start");

        base.FireBullet();

        if ( count_ammunitionCurrentlyInClip > 0 )
        {
            if( MyFireMode == GunFireMode.FullyAutomatic )
            {
                cd_FireFullAuto = fireRate_calculated;
            }
            else if( MyFireMode == GunFireMode.ThreeRoundBurst )
            {
                Log( $"setting burst alarm to '{fireRate_calculated}'" );
                cd_Burst = fireRate_calculated;
            }
        }
    }

    public override void ConstrainToHand()
    {
        Log_MethodStart($"ConstrainToHand('{name}')");
        anim.SetBool( animID_WeaponDrawn, true );
    }

    public override void ConstrainToHolster()
    {
        Log_MethodStart($"ConstrainToHolster('{name}')");
        anim.SetBool( animID_WeaponDrawn, false );
    }

    public override void BeginDraw()
    {
        Log_MethodStart("BeginDraw()");

        cd_DrawGun = MyStats.duration_cd_DrawGun;
    }

    public override void SetMeToDrawn()
    {
        Log_MethodStart("SetMeToDrawn()") ;


    }

    /// <summary>
    /// Gets called via player script's animation event.
    /// </summary>
    public override void InitiateReload()
    {
        base.InitiateReload();
    }

    public override void SetMeToUndrawn()
    {
        lineRenderer_muzzlePos.enabled = false;

    }

	public override bool CheckIfKosher()
	{
		bool amKosher = base.CheckIfKosher();

		if ( animatorWeaponMode != 1 )
		{
			PV_Debug.LogError($"{nameof(Gun_pistol)}.{nameof(animatorWeaponMode)} value should be 1");
			amKosher = false;
		}

		return amKosher;
	}
}
