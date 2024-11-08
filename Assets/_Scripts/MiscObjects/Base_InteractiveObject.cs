using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Enums;

/// <summary>Base class for interactive objects found in the environment, including pickups and prompts.</summary>
public class Base_InteractiveObject : MonoBehaviour
{
    protected Transform t;
    [SerializeField] private Collider triggerCollider;
    [Tooltip("Allows for quick identification for what type of interactive object an instance of the Base_interactiveObject class is supposed to represent.")]
    public InteractiveObjectType MyType;

    /// <summary>Angle that determines whether a player is considered facing an interactable object. Compared against an inversetransformpoint vector, so it will need to be pretty small</summary>
    [SerializeField] private float angle_interactionThreshold = 0.3f;


    protected Vector3 vITP;

    [SerializeField, TextArea(1,5)] protected string DBG_all;

    private void Awake()
    {
        t = GetComponent<Transform>();
    }

    /// <summary>Performs common action of checking if this should be the active, interactable object, and sets the players' active interactive object, and the canvas' prompt button active if true</summary>
    /// <param name="other"></param>
    /// <returns>true if it has just assigned this object to be the active interactable, false if not.</returns>
    protected bool AmValidForPlayerFocus()
    {
        vITP = PV_GameManager.Instance.Trans_PlayerPerspective.InverseTransformPoint( t.position ).normalized;
        DBG_all += $"{nameof(vITP)}: '{vITP}'\n";
        if ( vITP.z < 0f || PV_GameManager.Instance.PlayerScript.GunIsDrawn ) //In this case, the interactive object is behind us.
        {
            if (PV_GameManager.Instance.PlayerScript.InteractiveObject_focused == this)
            {
                RemoveFocusOnMe();
            }
            return false;
        }

        //This makes the threshold more forgiving the closer the player is to this object.
        float angleThreshold_calculated = Mathf.Lerp( 1f, angle_interactionThreshold, vITP.magnitude / triggerCollider.bounds.extents.magnitude );
        DBG_all += $"{nameof(angleThreshold_calculated)}: '{angleThreshold_calculated}'\n";
        if ( Mathf.Abs(vITP.x) < angleThreshold_calculated )
        {
            if ( PV_GameManager.Instance.PlayerScript.InteractiveObject_focused == null || (PV_GameManager.Instance.PlayerScript.InteractiveObject_focused != this && Mathf.Abs(PV_GameManager.Instance.PlayerScript.InteractiveObject_focused.vITP.x) > Mathf.Abs(vITP.x)) )
            {
                return true;
            }
        }
        else if (PV_GameManager.Instance.PlayerScript.InteractiveObject_focused == this)
        {
            RemoveFocusOnMe();
        }
        
        return false;
    }

    protected void BaseTriggerExitAction()
    {
        if ( PV_GameManager.Instance.PlayerScript.InteractiveObject_focused == this )
        {
            RemoveFocusOnMe();
        }

        DBG_all = "";
    }

    protected void FocusOnMe()
    {
        PV_GameManager.Instance.CanvasScript_inGame.ActivateInteractionIndicator();
        PV_GameManager.Instance.PlayerScript.InteractiveObject_focused = this;
    }

    void RemoveFocusOnMe()
    {
        PV_GameManager.Instance.PlayerScript.InteractiveObject_focused = null;
        PV_GameManager.Instance.CanvasScript_inGame.DeactivateInteractionIndicator();
    }
}
