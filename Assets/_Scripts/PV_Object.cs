using PV_Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PV_Object : MonoBehaviour
{
    [SerializeField, TextArea(1, 8)] protected string historyDebugString;

	protected void Log( string message_passed, PV_LogDestination destination_passed = PV_LogDestination.Hidden, PV_LogFormatting format_passed = PV_LogFormatting.Standard )
	{
		PV_Debug.Log(
			$"[{name}] {message_passed}",
			format_passed, destination_passed);

		historyDebugString += message_passed + "\n";
		
		if (historyDebugString.Length > 5000)
		{
			historyDebugString = string.Empty;
		}
	}

	protected void LogWarning(string message_passed, PV_LogDestination destination_passed = PV_LogDestination.Hidden, PV_LogFormatting format_passed = PV_LogFormatting.Standard)
	{
		PV_Debug.LogWarning( $"[{name}] {message_passed}" );

		historyDebugString += message_passed + "\n";

		if (historyDebugString.Length > 5000)
		{
			historyDebugString = string.Empty;
		}
	}

	protected void Log_MethodStart( string message_passed, PV_LogDestination destination_passed = PV_LogDestination.Hidden, PV_LogFormatting format_passed = PV_LogFormatting.Standard )
	{
		//In the future this method could be used to start a userAPI method by increasing the tab level
		LogInc( message_passed, destination_passed, format_passed );
	}

	protected void LogInc( string message_passed, PV_LogDestination destination_passed = PV_LogDestination.Hidden, PV_LogFormatting format_passed = PV_LogFormatting.Standard )
	{
		//In the future this method could be used to start a userAPI method by increasing the tab level
		Log(message_passed, destination_passed, format_passed);
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
