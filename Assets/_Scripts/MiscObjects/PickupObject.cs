using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PickupObject : Base_InteractiveObject
{
    public ItemData MyItemData;
    public int NumberHeld;


    private void OnTriggerStay(Collider other)
    {
        DBG_all = "";
        string s_append = "";
        if ( other.CompareTag("Player") )
        {
            s_append = "triggering with Player...\n"; 
            if ( AmValidForPlayerFocus() )
            {
				s_append = "am valid for player focus...";

				FocusOnMe();
            }
        }
        DBG_all += s_append;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BaseTriggerExitAction();

        }
    }

    public void Pickup( int amt_passed )
    {
        NumberHeld -= amt_passed;

        if ( NumberHeld <= 0 )
        {
            print($"Exhausted pickupObject amount. Destroying...");
            PV_GameManager.Instance.PlayerScript.InteractiveObject_focused = null;
            PV_GameManager.Instance.CanvasScript_inGame.DeactivateInteractionIndicator();
            Destroy( this.gameObject );
        }
    }
}
