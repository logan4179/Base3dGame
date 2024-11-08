using System; //todo: dwf with typeof stuff
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptObject : Base_InteractiveObject
{
    public List<string> PromptMessages;
    /// <summary>Keeps track of the index of the messasge we're on. </summary>
    [HideInInspector] public int Index_PromptMessages;

    private void Start()
    {
        Index_PromptMessages = -1;
    }

    private void OnTriggerStay( Collider other )
    {
        if ( other.CompareTag("Player") )
        {
            if ( AmValidForPlayerFocus() ) //returns true only if this has been newly assigned as active interactable.
            {
                FocusOnMe();
            }
        }

        DBG_all = $"triggering with: '{other.tag}'\n" +
            $"{nameof(Index_PromptMessages)}: '{Index_PromptMessages}'\n" +
            $"";
    }

    private void OnTriggerExit( Collider other )
    {
        if ( other.CompareTag("Player") )
        {
            BaseTriggerExitAction();

        }
    }
}
