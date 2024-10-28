using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_Base_living", fileName = "stats_Base_living")]
public class Stats_Base_living : ScriptableObject
{
    [Header("[----------GROUNDING/JUMPING----------]")]
    [Range(0f, 4f)]
    public float Radius_GroundCheckSphere = 0.1f;
    [Range(-3f, 3f)]
    [Tooltip("Determines how far above (or below if negative) the sphere check for jumping starts.")]
    public float VerticalOffset_GroundCheckSphere = 0f;
    [Range(0f, 1f)]
    [Tooltip("Is compared to the RaycastHit.normal that hits under the player's feet to decide if the ground is flat enough to support jumping. Lower value is more forgiving.")]
    public float yThreshAmGrounded;
    [Tooltip("Keeps player from immediately jumping after landing. IE: Amount of time before player is allowed to jump again after land.")]
    public float jumpRecoverBufferLength = 0.3f;
    [Tooltip("Amount of force in the y direction needed to trigger the land animation.")]
    public float landForceThreshold;
    //[Tooltip("Amount of time after landing that we restrict player's ability to jump again.")]
    //public float cdJumpRecoverBuffer = 0f;

    [Header("[----------VISION----------]")]
    /// <summary>Determines "peripheral" vision. Compared against an InverseTransformVector</summary>
    //public static float visionRadius = 20f; //todo: this needs to be changed to itp format...
    public float Distance_vision = 15f;
    [Range(0f, 1f), Tooltip("Percentage that an enemy's forward vector has to be aligned with something to be considered in it's 'sight cone'. " +
        "This will be a percentage from 0 to 1 with 0.5 being 45 degrees to the side. Higher means a tighter (smaller) vision cone (IE: less peripheral vision)")]
    public float Threshold_VisionRadius = 0.2f;
    //public float visionRangeVertical = 0.65f;
    public float SuspicionMultiplier_distance = 15f;
    public float SuspicionMultiplier_direction = 2f;

    [Header("[---------- OTHER ----------]")]
    [Range(0f, 1f), Tooltip("1 is perfect hearing. IE: Can hear the maximum amount of sound.")]
    public float HearingMultiplier = 0.5f;
}
