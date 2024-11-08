using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>Enemy manager class.</summary>
public class MGR_BasicFlyer : PV_Object
{
    public static MGR_BasicFlyer Instance;

    [Header("REFERENCE")]
    public Stats_Enemy_basicFlyer Stats_basicFlyer;
    public Stats_DebugLiving Stats_debug;
    public List<Enemy_BasicFlyer> BasicFlyerInstances;
    /// <summary>These are the instances that are currently in an 'active' area, and therefore need to be continually calculating logic.</summary>
    public List<Enemy_BasicFlyer> BasicFlyerInstances_active;
    public List<GameObject> BasicFlyerInstances_dead;

    [Space(20f)]
    [Header("REFERENCE - EXTERNAL")]
	[SerializeField] private GameObject prefab_basicFlyer;
	public PV_Player PlayerScript;

    [Space(20f)]
    [Header("STATIC-GLOBAL")]
    /// <summary>This tells if any enemies of this specific class are engaging with the player. So far, just used so that other classes can quickly tell how many enemies are engaging should they need to know.</summary>
    public static int numbEngaging = 0;

    [Space(20f)]
	[Header("---------------[[ OTHER ]]-----------------")]
	[SerializeField] private Transform trans_spawner;


	//[Space(20f)]
	[Header("---------------[[ DEBUG ]]-----------------")]
    public bool AmDebugging;
    public bool AmEditorDebugging;

    private void Awake()
    {
        Log_MethodStart($"mgrBasicEnemy Awake()");
        if( Instance == null )
        {
            Instance = this;
            Log("static instance was null. Set it to this.");
        }
        else
        {
            PV_Debug.LogError( $"VERTIGO ERROR! Enemy manager instance was not set to null on Awake. Is there more than one in the scene?" );
        }

        AmDebugging = true;
    }

    void Start()
    {
        Log_MethodStart($"mgrBasicEnemy Start()");

        AmDebugging = false;

		#region Animation--------------
		Enemy_BasicFlyer.animID_Flying = Animator.StringToHash( Stats_basicFlyer.AcString_Flying );
        Enemy_BasicFlyer.animID_Attack = Animator.StringToHash(Stats_basicFlyer.AcString_Attack);
		Enemy_BasicFlyer.animID_AttackChoice = Animator.StringToHash(Stats_basicFlyer.AcString_AttackChoice);
        //Enemy_BasicFlyer_members.animID_TakingDamage = Animator.StringToHash(Stats_basicFlyer.AcString_TakingDamage);
        //Enemy_BasicFlyer_members.animID_Dying = Animator.StringToHash(Stats_basicFlyer.AcString_Dying);
		//Enemy_BasicFlyer_members.animID_DamageClip = Animator.StringToHash(Stats_basicFlyer.AcString_i_DamageClip);
        #endregion
    }

	[ContextMenu("z call SpawnBasicFlyer()")]
	public Enemy_BasicFlyer SpawnBasicFlyer()
	{
		if ( prefab_basicFlyer == null )
		{
			PV_Debug.LogError( $"{nameof(MGR_BasicFlyer)}.{nameof(prefab_basicFlyer)} reference was null! Returning early..." );
			return null;
		}

		Enemy_BasicFlyer flyr = PV_Utilities.ConnectedInstantiate( prefab_basicFlyer, transform, trans_spawner ).GetComponent<Enemy_BasicFlyer>();

		RegisterBasicFlyer(flyr );
		PV_Debug.LogWarning($"You'll now need to explicitely set this enemy's triggering area variable AND patrol points.");

		return flyr;
	}

	public void level2Logic_action( float passedDeltaTime )
    {
        if( BasicFlyerInstances_active != null && BasicFlyerInstances_active.Count > 0 )
        {
			foreach ( Enemy_BasicFlyer _scrBasicFlyer in BasicFlyerInstances_active )
			{
				_scrBasicFlyer.CalculatePlayerVisibility( passedDeltaTime );
				_scrBasicFlyer.DecideNextAction( passedDeltaTime );
			}
		}
    }

