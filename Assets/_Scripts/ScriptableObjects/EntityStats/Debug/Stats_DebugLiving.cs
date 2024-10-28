using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_DebugLiving", fileName = "stats_DebugLiving")]
public class Stats_DebugLiving : ScriptableObject
{
    [Header("---------------[[ PATHS ]]-----------------")]

    public Color Color_PathPt;
    public Color Color_PatrolPt_highlighted;
    //[Range(0f, 3f)]
    //public float Size_PathCones = 0.3f;
    //[Range(0f, 3f)]
    //public float VerticalOffset_PathCones = 0.5f;
    //public float Size_PathVerticalLineWidth;
    [Range(0f, 3f)]
    public float Height_PathVerticalLine;
    [Range(0f, 15f)]
    public float Width_PathVerticalLine = 6f;
    [Range(0f, 10f)]
    public float Size_DottedLine = 6;
    [Range(0f, 3f)]
    public float Size_PatrolArrow =1f;

    [Space(10f)]
    [Header("---------------[[ PATROL POINT ]]-----------------")]
    public Color Color_patrolPointGrabber;
    [Range(0.05f, 2f)] public float Size_patrolPointGrabber;
    [Space(3f)]
    public Color Color_cachedPatrolPoints;

    [Space(10f)]
    [Header("---------------[[ VISION ]]-----------------")]
    public Color Color_VisionIndicatorLines;

    [Header("---------------[[ ATTACK COLLIDERS ]]-----------------")]
    public Color Color_attackColliders_unarmed;
	public Color Color_attackColliders_armed;

}
