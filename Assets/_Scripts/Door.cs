using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Enums;

/// <summary>Base class for doors to inherit from</summary>
public class Door : Base_InteractiveObject
{
    [Header("REFERENCE")]
    [SerializeField] Animator animator;

    [SerializeField] AudioClip openAudioClip, closeAudioClip;

    //[Header("TRUTH")]
    public DoorStates MyState = DoorStates.closed;

    public bool AmAutomatic => (MyType == InteractiveObjectType.AutoOpenDoor || MyType == InteractiveObjectType.AutoOpenUpAndDownDoor);
    public bool AmInTransition => (MyState == DoorStates.opening || MyState == DoorStates.closing);

	/// <summary>Returns true if the player is triggering inside this object's trigger collider without considering the angle the player is facing towards.</summary>
	protected bool playerIsCurrentlyTriggeringMe;

	[Header("STATS")]
    [SerializeField] private float checkWaitTime; //todo:


    private void OnTriggerEnter(Collider other)
    {
        if ( other.CompareTag("Player") )
        {
            playerIsCurrentlyTriggeringMe = true;

            if ( AmAutomatic && !AmInTransition && MyState == DoorStates.closed )
            {
                OpenMe();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ( other.CompareTag("Player") )
        {
            if( MyType == InteractiveObjectType.StandardDoor )
            {
                if ( AmValidForPlayerFocus() ) //returns true only if this has been newly assigned as active interactable.
                {
                    FocusOnMe();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ( other.CompareTag("Player") )
        {
            BaseTriggerExitAction();
			playerIsCurrentlyTriggeringMe = false;

			if ( AmAutomatic && !AmInTransition && MyState == DoorStates.open )
            {
                CloseMe();
            }
        }
    }

    public void InteractAction()
    {
        if ( MyState == DoorStates.open )
        {
            CloseMe();
        }
        else
        {
            OpenMe();
        }
    }

    public void OpenMe()
    {
        if ( MyType == InteractiveObjectType.AutoOpenUpAndDownDoor )
        {
            animator.SetTrigger("t_openUp");
            if (openAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(openAudioClip, transform.position);
            }
        }
        else
        {
            animator.SetTrigger("t_open");
        }
        StartCoroutine(CheckDoorState());
    }

    public void CloseMe()
    {
        if (MyType == InteractiveObjectType.AutoOpenUpAndDownDoor)
        {
            animator.SetTrigger("t_closeDown");
            if (closeAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(closeAudioClip, transform.position);

            }
        }
        else
        {
            animator.SetTrigger("t_close");
        }
    }

    IEnumerator CheckDoorState()
    {
        yield return new WaitForSeconds(checkWaitTime);
        if( !playerIsCurrentlyTriggeringMe && MyState == DoorStates.open )
        {
            CloseMe();
        }
    }


    public void AnimEvent_SetToOpened()
    {
        MyState = DoorStates.open;
    }

    public void AnimEvent_SetToClosed()
    {
        MyState = DoorStates.closed;
    }
}
