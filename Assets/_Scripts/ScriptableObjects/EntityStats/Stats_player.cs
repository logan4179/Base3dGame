using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_player", fileName = "stats_player")]
public class Stats_player : ScriptableObject
{
    [Header("[----------MOVEMENT----------]")]
    [Tooltip("This is the max speed that normal, in-game movement can be. In this case, 'normal' means that the character is not fettered by a condition like the gun being drawn, or the game slowing the character down on purpose to look at something")] 
    public float moveSpeed_unfettered = 7f;
    [Tooltip("This is the max speed the player can move when the game wants him to be in a 'fettered' state, such as when his gun is drawn or the game wants to slow him down to look at something")]
    public float MoveSpeed_fettered = 2.1f;

    [Tooltip("Speed that the move speed multiplier gets lerped by to create gradual movement.")]
    public float LerpSpeed_IndependentMoveInertia;

    [Header("[----------VISION----------]")]
    /// <summary>Used in the player script to determine how quickly the visibilityIndex lerps towards it's target.</summary>
    public float visLerpSpeed = 5f;


    [Header("[----------ANIMATION----------]")]
    public string AcString_GunIsDrawn = "b_GunIsDrawn";
    public string AcString_TakeDamageTrigger = "t_TakeDamage";
    public string AcString_ReloadTrigger = "t_Reloading";
    public string AcString_WeaponMode = "i_weaponMode";
    public string AcString_EquipWeaponTrigger = "t_equipWeapon";
    public string AcString_Airborn = "b_Airborn";
    public string AcString_Travelling = "b_AmTravelling";
    public string AcString_MoveSpeed_straight = "f_MoveSpeed_straight";
    public string AcString_MoveSpeed_side = "f_MoveSpeed_side";
    public string AcString_Die = "t_Die";

    [Header("[---------- AUDIO ----------]")]
    public List<AudioClip> AudioClips_Footstep_ConcreteA;

    [Header("[----------OTHER----------]")]
    public float distance_laserSight;

}