	public void level3Logic_action( float passedDeltaTime )
	{
		if ( BasicFlyerInstances_active != null && BasicFlyerInstances_active.Count > 0 )
		{
			foreach ( Enemy_BasicFlyer flyr in BasicFlyerInstances_active )
			{
				if ( flyr.CanSeePlayer )
				{
					flyr.UpdatePath(flyr.PlayerFocusPosition, 0.5f);
				}
			}
		}
	}

	public void level4Logic_action( float passedDeltaTime )
	{
		if ( BasicFlyerInstances_active != null && BasicFlyerInstances_active.Count > 0 )
		{
			foreach ( Enemy_BasicFlyer flyr in BasicFlyerInstances_active )
			{
				if ( flyr.MyPath.AmValid )
				{
					flyr.CheckPath();

				}
			}
		}
	}

    public void RegisterBasicFlyer(Enemy_BasicFlyer flyr_passed )
    {
        if ( BasicFlyerInstances == null )
        {
            BasicFlyerInstances = new List<Enemy_BasicFlyer>();
        }

        if ( !BasicFlyerInstances.Contains(flyr_passed) )
        {
            BasicFlyerInstances.Add(flyr_passed);
        }
    }

    public void ActivateBasicFlyer( Enemy_BasicFlyer flyr_passed )
    {
        Log($"ActivateBasicFlyer('{flyr_passed.name}')");
        if ( !BasicFlyerInstances_active.Contains(flyr_passed) )
        {
            BasicFlyerInstances_active.Add( flyr_passed );
        }

        flyr_passed.gameObject.SetActive( true );
    }

    public void UnregisterAndDeactivateBasicFlyer(Enemy_BasicFlyer flyr_passed )
    {
        Log($"UnregisterAndDeactivateBasicFlyer('{flyr_passed.name}')");

        if ( BasicFlyerInstances_active.Contains(flyr_passed) )
        {
            BasicFlyerInstances_active.Remove(flyr_passed);
        }
        flyr_passed.gameObject.SetActive( false );
    }

    public void RegisterDeadBasicFlyer( GameObject flyer_passed )
    {
        PV_Debug.Log( $"RegisterDeadBasicFlyer('{flyer_passed.name}')", PV_Enums.PV_LogFormatting.UserMethod, PV_Enums.PV_LogDestination.Console );
        Enemy_BasicFlyer flyrScript = null;
        if( !flyer_passed.TryGetComponent<Enemy_BasicFlyer>(out flyrScript) )
        {
            PV_Debug.LogError($"\t PV ERROR! Couldn't get flyer script component from passed object. Returning early...");
            return;
        }

        if( BasicFlyerInstances == null || BasicFlyerInstances.Count <= 0 )
        {
            PV_Debug.LogError($"\t PV ERROR! FlyerInstances was null or 0. It should be set at this point...");
        }
        else if ( !BasicFlyerInstances.Contains(flyrScript) )
        {
            PV_Debug.LogError($"\t PV ERROR! FlyerInstances was NOT null or 0, but it somehow didn't contain this flyer...");
        }

        if ( BasicFlyerInstances_dead == null )
        {
            BasicFlyerInstances_dead = new List<GameObject>();
        }

        BasicFlyerInstances_dead.Add( flyer_passed );
    }

    /// <summary>
    /// Used to create the effect of an audible sound that the enemies can react to if close enough.
    /// </summary>
    /// <param name="pos_passed"></param>
    /// <param name="volumeDistance_passed"></param>
    public void EmulateEnvironmentalSound( Vector3 pos_passed, float volumeDistance_passed )
    {
        foreach( Enemy_BasicFlyer flyr in BasicFlyerInstances )
        {
            flyr.EmulateEnvironmentalSound( pos_passed, volumeDistance_passed );
        }
    }

    public void HandlePlayerDeath()
    {
        Log_MethodStart($"HandlePlayerDeath()" );
        foreach(Enemy_BasicFlyer flyr in BasicFlyerInstances_active )
        {
            if( flyr.MyActionState != PV_Enums.EnemyActionState.Patrolling )
            {
                flyr.HandlePlayerDeath();
                ///Debug.Break();
            }
        }
    }

	public string GetDebugString()
	{
		return $"{nameof(numbEngaging)}: '{numbEngaging}'.\n";

	}
}
