using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>Enemy manager class.</summary>
public class MGR_BugEnemy : PV_Object
{
    public static MGR_BugEnemy Instance;

    [Header("REFERENCE")]
    public Stats_Enemy_bug Stats_bug;
    private List<Enemy_Bug> bugInstances;

	/// <summary>These are the instances that are currently in an 'active' area, and therefore need to be continually calculating logic.</summary>
	private List<Enemy_Bug> bugInstances_active;
    public List<Enemy_Bug> BugInstances_active => bugInstances_active;
    private List<GameObject> BugInstances_dead;

    [Space(20f)]
    [Header("REFERENCE - EXTERNAL")]
    [SerializeField] private GameObject prefab_bugEnemy;

    [Space(20f)]
    [Header("STATIC-GLOBAL")]
    /// <summary>This tells if any enemies of this specific class are engaging with the player. So far, just used so that other classes can quickly tell how many enemies are engaging should they need to know.</summary>
    public static int numbEngaging = 0;

    [Space(20f)]
    [Header("---------------[[ OTHER ]]-----------------")]
    [SerializeField] private Transform trans_spawner;

    private void Awake()
    {
        Log_MethodStart( $"mgrBasicEnemy Awake()" );
        if( Instance == null )
        {
            Instance = this;
        }
        else
        {
            PV_Debug.LogError( $"VERTIGO ERROR! Enemy manager instance was not set to null on Awake. Is there more than one in the scene?" );
        }

        bugInstances_active = new List<Enemy_Bug>();
        BugInstances_dead = new List<GameObject>();
    }

    void Start()
    {
        Log_MethodStart($"mgrBasicEnemy Start()");

		#region Animation--------------
		Enemy_Bug.animID_Walking = Animator.StringToHash( Stats_bug.AcString_Walking );
		Enemy_Bug.animID_Running = Animator.StringToHash(Stats_bug.AcString_Running);
		Enemy_Bug.animID_Attack = Animator.StringToHash(Stats_bug.AcString_Attack);
		Enemy_Bug.animID_AttackChoice = Animator.StringToHash(Stats_bug.AcString_AttackChoice);
		Enemy_Bug.animID_TakingDamage = Animator.StringToHash(Stats_bug.AcString_TakingDamage);
		Enemy_Bug.animID_Dying = Animator.StringToHash(Stats_bug.AcString_Dying);
		Enemy_Bug.animID_DamageClip = Animator.StringToHash(Stats_bug.AcString_i_DamageClip);
        #endregion

        if( bugInstances != null && bugInstances.Count > 0 )
        {
            Log( $"Had '{bugInstances.Count}' bug instances logged at start." );
        }
        else
        {
            PV_Debug.LogWarning( $"{nameof(MGR_BugEnemy)} had no bug instances registered at start. Was this a mistake?" );
        }
    }

    [ContextMenu("z call SpawnBug()")]
    public Enemy_Bug SpawnBug()
    {
        if( prefab_bugEnemy == null )
        {
            PV_Debug.LogError( $"{nameof(MGR_BugEnemy)}.{nameof(prefab_bugEnemy)} reference was null! Returning early..." );
            return null;
        }

        Enemy_Bug bug = PV_Utilities.ConnectedInstantiate( prefab_bugEnemy, transform, trans_spawner ).GetComponent<Enemy_Bug>();
		RegisterBug( bug );

        try
        {
            bug.Trans_patrolPointGrabber.GetComponent<PatrolPointGrabber>().cachedEnvironmentManagerReference = 
                GameObject.FindGameObjectWithTag( PV_GameManager.Tag_EnvironmentManager ).GetComponent<PV_Environment>();
        }
        catch ( System.Exception )
        {
            PV_Debug.LogWarning($"Couldn't find the environment manager by tag. You'll need to manually set this for reference in the Patrol Point Grabber.");
            
        }


		PV_Debug.LogWarning( $"You'll now need to explicitely set this enemy's triggering area variable AND patrol points." );

        return bug;
    }

    public void level2Logic_action( float passedDeltaTime )
    {
        if( bugInstances_active != null && bugInstances_active.Count > 0 )
        {
		    foreach ( Enemy_Bug _scrBasicEnemy in bugInstances_active )
		    {
			    _scrBasicEnemy.CalculatePlayerVisibility( passedDeltaTime );
			    _scrBasicEnemy.DecideNextAction( passedDeltaTime );
		    }
        }
	}

