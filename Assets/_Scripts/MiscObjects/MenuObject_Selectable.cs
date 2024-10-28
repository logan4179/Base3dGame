using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>Class for handling menu container objects that move player through menus. For handling items in an inventory, see InventoryItem.cs</summary>
public class MenuObject_Selectable : MonoBehaviour,ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>Allows you to deactivate controlled container on init</summary>
    public bool DeactivateControlledOnEnable;
    public RawImage ri_Controlled;
    public Texture texture_Controlled;
    public TextMeshProUGUI txt_Controlled;
    public string string_Controlled;
    public Canvas compCanvas;

    [Header("--------------CLICK----------------")]
    public List<GameObject> GOs_toActivate_OnClick;
    public List<GameObject> GOs_toDeactivate_OnClick;
    public GameObject go_SetFocus_OnClick;
    public UnityEvent OnClick_UserAdded;

    [Header("--------------SELECT-----------------")]
    public List<GameObject>GOs_toActivate_OnSelect;
    public List<GameObject> GOs_toDeactivate_OnSelect;
    public UnityEvent OnSelect_UserAdded;

    void OnEnable()
    {
        //Debug.Log($"OnEnable for: '{gameObject.name}'");

        if (DeactivateControlledOnEnable && (GOs_toActivate_OnClick != null && GOs_toActivate_OnClick.Count > 0) || (GOs_toDeactivate_OnClick != null && GOs_toDeactivate_OnClick.Count > 0))
        {
            foreach (GameObject g in GOs_toActivate_OnClick)
            {
                g.SetActive(false);
            }
        }
    }

    void Awake()
    {
        //Debug.Log($"Awake for: '{gameObject.name}'");
       
    }

    public void ClickMe()
    {
        bool dbg = true;
        if (dbg) Debug.Log($"<color=white>-------------ClickMe('{gameObject.name}')---------------</color>");

        if( GOs_toActivate_OnClick != null )
        {
            foreach( GameObject go in GOs_toActivate_OnClick )
            {
                go.SetActive(true);
            }
        }

        if( GOs_toDeactivate_OnClick != null )
        {
            bool shouldDeactiaveSelf = false;
            foreach( GameObject g in GOs_toDeactivate_OnClick )
            {
                if (g != gameObject) g.SetActive(false);
                else shouldDeactiaveSelf = true;
            }
            if (shouldDeactiaveSelf) gameObject.SetActive(false); //This allows a MenuObject to deactivate the gameObject it represents when it's UI element (probably a button) is selected.
        }

        if( go_SetFocus_OnClick != null )
        {
            EventSystem.current.SetSelectedGameObject(go_SetFocus_OnClick);
        }

        OnClick_UserAdded.Invoke();
    }
    public void OnSelect(BaseEventData eventData)
    {
        bool dbg = true;
        PV_Debug.LogWithConsoleConditional( $"OnSelect('{gameObject.name}')", dbg, PV_Enums.PV_LogFormatting.UserMethod );

        if( GOs_toActivate_OnSelect != null )
        {
            foreach( GameObject g in GOs_toActivate_OnSelect)
            {
                g.SetActive(true);
            }
        }

        if (GOs_toDeactivate_OnSelect != null)
        {
            foreach (GameObject g in GOs_toDeactivate_OnSelect)
            {
                g.SetActive(true);
            }
        }

        if( txt_Controlled != null )
        {
            txt_Controlled.text = string_Controlled;
        }

        OnSelect_UserAdded.Invoke();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        bool dbg = true;
        PV_Debug.LogWithConsoleConditional( $"OnDeselect('{gameObject.name}')", dbg, PV_Enums.PV_LogFormatting.UserMethod );

        if( GOs_toActivate_OnSelect != null && GOs_toActivate_OnSelect.Count > 0 )
        {
            foreach( GameObject g in GOs_toActivate_OnSelect )
            {
                g.SetActive(false);
            }
        }

        if( txt_Controlled != null )
        {
            txt_Controlled.text = string.Empty;
        }

        if( ri_Controlled != null )
        {
            ri_Controlled.texture = null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool dbg = true;
        PV_Debug.LogWithConsoleConditional($"OnPointerEnter('{gameObject.name}')", dbg, PV_Enums.PV_LogFormatting.UserMethod);

        EventSystem.current.SetSelectedGameObject(gameObject); //This forces the eventsystem to consider this object "selected" upon mouse over. Doing this will also indirectly call OnSelect()
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bool dbg = true;
        PV_Debug.LogWithConsoleConditional($"OnPointerExit('{gameObject.name}')", dbg, PV_Enums.PV_LogFormatting.UserMethod);
    }
}
