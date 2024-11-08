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
        Log_MethodStart("Start()");

        //PV_GameManager.Event_OnPickup.AddListener(Pickup);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pickupObject_passed"></param>
    /// <returns></returns>
    public bool CanPickup( PickupObject pickupObject_passed )
    {
        Log_MethodStart($"CanPickup(pickupObject_passed: '{pickupObject_passed.name}')");

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
                Log($"Found that we can, indeed, pick this item up.");
                return true;
            }
        }

        Log($"Looped through entire inventory and decided that I'm too full to pick this item up.");
        return false;
    }

    public void Pickup( PickupObject pickupObject_passed )
    {
        Log_MethodStart($"Inventory.Pickup(pickupObject_passed: '{pickupObject_passed.name}')");
        print($"inventory pickup action");
        bool amKosher = false;
        while ( !amKosher )
        {
            Log($"whiling..." );

            ItemSlot itmSlot = GetItemSlotOfExistingItemInInventory( pickupObject_passed.MyItemData.itemName, true );
            if ( itmSlot == null )
            {
                itmSlot = GetFirstEmptyItemSlot();
            }

            if ( itmSlot != null )
            {
                Log($"Settled on item slot: '{itmSlot.name}'");
                int remaining = pickupObject_passed.NumberHeld - itmSlot.Pickup(pickupObject_passed);
                if (remaining <= 0)
                {
                    Log($"remaining space is 0. Breaking...");
                    amKosher = true;
                }
            }
            else
            {
                Log($"couldn't find appropriate item slot.");
                amKosher = true;
                return; //this return prevents the following event to be invoked if it doesn't need to be
            }
        }

        PV_GameManager.Event_OnItemPickedUp.Invoke( pickupObject_passed );
    }

    public bool ExistsInInventory( ItemName itemName_passed )
    {
        Log_MethodStart($"Inventory.ExistsInInventory(itemName_passed: '{itemName_passed}')");

        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            Log($"foreaching: '{itmSlot}'");
            if ( itmSlot.MyItemData != null && itmSlot.MyItemData.itemName == itemName_passed )
            {
                Log($"Found this item in inventory.");
                return true;
            }
        }

        Log($"Didn't find this item in inventory.");

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
        Log_MethodStart($"Inventory.GetItemSlotOfExistingItemInInventory(itemName_passed: '{itemName_passed}')" );

        ItemSlot returnItemSlot = null;
        int runningSmallestAmt = int.MaxValue;
        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            if ( itmSlot.MyItemData != null && itmSlot.MyItemData.itemName == itemName_passed && 
                itmSlot.NumberHeld > 0 && itmSlot.NumberHeld < runningSmallestAmt )
            {
                if ( !excludeFullSlots || itmSlot.NumberHeld < itmSlot.MyItemData.count_maxPerSlot )
                {
                    Log( $"Found itemslot: '{itmSlot.name}' where '{itemName_passed}' already exists." );
                    returnItemSlot = itmSlot;
                    runningSmallestAmt = itmSlot.NumberHeld;
                }
            }
        }

        if ( returnItemSlot == null )
        {
            Log($"Did NOT find this item in inventory.");
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
        Log_MethodStart($"Inventory.TotalAmountInInventory(itemName_passed: '{itemName_passed}')");
        int amt = 0;
        foreach ( ItemSlot itmSlot in MyItemSlots )
        {
            Log($"foreaching: '{itmSlot}'. data null: '{itmSlot.MyItemData == null}' itemname: '{(itmSlot.MyItemData == null ? "n/a" : itmSlot.MyItemData.itemName)}'");
            if ( itmSlot.MyItemData != null && itmSlot.MyItemData.itemName == itemName_passed )
            {
                amt += itmSlot.NumberHeld;
                Log($"itemslot was a match with '{itmSlot.NumberHeld}' count. return amount now: '{amt}'");
            }
        }
        Log($"Found total to be: '{amt}'");

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
        Log_MethodStart($"ConsumeItemByAmount(requested: '{amountRequested_passed}' of: '{itemName_passed}' )");
        int amt = 0;
        
        if( ExistsInInventory(itemName_passed) )
        {
            bool isKosher = false;
            while(!isKosher)
            {
                Log($"whiling... ");

                int amt_fetched = GetItemSlotOfExistingItemInInventory(itemName_passed, false).UseItem(amountRequested_passed-amt);
                amt += amt_fetched;
                Log($"fetched: '{amt}'. ");

                if ( amt >= amountRequested_passed || !ExistsInInventory(itemName_passed) )
                {
                    isKosher = true;
                    Log($"Found that I have either exhausted item in inventory, or have retrieved the amount requested, and am finished with while loop...");
                }
            }
        }

        return amt;
    }

    public bool AttemptToUseItem( ItemSlot itemSlot_passed )
    {
        Log_MethodStart( $"AttemptToUseItem('{itemSlot_passed.name}')" );
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
        Log_MethodStart($"GetFirstEmptyItemSlot()");

        foreach (ItemSlot itmSlot in MyItemSlots)
        {
            Log($"foreaching: '{itmSlot}'");

            if (itmSlot.MyItemData == null )
            {
                Log($"found this item slot is null. Returning this item slot...");
                return itmSlot;
            }
        }

        Log($"Didn't find any item slots with null data. Returning...");
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