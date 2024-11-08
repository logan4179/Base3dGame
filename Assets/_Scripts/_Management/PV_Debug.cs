using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using PV_Enums;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Rendering.VirtualTexturing;
using LoganPackages;
using LHM;

/// <summary>
/// Debug manager class for game.
/// </summary>
public class PV_Debug : MonoBehaviour
{
    public static PV_Debug Instance;

    [Header("[------ REFERENCE ------]")]
    [SerializeField] private MomentaryDebugLogger momentaryDebugLogger_cached;


    private void Awake()
    {
        Instance = this;

    }

    public static void Log(string message_passed, PV_LogFormatting logType_passed = PV_LogFormatting.Standard, PV_LogDestination logDestination_passed = PV_LogDestination.Hidden )
    {
        string s = "";

        if ( logType_passed == PV_LogFormatting.UserMethod )
        {
            s += $"<color=white><b>-----------------{message_passed}----------------</b></color>";
        }
        else
        {
            s = message_passed;
        }

        if( logDestination_passed == PV_LogDestination.Everywhere || logDestination_passed == PV_LogDestination.Console || logDestination_passed == PV_LogDestination.ConsoleAndMomentaryLogger )
        {
            Debug.Log(s);
        }

        s += Environment.NewLine;
        if ( logDestination_passed == PV_LogDestination.Everywhere || logDestination_passed == PV_LogDestination.MomentaryLogger )
        {
            Instance.momentaryDebugLogger_cached.LogMomentarily( message_passed );

			//Instance.tmp_MomentaryDebugLogger.text += s;
            //Instance.momentaryLogs.Add(s);

            //Instance.StartCoroutine( Instance.DeleteMomentaryLog(s) );
        }

        //TODO: Implement session log logic here...

    }

    /// <summary>
    /// This overload is the same as Log() except it allows you to log the message to another string that you pass in.
    /// </summary>
    /// <param name="message_passed"></param>
    /// <param name="logString_passed"></param>
    /// <param name="logType_passed"></param>
    /// <param name="logDestination_passed"></param>
    public static void Log( string message_passed, ref string logString_passed, PV_LogFormatting logType_passed = PV_LogFormatting.Standard, PV_LogDestination logDestination_passed = PV_LogDestination.Hidden )
    {
        Log(message_passed, logType_passed, logDestination_passed);

        if (logString_passed == null)
            logString_passed = string.Empty;

        logString_passed += message_passed + Environment.NewLine;
    }

    /// <summary>
    /// Overload meant for cases where I want my code to only log to the console on a conditional basis. This will still always print to the session log, but allow you to not print to console when you 
    /// don't want undesired console log statements. Basically, use this for methods where you want logs within the method to only print to the console when you're specifically debugging the script/object 
    /// they belong to.
    /// </summary>
    /// <param name="message_passed"></param>
    /// <param name="logType_passed"></param>
    /// <param name="logToAllConditional_passed"></param>
    public static void LogWithConsoleConditional(string message_passed, bool logToAllConditional_passed, PV_LogFormatting logType_passed = PV_LogFormatting.Standard )
    {
        Log( message_passed, logType_passed, logToAllConditional_passed ? PV_LogDestination.Everywhere : PV_LogDestination.Hidden);
    }

    public static void LogError( string message_passed )
    {
        Debug.LogError( message_passed );

        if( Application.isPlaying )
        {
            Instance.momentaryDebugLogger_cached.LogMomentarily( message_passed );
        }

        Log( message_passed, PV_LogFormatting.Standard, PV_LogDestination.Hidden );
    }

    public static void LogWarning( string message_passed, bool logToMomentaryCanvas_passed = true )
    {
        Debug.LogWarning( message_passed );

		if (Application.isPlaying)
        {
			Instance.momentaryDebugLogger_cached.LogMomentarily( message_passed );
        }

        Log(message_passed, PV_LogFormatting.Standard, PV_LogDestination.Hidden);
    }

}
