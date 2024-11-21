using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManger : PV_Object
{
    public static DataManger Instance;


    private void Awake()
    {
        Instance = this;

        Log_MethodStart($"DataManger Awake()");


    }

    #region AUDIO-----------------------------------------------
    public static float GetMasterVolumePref()
    {
        PV_Debug.Log( $"GetMasterVolumePref()", PV_Enums.PV_LogFormatting.UserMethod );

        float val = PlayerPrefs.GetFloat( "VERTIGO_MASTER_VOLUME", 100f );

        if ( !PlayerPrefs.HasKey("VERTIGO_MASTER_VOLUME") )
        {
            PlayerPrefs.SetFloat("VERTIGO_MASTER_VOLUME", 100f);
            PV_Debug.Log($"key for master volume wasn't present. Set key to default value in playerprefs...");
        }

        return val;
    }

    public static void SetMasterVolumePref( float volume_passed )
    {
        PV_Debug.Log( $"SetMasterVolumePref()", PV_Enums.PV_LogFormatting.UserMethod );
        PlayerPrefs.SetFloat( "VERTIGO_MASTER_VOLUME", volume_passed );
    }

    public static float GetMusicVolumePref()
    {
        PV_Debug.Log( $"GetMusicVolumePref()", PV_Enums.PV_LogFormatting.UserMethod );
        float val = PlayerPrefs.GetFloat("VERTIGO_MUSIC_VOLUME", 100f);

        if ( !PlayerPrefs.HasKey("VERTIGO_MUSIC_VOLUME") )
        {
            PlayerPrefs.SetFloat("VERTIGO_MUSIC_VOLUME", 100f);
            PV_Debug.Log( $"\"key for music volume wasn't present. Set key to default value in playerprefs...\"" );
        }

        return val;
    }

    public static void SetMusicVolumePref(float volume_passed)
    {
        PV_Debug.Log( $"SetMusicVolumePref()", PV_Enums.PV_LogFormatting.UserMethod );
        PlayerPrefs.SetFloat("VERTIGO_MUSIC_VOLUME", volume_passed);
    }
    public static float GetEffectsVolumePref()
    {
        PV_Debug.Log( $"GetEffectsVolumePref()", PV_Enums.PV_LogFormatting.UserMethod );
        float val = PlayerPrefs.GetFloat("VERTIGO_EFFECTS_VOLUME", 100f);

        if (!PlayerPrefs.HasKey("VERTIGO_EFFECTS_VOLUME"))
        {
            PlayerPrefs.SetFloat("VERTIGO_EFFECTS_VOLUME", 100f);
            PV_Debug.Log( "key for effects volume wasn't present. Set key to default value in playerprefs..." );
        }

        return val;
    }

    public static void SetEffectsVolumePref(float volume_passed)
    {
        PV_Debug.Log( $"SetEffectsVolumePref()" , PV_Enums.PV_LogFormatting.UserMethod);
        PlayerPrefs.SetFloat("VERTIGO_EFFECTS_VOLUME", volume_passed);
    }
    #endregion

    #region CONTROLS---------------------
    public float GetLookSensitivityPref()
    {
        Log_MethodStart($"GetLookSensitivityPref()");
        float val = PlayerPrefs.GetFloat( "VERTIGO_LOOK_SENSITIVITY", 5f );

        if ( !PlayerPrefs.HasKey("VERTIGO_LOOK_SENSITIVITY") )
        {
            PlayerPrefs.SetFloat( "VERTIGO_LOOK_SENSITIVITY", 5f );
            Log( "key for sensitivity wasn't present. Set key to default value in playerprefs..." );
        }
        else
        {
            Log($"key for sensitivity was present. returning val: '{val}'");
        }

        return val;
    }

    public void SetLookSensitivityPref( float sensitivity_passed )
    {
        Log_MethodStart($"SetLookSensitivityPref() '{sensitivity_passed}'");
        PlayerPrefs.SetFloat( "VERTIGO_LOOK_SENSITIVITY", sensitivity_passed );
    }
  
    public float GetLookSmoothing()
    {
        Log_MethodStart($"GetLookSmoothing()");
        float val = PlayerPrefs.GetFloat("VERTIGO_LOOKSMOOTHING", 25f);

        if ( !PlayerPrefs.HasKey( "VERTIGO_LOOKSMOOTHING" ) )
        {
            PlayerPrefs.SetFloat( "VERTIGO_LOOKSMOOTHING", 25f );
            Log( "key for look smoothing wasn't present. Set key to default value in playerprefs..." );
        }
        else
        {
            Log($"key for look smoothing was present. returning val: '{val}'");
        }

        return val;
    }

    public void SetLookSmoothing( float val_passed )
    {
        Log_MethodStart($"SetLookSmoothing(), '{val_passed}'");
        PlayerPrefs.SetFloat("VERTIGO_LOOKSMOOTHING", val_passed);
    }
    #endregion

    #region EDITOR HELPERS --------------------------------
    [ContextMenu("z call SayControlPrefs")]
    public void SayControlPrefs()
    {
        Debug.Log($"{nameof(SayControlPrefs)}()...");

        Debug.Log($"{nameof(GetLookSensitivityPref)}(): '{GetLookSensitivityPref()}'\n" +
            $"{nameof(GetLookSmoothing)}(): '{GetLookSmoothing()}'\n");
    }
	#endregion
}
