using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PV_Enums;
using System.Linq;
using UnityEngine.AI;
using LogansPoolingSystem;
using System.Text;
using LogansAreaManagementSystem;

/// <summary>Manager class for the level</summary>
public class PV_Environment : PV_Object
{
    [SerializeField] private bool amDebuggingMethod;

    [Header("----------REFERENCE [INTERNAL]---------")]
    public static PV_Environment Instance;
    public Stats_environment MyStats;

    //[Header("----------REFERENCE [EXTERNAL]---------")]

    //[Space(10f)]

    [Header("----------REFERENCE [AREAS]---------")]
    public LAMS_Area StartingArea;
    public List<LAMS_Area> AllAreas = new List<LAMS_Area>();
    /// <summary>The areas that are currently active in the scene.</summary>
    [SerializeField] private List<LAMS_Area> Areas_active = new List<LAMS_Area>();
    [Space(10f)]

    [Header("----------REFERENCE [POOLS]---------")]
    [SerializeField] private Pool pool_gunshotDecalsA;
    public Pool Pool_gunshotDecalsA => pool_gunshotDecalsA;

    [SerializeField] private PV_EffectPool pool_bulletImpact_concrete_A;
    public PV_EffectPool Pool_bulletImpact_concrete_A => pool_bulletImpact_concrete_A;

	[SerializeField] private PV_EffectPool pool_bloodDamageEffectsA_red;
	public PV_EffectPool Pool_bloodDamageEffectsA_red => pool_bloodDamageEffectsA_red;

	[SerializeField] private PV_EffectPool pool_bloodDamageEffectsA_green;
	public PV_EffectPool Pool_bloodDamageEffectsA_green => pool_bloodDamageEffectsA_green;

	[SerializeField] private PV_EffectPool pool_bloodDamageEffectsA_blue;
	public PV_EffectPool Pool_bloodDamageEffectsA_blue => pool_bloodDamageEffectsA_blue;

	[SerializeField] private PV_EffectPool pool_bloodDamageEffectsA_purple;
	public PV_EffectPool Pool_bloodDamageEffectsA_purple => pool_bloodDamageEffectsA_purple;

    [SerializeField] private Pool pool_bloodSpotA_red;
    public Pool Pool_bloodSpotA_red => pool_bloodSpotA_red;

	[SerializeField] private Pool pool_bloodSpotA_green;
	public Pool Pool_bloodSpotA_green => pool_bloodSpotA_green;

	[SerializeField] private Pool pool_bloodSpotA_blue;
	public Pool Pool_bloodSpotA_blue => pool_bloodSpotA_blue;

	[SerializeField] private Pool pool_bloodSpotA_purple;
	public Pool Pool_bloodSpotA_purple => pool_bloodSpotA_purple;

	[Header("LAYERS")]
    public int Layer_Solid_Environmental;
    public int Layer_Living;

    [Header("LAYER MASKS")]
    public int Mask_EnvSolid;
    public int Mask_Destructable;
    /// <summary>Catches all layers that a living entity shouldn't be able to see through at all.</summary>
    public int Mask_CompletelyOpaque;
    /// <summary>Catches all layers that the player should be able to walk/jump on. Currently only needed for jumping, but maybe could be used for walking somehow.</summary>
    public int Mask_WalkableJumpable;
    /// <summary>Catches everything that can receive a bullet, whether damageable or not.  This way the raycast ends at this object..</summary>
    public int Mask_Shootable;
    /// <summary>Mask for surfaces that muffle, but don't mute, sounds.</summary>
    public int Mask_MufflesSound;
	/// <summary>Mask for all living entities.</summary>
	public int Mask_Living;


    private void Awake()
    {
        Log_MethodStart($"PV_Environment Awake()");
        if ( Instance == null )
        {
            Instance = this;
		}
		else
        {
            PV_Debug.LogError($"VERTIGO ERROR! Environment manager instance was not set to null on Awake. Is there more than one in the scene?");
        }
    }

    void Start()
    {
        Log_MethodStart($"PV_Environment Start()");

        CalculateAndCacheLayersAndMasks();
    }

    [ContextMenu("z Call CalculateAndCacheLayers()")]
    public void CalculateAndCacheLayersAndMasks()
    {
        //SET UP LAYERS------------------
        Layer_Solid_Environmental = LayerMask.NameToLayer("lr_EnvSolid");
        Layer_Living = LayerMask.NameToLayer("lr_Living");
        
        //SET UP MASKS-------------------
        Mask_EnvSolid = LayerMask.GetMask("lr_EnvSolid");
        Mask_Destructable = LayerMask.GetMask("lr_Destructable");
        Mask_CompletelyOpaque = LayerMask.GetMask("lr_EnvSolid", "lr_Living");
        Mask_WalkableJumpable = LayerMask.GetMask("lr_EnvSolid");
        Mask_Shootable = LayerMask.GetMask("lr_EnvSolid", "lr_Living");
        Mask_MufflesSound = LayerMask.GetMask("lr_EnvSolid");
        Mask_Living = LayerMask.GetMask("lr_Living");

	}

    /// <summary>Returns a random vector for enemy patrols.</summary>
    /// <param name="sampleOrigin">Origin position to find a random spot around.</param>
    /// <param name="currentPos">Current position of the entity, to make sure it isn't going too far or not far enough.</param>
    /// <param name="sampleRadius">Radius around the sampleOrigin to sample within.</param>
    /// <param name="maxSingleMoveDist">The maximum distance this method will return away from currentPos.</param>
    /// <param name="minSingleMoveDist">The minimum distance this method will return away from currentPos.</param>
    /// <returns></returns>
    public static Vector3 FetchRandomVectorWithin( Vector3 sampleOrigin, Vector3 currentPos, float sampleRadius, bool sampleNavMesh = true )
    {
        StringBuilder sb = new StringBuilder($"start of fetchRandomVectorWithin({nameof(sampleOrigin)}: '{sampleOrigin}', {nameof(sampleRadius)}: '{sampleRadius}')");
        Vector3 nextPt = sampleOrigin;

        if( sampleNavMesh )
        {
			sb.AppendLine( $"am sampling navmesh..." );
            NavMeshHit hit;

            for ( int i = 0; i < 15; i++ )
            {
                Vector3 vTry = new Vector3( 
                    Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f) 
                    ).normalized * Random.Range(0f, sampleRadius);

				sb.AppendLine( $"vTry: '{vTry}'. mag: '{vTry.magnitude}'. Now attempting sample..." );

				if ( NavMesh.SamplePosition(sampleOrigin + vTry, out hit, sampleRadius, NavMesh.AllAreas) )
                {
					sb.AppendLine( $"sample true!\n" );

					float distToPos = Vector3.Distance( currentPos, hit.position );
					sb.AppendLine( $"distToPos was: '{distToPos}'\n" );

                    nextPt = hit.position;
					sb.AppendLine(
						$"Patrolpoint with sampleOrigin: '{sampleOrigin}' took '{i}' tries to generate a new position at: '{nextPt}'."
                        );
                    Instance.Log( sb.ToString() );
                    return nextPt;
                }
                else
                {
					sb.AppendLine($"sample returned false...");
				}
			}

			sb.AppendLine($"Patrolpoint with sampleOrigin: '{sampleOrigin}' tried 15 times to generate a new position. Returning current position at this point...");
        }
        else
        {
            nextPt = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(0f, sampleRadius);

        }

		print(sb);

		return nextPt;
    }
}
