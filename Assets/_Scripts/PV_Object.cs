using PV_Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PV_Object : MonoBehaviour
{
    [SerializeField, TextArea(1, 8)] protected string historyDebugString;

    public void LogHistoric( string message_passed, PV_LogFormatting format_passed = PV_LogFormatting.Standard, bool consoleConditional = false )
    {
        if (format_passed == PV_LogFormatting.UnityAPIMethod)
        {
            string s = $"////////---------{message_passed}----------////////\n";
            historyDebugString += s;

            //If this is a UnityAPI method, I want to always write to the session log in addition to the history string...

        }
        else if (format_passed == PV_LogFormatting.UserMethod)
        {
            historyDebugString += $"[------ {message_passed} ------]\n";
        }
        else
        {
            historyDebugString += $"{message_passed}\n";
        }

        PV_Debug.Log( $"{ System.DateTime.Now}_{name}.{message_passed}", format_passed );

        if ( historyDebugString.Length > 5000 )
        {
            historyDebugString = string.Empty;
        }

        if( consoleConditional )
        {
            Debug.Log($"{message_passed}");
        }
    }

    public virtual void DrawMyGizmos()
    {

    }

    protected void WriteHistoryToFile()
    {
        //throw NotImplementedException();
    }

	public virtual bool CheckIfKosher()
    {
        return true;
    }
}
