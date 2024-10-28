using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManger : PV_Object
{
    public static DataManger Instance;


    private void Awake()
    {
        Instance = this;

        LogHistoric( $"DataManger Awake()", PV_Enums.PV_LogFormatting.UserMethod );

        LogHistoric( $"End of DataManger Awake", PV_Enums.PV_LogFormatting.UserMethod );

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
        LogHistoric( $"GetLookSensitivityPref()", PV_Enums.PV_LogFormatting.UserMethod );
        float val = PlayerPrefs.GetFloat( "VERTIGO_LOOK_SENSITIVITY", 5f );

        if ( !PlayerPrefs.HasKey("VERTIGO_LOOK_SENSITIVITY") )
        {
            PlayerPrefs.SetFloat( "VERTIGO_LOOK_SENSITIVITY", 5f );
            LogHistoric( "key for sensitivity wasn't present. Set key to default value in playerprefs...", PV_Enums.PV_LogFormatting.UserMethod );
        }
        else
        {
            LogHistoric($"key for sensitivity was present. returning val: '{val}'");
        }

        return val;
    }

    public void SetLookSensitivityPref( float sensitivity_passed )
    {
        LogHistoric( $"SetLookSensitivityPref() '{sensitivity_passed}'", PV_Enums.PV_LogFormatting.UserMethod );
        PlayerPrefs.SetFloat( "VERTIGO_LOOK_SENSITIVITY", sensitivity_passed );
    }
  
    public float GetLookSmoothing()
    {
        LogHistoric( $"GetLookSmoothing()", PV_Enums.PV_LogFormatting.UserMethod );
        float val = PlayerPrefs.GetFloat("VERTIGO_LOOKSMOOTHING", 25f);

        if ( !PlayerPrefs.HasKey( "VERTIGO_LOOKSMOOTHING" ) )
        {
            PlayerPrefs.SetFloat( "VERTIGO_LOOKSMOOTHING", 25f );
            LogHistoric( "key for look smoothing wasn't present. Set key to default value in playerprefs..." );
        }
        else
        {
            LogHistoric($"key for look smoothing was present. returning val: '{val}'");
        }

        return val;
    }

    public void SetLookSmoothing( float val_passed )
    {
        LogHistoric( $"SetLookSmoothing(), '{val_passed}'", PV_Enums.PV_LogFormatting.UserMethod );
        PlayerPrefs.SetFloat("VERTIGO_LOOKSMOOTHING", val_passed);
    }
    #endregion
}
