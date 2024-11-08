using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LogansUINavigator;
using PV_Enums;

/// <summary>
/// Represents an item slot in the inventory. These are created by assigning one of these as a component to an item slot on a canvas, then they are referenced through a list on the Inventory object in the Player.
/// </summary>
public class ItemSlot : PV_Object
{
    public ItemData MyItemData;
    /// <summary>Keeps track of how many of this item still remain in this slot.</summary>
    public int NumberHeld;

    [Header("---------------[[ REFERENCE (internal) ]]-----------------")]
    /// <summary>Link the appropriate button image component in the heirarchy to this reference.</summary>
    [SerializeField] private Image _img;
    [SerializeField] private TextMeshProUGUI _txt_DescriptionDisplay, _txt_NumberHeld;
    [SerializeField] private GameObject _go_grp_options;
    [SerializeField] private Button myButtonComponent;
    [SerializeField] private Button btn_use;
    [Space(10f)]

    [Header("---------------[[ REFERENCE (external) ]]-----------------")]
    [SerializeField] private PV_Inventory inventory;
    [SerializeField] private LCN_SelectHandler lcn_SelectHandler;
    [SerializeField] private LCN_Action lcnAction_useItem;
    [SerializeField] private LCN_Listener lcnListener_options;
    [Space(10f)]


    private static bool amDebuggingScript_static = false;


    private void Start()
    {
        Log_MethodStart("Start()");
        if ( inventory == null )
        {
            Debug.LogError($"VERTIGO ERROR! This itemslot is missing its inventory reference! Returning early...");
            return;
        }

        if( MyItemData != null && MyItemData.itemName != ItemName.empty )
        {
            _txt_NumberHeld.text = NumberHeld.ToString();
            _img.color = Color.white;
            _img.sprite = MyItemData.ItemSprite;
            if( MyItemData.MyUseMethod == UseMethod.None )
            {
                btn_use.gameObject.SetActive(false);
            }
        }
        else
        {
            DisableMyClickFunctionality();
            _txt_NumberHeld.text = "";
        }

        _go_grp_options.SetActive( false );

    }

    public void ClearMe()
    {
        Log_MethodStart($"ClearMe()");

        MyItemData = null;
        NumberHeld = 0;
        _txt_DescriptionDisplay.text = "";
        _txt_NumberHeld.text = "";
        _img.sprite = null;
        _img.color = Color.clear; //Yes, this is necessary. Otherwise, it will show asa a white square!
        _go_grp_options.SetActive( false );
        myButtonComponent.enabled = false;
    }

    public void ClickMe()
    {
        // todo: might want to implement this method in the future for when the itemslot button is clicked if using the buttons' onclick event in the unity inspector is insufficent.
    }

    public void MainButtonAction()
    {
        Log_MethodStart($"MainButtonAction()");

        if ( MyItemData != null )
        {
            if ( _go_grp_options.activeSelf )
            {
                _go_grp_options.SetActive(false);
                inventory.DisableInventoryButtons();
                PV_GameManager.Instance.CanvasScript_inGame.DisableTopInventoryCanvasButtons();
                lcnListener_options.MakeMeCurrentListener();
            }

        }
    }

    public void AttemptToUseMyItem()
    {
        Log_MethodStart($"'{name}'.UseMyItem(). currently holding: '{MyItemData.itemName}'");
        if( MyItemData == null || MyItemData.itemName == ItemName.empty )
        {
            PV_Debug.LogError($"VERTIGO ERROR! Item's paramters are not set valid for use.");
            return;
        }

        if( inventory.AttemptToUseItem(this) )
        {
            lcnAction_useItem.Execute();
        }
    }