    public void level3Logic_action(float passedDeltaTime)
    {
        if ( bugInstances_active != null && bugInstances_active.Count > 0 )
        {
			foreach ( Enemy_Bug bug in bugInstances_active )
			{
				if ( bug.CanSeePlayer )
				{
					bug.UpdatePath( PV_GameManager.Instance.Trans_Player.position, 0.5f );
				}
			}
		}
    }

	public void level4Logic_action( float passedDeltaTime )
	{
		if ( bugInstances_active != null && bugInstances_active.Count > 0 )
		{
			foreach ( Enemy_Bug bug in bugInstances_active )
			{
				if ( bug.MyPath.AmValid )
				{
					bug.CheckPath();
				}
			}
		}
	}

    public void RegisterBug( Enemy_Bug bug_passed )
    {
        if( bugInstances == null )
        {
            bugInstances = new List<Enemy_Bug>();
        }

        if( !bugInstances.Contains( bug_passed ) )
        {
            bugInstances.Add( bug_passed );
        }
    }
    public void RemoveBugFromManager( Enemy_Bug bug )
    {
        if( bugInstances != null && bugInstances.Contains( bug ) )
        {
            bugInstances.Remove( bug );
        }
    }

    public void ActivateBug( Enemy_Bug bug_passed )
    {
        Log($"{nameof(ActivateBug)}('{bug_passed.name}')");
        if ( !bugInstances_active.Contains(bug_passed) )
        {
            bugInstances_active.Add( bug_passed );
        }

        bug_passed.gameObject.SetActive( true );
    }

    public void DeactivateBug( Enemy_Bug bug_passed )
    {
        LogInc($"{nameof(DeactivateBug)}('{bug_passed.name}')", PV_Enums.PV_LogDestination.Console);

        if ( bugInstances_active.Contains(bug_passed) )
        {
            bugInstances_active.Remove(bug_passed);
            Log($"'{bugInstances_active}' contained the passed enemy. Removed it from this list...");
        }

        bug_passed.gameObject.SetActive( false );
    }

    public void RegisterDeadBug( GameObject bug_passed )
    {
        PV_Debug.Log( $"RegisterDeadBug('{bug_passed.name}')", PV_Enums.PV_LogFormatting.UserMethod, PV_Enums.PV_LogDestination.Console );
        Enemy_Bug bugScript = null;
        if( !bug_passed.TryGetComponent<Enemy_Bug>(out bugScript) )
        {
            PV_Debug.LogError($"\t PV ERROR! Couldn't get bug script component from passed object. Returning early...");
            return;
        }

        if( bugInstances == null || bugInstances.Count <= 0 )
        {
            PV_Debug.LogError($"\t PV ERROR! BugInstances was null or 0. It should be set at this point...");
        }
        else if ( !bugInstances.Contains(bugScript) )
        {
            PV_Debug.LogError($"\t PV ERROR! BugInstances was NOT null or 0, but it somehow didn't contain this bug...");
        }

        if ( BugInstances_dead == null )
        {
            BugInstances_dead = new List<GameObject>();
        }

        BugInstances_dead.Add( bug_passed );
    }

    /// <summary>
    /// Used to create the effect of an audible sound that the enemies can react to if close enough.
    /// </summary>
    /// <param name="pos_passed"></param>
    /// <param name="volumeDistance_passed"></param>
    public void EmulateEnvironmentalSound( Vector3 pos_passed, float volumeDistance_passed )
    {
        foreach( Enemy_Bug bug in bugInstances )
        {
            bug.EmulateEnvironmentalSound( pos_passed, volumeDistance_passed );
        }
    }

    public void HandlePlayerDeath()
    {
        PV_Debug.Log($"HandlePlayerDeath()", PV_Enums.PV_LogFormatting.UserMethod, PV_Enums.PV_LogDestination.ConsoleAndMomentaryLogger );
        foreach( Enemy_Bug bug in BugInstances_active )
        {
            if( bug.MyActionState != PV_Enums.EnemyActionState.Patrolling )
            {
                bug.HandlePlayerDeath();
            }
        }
    }

	public string GetDebugString()
    {
		return $"{nameof(numbEngaging)}: '{numbEngaging}'.\n";

	}
}
