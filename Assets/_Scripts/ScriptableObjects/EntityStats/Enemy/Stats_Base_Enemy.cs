using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_Base_Enemy", fileName = "stats_Base_Enemy")]
public class Stats_Base_Enemy : ScriptableObject
{
    [Header("[----------MOVEMENT----------]")]
    public float Speed_Move_fast = 8.3f;
    [Tooltip("Movement speed while patrolling.")]
    public float Speed_move_patrol = 0.8f;
    [Tooltip("Speed when suspicious and moving toward where the player was last seen.")]
    public float Speed_Move_suspicious = 1.2f;

    [Header("[----------ROTATION----------]")]
    public float Speed_Rotate_patrol = 1.1f;
    public float Speed_Rotate_fast = 5f;
    [Tooltip("Maximum force amount used for regular rotation to orient towards a goal. This is necessarily lessened the closer the enemy is to pointing towards it's goal to avoid inertia-based overshoot.")]
    public float force_Torque_regular;
    [Tooltip("Maximum force amount used for rotation to orient towards a goal when attacking or chasing the player. This is necessarily lessened the closer the enemy is to pointing towards it's goal to avoid inertia-based overshoot.")]
    public float force_Torque_excited;

    [Header("[----------PATROLLING----------]")]
    [Tooltip("How many meters to patrol around my start location. More accurately, how many meters around my start location that FetchRandomVectorWithin() will be passed when looking for a new vector.")]
    public float normalPatrolRadius = 20f;

    [Range(0f, 1f), Tooltip("Percentage this enemy needs to be within to be considered adequately facing the next patrol point in order to both rotate and move forward at the same time. " +
        "If the enemy is beyond this limit, they should only rotate until they are within it before they're allowed to move forward.")]
    public float Percentage_consideredRoughlyFacing_patrolPt = 0.7f;
    public float dist_MaxPatrolCanBeFromMyOrigin = 20f;
    public float Duration_PatrolWait_min = 4f;
    public float Duration_PatrolWait_max = 13f;

    [Header("[----------CHASING----------]")]
    //[Tooltip("Distance enemy should travel to be within if he is chasing the player. At this distance, the enemy will either become one of the engaging enemies, or hang back and wait for an engaging spot to open.")]
    //public float dist_chaseTo = 10f;
    [Tooltip("The amount of time the enemy will remain in the alert state after becoming aware of the player.")]
    public float Duration_alertPursuit = 30f;
    [Tooltip("Distance, beyond which, this enemy will go from engaging to chasing")]
    public float Dist_TriggerChasing_fromEngaging = 6f;
    [Tooltip("Height, beyond which, this enemy will go from engaging to chasing")]
    public float Height_TriggerChasing_fromEngaging; //todo: is this being used? Actually, I think this is so that the bug enemies can decide if they need to crawl up a ledge further to attack...

    [Header("[----------ENGAGING----------]")]
    [Range(0f, 1f), Tooltip("Percentage this enemy needs to be within to be considered adequately facing the next patrol point in order to both rotate and move forward at the same time. " +
        "If the enemy is beyond this limit, they should only rotate until they are within it before they're allowed to move forward.")]
    public float Percentage_consideredRoughlyFacing_threat = 0.7f;
    [Tooltip("Distance within which an enemy in the chasing state should be put in the engaging state.")]
    public float Dist_TriggerEngaging = 1.5f;


    [Header("[----------TRAVELLING----------]")]
    [Range(0f, 0.5f)] public float dist_RoughlyThere = 0.3f;

    [Header("[----------ATTACKING----------]")]
    [Range(1f, 10f)] public float AggressionLevel;
    public float Duration_MinWaitBetweenAttacks = 2f;
    public float Duration_ApproachWithIntentToStrike = 6f;

    [Header("[---------- TAKE DAMAGE----------]")]
    public float Duration_cd_TakingDamage = 0.5f;

}