    public int Pickup( PickupObject pickupObject_passed )
    {
        Log_MethodStart( $"Pickup('{pickupObject_passed.MyItemData.itemName}'). NumberHeld: '{NumberHeld}', requested pickup amount: '{pickupObject_passed.NumberHeld}'," +
            $"slot item data null?: '{MyItemData == null}', pickup item data null?: '{pickupObject_passed.MyItemData == null}'" );
        MyItemData = pickupObject_passed.MyItemData;
        myButtonComponent.enabled = true;
        if( MyItemData.MyUseMethod != UseMethod.None )
        {
            btn_use.enabled = true;
        }
        _img.sprite = pickupObject_passed.MyItemData.ItemSprite;
        if( MyItemData.MyUseMethod == UseMethod.None )
        {
            btn_use.gameObject.SetActive( false );
        }
        else
        {
            btn_use.gameObject.SetActive ( true );
        }
        _img.color = Color.white;
        int amt = 0;
        if( (pickupObject_passed.NumberHeld + NumberHeld) < pickupObject_passed.MyItemData.count_maxPerSlot )
        {
            Log("had more than enough room to pickup this amount");
            amt += pickupObject_passed.NumberHeld ;
        }
        else
        {
            amt += ( pickupObject_passed.MyItemData.count_maxPerSlot - NumberHeld );
            Log($"only had enough room to pickup '{amt}', had to leave some behind.");

        }
        Log($"final caluclated amount to pick up: '{amt}'");
        NumberHeld += amt;
        pickupObject_passed.Pickup(amt);

        _txt_NumberHeld.text = NumberHeld.ToString();
        EnableMyClickFunctionality();
        return amt;
    }

    /// <summary>
    /// Attempts to use the amount requested of this item. Clears this item slot if it uses all of the number held in this item slot.
    /// </summary>
    /// <param name="amountRequested_passed">How many of this item is being requested. For key items, this will typically be 1, 
    /// but for a gun requesting ammunition via reload() it could be more.</param>
    /// <returns>How many of this item it was succesfully able to pickup.</returns>
    public int UseItem( int amountRequested_passed )
    {
        PV_Debug.LogWithConsoleConditional( $"useitem(amountRequested_passed: '{amountRequested_passed}')", amDebuggingScript_static, PV_LogFormatting.UserMethod );
        int amt = 0;

        if( amountRequested_passed < NumberHeld )
        {
            if (amDebuggingScript_static) print($"Item was able to return the full amount passed ('{amountRequested_passed}').");
            amt = amountRequested_passed;
            NumberHeld -= amt;
            _txt_NumberHeld.text = NumberHeld.ToString();

        }
        else
        {
            if (amDebuggingScript_static) print($"More was requested than was available. Using what is left ('{NumberHeld}')...");
            amt = NumberHeld;
            ClearMe();
        }

        return amt;
    }

    /// <summary>All the tasks to perform when this slot is highlighted.</summary>
    public void Highlight_action()
    {
        if ( MyItemData != null && MyItemData.itemName != ItemName.empty )
        {
            _txt_DescriptionDisplay.text = MyItemData.ItemDescription;

        }
        else if ( MyItemData == null || MyItemData.itemName == ItemName.empty )
        {
            _txt_DescriptionDisplay.text = string.Empty;
        }
    }

    public void Unhighlight_action()
    {
        _txt_DescriptionDisplay.text = string.Empty;

    }

    public void DisableMyClickFunctionality()
    {
        myButtonComponent.enabled = false;
        lcn_SelectHandler.enabled = false;
    }

    public void EnableMyClickFunctionality()
    {
        myButtonComponent.enabled = true;
        lcn_SelectHandler.enabled = true;

    }

    public void CancelButtonAction()
    {
        _go_grp_options.SetActive( false );
        PV_GameManager.Instance.CanvasScript_inGame.EnableTopInventoryCanvasButtons();
        PV_GameManager.Instance.CanvasScript_inGame.Txt_resultOfUseAttempt.text = string.Empty;
        inventory.EnableInventoryButtons();
       
    }

    public void SayHay()
    {
        print("hay");
    }
}