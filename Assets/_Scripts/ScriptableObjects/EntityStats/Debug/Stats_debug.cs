using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_debug", fileName = "stats_debug")]
public class Stats_debug : ScriptableObject
{
	[Header("------- [ENVIRONMENT] --------")]
	public Color Color_PickupObjectCollider_debug;
    public Color Color_PromptObjectCollider_debug;
    public Color Color_ReverbFollower_debug;
    public Color Color_ReverbZone_debug;
	public Color Color_AreaTrigger_debug;


	[Header("---------------[[ AIRSPACE ]]-----------------")]
    public Color Color_airpointCreatorLine = Color.white;
    public Color Color_airpointCreatorSphere = Color.white;
    [Range(0f, 3f)] public float Radius_airpointCreatorSphere = 1f;
    [Range(0f, 3f)] public float Radius_airpointCreatorSegmentSphere = 1f;
    public Color Color_airpointCreatorSegment = Color.white;

    [Space(10f)]
    public Color Color_pointPositionSphere, Color_pointPositionSphere_highlighted;
    [Range(0f, 3f)] public float Radius_pointPositionSphere, Radius_pointPositionSphere_highlighted;
    public Color Color_relatedPointLine;
}
