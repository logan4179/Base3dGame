using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LogansReverbManagementSystem;

/// <summary>Audio manager script class.</summary>
public class PV_AudioManager : MonoBehaviour
{
    public static PV_AudioManager Instance;
    //[Header("STATS")]
    public static float volume_Master, volume_Effects, volume_Music;

    [Header("REFERENCE")]
    [SerializeField] private List<ReverbFollower> reverbFollowers;

    [Header("DEBUG")]
    [TextArea(1, 10), SerializeField] private string DebugCanonString;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PV_Debug.Log( $"PV_AudioManager Start()", ref DebugCanonString, PV_Enums.PV_LogFormatting.UserMethod );

        volume_Master = DataManger.GetMasterVolumePref();
        volume_Effects = DataManger.GetEffectsVolumePref();
        volume_Music = DataManger.GetMusicVolumePref();

        PV_Debug.Log($"End of PV_AudioManager Start(), master: '{volume_Master}', effects: '{volume_Effects}', music: '{volume_Music}'\"", ref DebugCanonString, PV_Enums.PV_LogFormatting.UserMethod );
        DebugCanonString += $"End of PV_AudioManager Start(), master: '{volume_Master}', effects: '{volume_Effects}', music: '{volume_Music}'\n";
    }

    public static void EmulateEnvironmentalSound( Vector3 pos_passed, float volumeDistance_passed )
    {
        MGR_BugEnemy.Instance.EmulateEnvironmentalSound( pos_passed, volumeDistance_passed );
    }
}