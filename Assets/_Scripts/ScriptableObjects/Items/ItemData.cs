using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Enums;

[CreateAssetMenu(menuName = "VertigoObjects/ItemData", fileName = "itemData")]
public class ItemData : ScriptableObject
{
    [Header("---------------[[ GENERAL ]]-----------------")]
    public ItemName itemName;
    /// <summary>This is currently used for making quick and efficient decisions about what to do when picking up an item in certain occasions.</summary>
    public GeneralItemCategory GeneralCategory;
    public UseMethod MyUseMethod;
    [Space(5f)]

    [Header("---------------[[ INVENTORY INTERACTION ]]-----------------")]
    public int count_maxAllowedInInventory; //currently I don't think logic is implemented to take this into account. I may not want to restrict the amount allowed in the inventory.
    public int count_maxPerSlot;
    /// <summary>Icon to visually represent the item in the inventory part of the canvas UI.</summary>
    public Sprite ItemSprite;
    /// <summary>This will show up in the UI container 'ctr_SelectedItemInfo' when the player makes this item active.</summary>
    public string ItemDescription;

    [Space(5f)]

    [Header("---------------[[ HEALTH ITEMS ONLY ]]-----------------")]
    public int Amt_heal;

}
