using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PV_Enums;
using PV_Utils;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class PV_Inventory : PV_Object
{
    public ItemSlot[] MyItemSlots;
    [Space(10f)]

    private Gun_fullyAutomaticRifle fullAutoRifle;
    public Gun_fullyAutomaticRifle FullAutoRifle 
    {
        get { return fullAutoRifle; }
        set { fullAutoRifle = value; }
    }

	//public Gun_pistol pistol;
	//public Gun_pistol Pistol => pistol;

	private void Awake()
	{
        PV_GameManager.Instance.InventoryScript = this;
	}

	private void Start()
    {
        LogHistoric("Start()", PV_LogFormatting.UnityAPIMethod);

        //PV_GameManager.Event_OnPickup.AddListener(Pickup);

        LogHistoric("Start() end", PV_LogFormatting.UnityAPIMethod);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pickupObject_passed"></param>
    /// <returns></returns>
    public bool CanPickup( PickupObject pickupObject_passed )
    {
        LogHistoric( $"CanPickup(pickupObject_passed: '{pickupObject_passed.name}')", PV_LogFormatting.UserMethod );

        if ( pickupObject_passed.NumberHeld <= 0 )
        {
            PV_Debug.LogError( $"VERTIGO ERROR! This pickupObject: '{pickupObject_passed.name}' has a numberheld value of: '{pickupObject_passed.NumberHeld}', which will cause problems. Returning early..." );
            return false;
        }

        if (pickupObject_passed.MyItemData == null)
        {
            PV_Debug.LogError($"VERTIGO ERROR! This pickupObject: '{pickupObject_passed.name}' has null itemdata, which will cause problems if added to the inventory. Returning early...");
            return false;
        }
        else if (pickupObject_passed.MyItemData.count_maxPerSlot <= 0)
        {
            PV_Debug.LogError($"VERTIGO ERROR! This pickupObject: '{pickupObject_passed.name}' has a maxperslot value of: '{pickupObject_passed.MyItemData.count_maxPerSlot}', which will cause problems. Returning early...");
            return false;
        }
        else if (pickupObject_passed.MyItemData.itemName == ItemName.empty)
        {
            PV_Debug.LogWarning($"Attempted to pickup an object with an empty name on gameObject: '{name}', which will cause problems. Returning early...");
            return false;
        }

        else if (pickupObject_passed.MyItemData.count_maxAllowedInInventory == 0) //Note: I'm checking if equal to zero rather than less-than-or-equal-to because I want -1 to mean unlimited...
        {
            PV_Debug.LogError($"VERTIGO WARNING! This pickupObject: '{pickupObject_passed.name}' has a count_maxAllowedInInventory value of: '{pickupObject_passed.MyItemData.count_maxAllowedInInventory}', which will cause problems. Returning early...");
            return false;
        }

        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            if( itmSlot.MyItemData == null || itmSlot.MyItemData.itemName == ItemName.empty || 
                (itmSlot.MyItemData.itemName == pickupObject_passed.MyItemData.itemName && itmSlot.NumberHeld < pickupObject_passed.MyItemData.count_maxPerSlot) )
            {
                LogHistoric($"Found that we can, indeed, pick this item up.");
                return true;
            }
        }

        LogHistoric($"Looped through entire inventory and decided that I'm too full to pick this item up.");
        return false;
    }

    public void Pickup( PickupObject pickupObject_passed )
    {
        LogHistoric($"Inventory.Pickup(pickupObject_passed: '{pickupObject_passed.name}')", PV_LogFormatting.UserMethod);
        print($"inventory pickup action");
        bool amKosher = false;
        while ( !amKosher )
        {
            LogHistoric($"whiling...", PV_LogFormatting.Standard );

            ItemSlot itmSlot = GetItemSlotOfExistingItemInInventory( pickupObject_passed.MyItemData.itemName, true );
            if ( itmSlot == null )
            {
                itmSlot = GetFirstEmptyItemSlot();
            }

            if ( itmSlot != null )
            {
                LogHistoric($"Settled on item slot: '{itmSlot.name}'");
                int remaining = pickupObject_passed.NumberHeld - itmSlot.Pickup(pickupObject_passed);
                if (remaining <= 0)
                {
                    LogHistoric($"remaining space is 0. Breaking...");
                    amKosher = true;
                }
            }
            else
            {
                LogHistoric($"couldn't find appropriate item slot.");
                amKosher = true;
                return; //this return prevents the following event to be invoked if it doesn't need to be
            }
        }

        PV_GameManager.Event_OnItemPickedUp.Invoke( pickupObject_passed );
    }

    public bool ExistsInInventory( ItemName itemName_passed )
    {
        LogHistoric( $"Inventory.ExistsInInventory(itemName_passed: '{itemName_passed}')", PV_LogFormatting.UserMethod );

        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            LogHistoric($"foreaching: '{itmSlot}'", PV_LogFormatting.UserMethod);
            if ( itmSlot.MyItemData != null && itmSlot.MyItemData.itemName == itemName_passed )
            {
                LogHistoric($"Found this item in inventory.", PV_LogFormatting.Standard);
                return true;
            }
        }

        PV_Debug.Log($"Didn't find this item in inventory.");

        return false;
    }

    /// <summary>
    /// Returns the slot where the passed item exists in inventory. Returns null if no slot contains the passed item.
    /// </summary>
    /// <param name="data_passed"></param>
    /// <param name="excludeFullSlots">This optionally excludes item slots that have the passed item, but are completely full.</param>
    /// <returns></returns>
    public ItemSlot GetItemSlotOfExistingItemInInventory( ItemName itemName_passed, bool excludeFullSlots )
    {
        LogHistoric($"Inventory.GetItemSlotOfExistingItemInInventory(itemName_passed: '{itemName_passed}')", PV_LogFormatting.UserMethod);

        ItemSlot returnItemSlot = null;
        int runningSmallestAmt = int.MaxValue;
        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            if ( itmSlot.MyItemData != null && itmSlot.MyItemData.itemName == itemName_passed && 
                itmSlot.NumberHeld > 0 && itmSlot.NumberHeld < runningSmallestAmt )
            {
                if ( !excludeFullSlots || itmSlot.NumberHeld < itmSlot.MyItemData.count_maxPerSlot )
                {
                    LogHistoric( $"Found itemslot: '{itmSlot.name}' where '{itemName_passed}' already exists." );
                    returnItemSlot = itmSlot;
                    runningSmallestAmt = itmSlot.NumberHeld;
                }
            }
        }

        if ( returnItemSlot == null )
        {
            LogHistoric($"Did NOT find this item in inventory.");
        }

        return returnItemSlot;
    }

    /// <summary>
    /// Returns the total amount of the passed item that exist in the inventory across all the inventory slots.
    /// </summary>
    /// <param name="itemName_passed"></param>
    /// <returns></returns>
    public int TotalAmountInInventory( ItemName itemName_passed )
    {
        LogHistoric( $"Inventory.TotalAmountInInventory(itemName_passed: '{itemName_passed}')", PV_LogFormatting.UserMethod );
        int amt = 0;
        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            LogHistoric($"foreaching: '{itmSlot}'. data null: '{itmSlot.MyItemData == null}' itemname: '{(itmSlot.MyItemData == null ? "n/a" : itmSlot.MyItemData.itemName)}'");
            if ( itmSlot.MyItemData != null && itmSlot.MyItemData.itemName == itemName_passed )
            {
                amt += itmSlot.NumberHeld;
                LogHistoric($"itemslot was a match with '{itmSlot.NumberHeld}' count. return amount now: '{amt}'");
            }
        }
        LogHistoric($"Found total to be: '{amt}'");

        return amt;
    }

    /// <summary>
    /// Attempts to use the amount requested of this item.
    /// </summary>
    /// <param name="itemName_passed">Name of the item to attempt to use.</param>
    /// <param name="amountRequested_passed">Amount requested to use.</param>
    /// <returns></returns>
    public int ConsumeItemByAmount( ItemName itemName_passed, int amountRequested_passed )
    {
        LogHistoric($"ConsumeItemByAmount(requested: '{amountRequested_passed}' of: '{itemName_passed}' )", PV_LogFormatting.UserMethod);
        int amt = 0;
        
        if( ExistsInInventory(itemName_passed) )
        {
            bool isKosher = false;
            while(!isKosher)
            {
                LogHistoric($"whiling... ");

                int amt_fetched = GetItemSlotOfExistingItemInInventory(itemName_passed, false).UseItem(amountRequested_passed-amt);
                amt += amt_fetched;
                LogHistoric($"fetched: '{amt}'. ");

                if ( amt >= amountRequested_passed || !ExistsInInventory(itemName_passed) )
                {
                    isKosher = true;
                    LogHistoric($"Found that I have either exhausted item in inventory, or have retrieved the amount requested, and am finished with while loop...");
                }
            }
        }

        return amt;
    }

    public bool AttemptToUseItem( ItemSlot itemSlot_passed )
    {
        LogHistoric( $"AttemptToUseItem('{itemSlot_passed.name}')", PV_LogFormatting.UserMethod, true );
        if( itemSlot_passed.MyItemData.MyUseMethod == UseMethod.None )
        {
            PV_GameManager.Instance.CanvasScript_inGame.Txt_resultOfUseAttempt.text = "This item cannot be used like this.";
            return false;
        }
        else
        {
            if( itemSlot_passed.MyItemData.GeneralCategory == GeneralItemCategory.health )
            {
                print("health");
                if( PV_GameManager.Instance.PlayerScript.HP < 100 )
                {
                    print("using");
                    PV_GameManager.Instance.PlayerScript.Heal( itemSlot_passed.MyItemData.Amt_heal );
                    itemSlot_passed.ClearMe();
                    return true;
                }
                else
                {
                    print("not using");
                    PV_GameManager.Instance.CanvasScript_inGame.Txt_resultOfUseAttempt.text = "Already at full health.";
                }
            }
            else if( itemSlot_passed.MyItemData.GeneralCategory == GeneralItemCategory.key )
            {

                return false;
            }
        }

        return false;
    }

    public ItemSlot GetFirstEmptyItemSlot()
    {
        LogHistoric($"GetFirstEmptyItemSlot()", PV_LogFormatting.UserMethod);

        foreach (ItemSlot itmSlot in MyItemSlots)
        {
            LogHistoric($"foreaching: '{itmSlot}'");

            if (itmSlot.MyItemData == null )
            {
                LogHistoric($"found this item slot is null. Returning this item slot...");
                return itmSlot;
            }
        }

        LogHistoric($"Didn't find any item slots with null data. Returning...");
        return null;
    }

    #region UI---------------------
    public void DisableInventoryButtons()
    {
        print($"DisableInventoryButtons(). count: '{MyItemSlots.Length}'");

        foreach ( ItemSlot slot in MyItemSlots )
        {
            slot.DisableMyClickFunctionality();
        }
    }

    public void EnableInventoryButtons()
    {
        print($"EnableInventoryButtons(). count: '{MyItemSlots.Length}'");

        foreach ( ItemSlot slot in MyItemSlots )
        {
            if( slot.MyItemData != null )
            {
                slot.EnableMyClickFunctionality();

            }
        }
    }
    #endregion

    [ContextMenu("z call OrderInventorySlots()")]
    public void OrderInventorySlots()
    {
        PV_Debug.Log( $"{nameof(OrderInventorySlots)}()" );
        //Debug.LogWarning("PV WARNING! This currently will only work properly if every row has the same amount of cells/children. I need to implement this functionality in the future and am leaving this note to remind myself to do that.");

        Button[] btns = new Button[MyItemSlots.Length];


		for ( int i = 0; i < MyItemSlots.Length; i++ )
		{
            btns[i] = MyItemSlots[i].transform.Find("btn_ItemSlot").GetComponent<Button>();
		}

		PV_Utilities.SetNavigationOnGridLayoutGroupButtons( 
            MyItemSlots[0].transform.parent.GetComponent<GridLayoutGroup>(), btns 
            );
        //EditorSceneManager.MarkSceneDirty( SceneManager.GetActiveScene() ); 
	}
}