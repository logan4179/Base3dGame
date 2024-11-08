using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Enums;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_gun", fileName = "stats_gun")]
public class Stats_gun : ScriptableObject
{
    public ItemName MyItemName;
    public ItemName itemName_myAmmo;

    public int Amount_Damage;
    public int Force_Damage;

    [Tooltip("How much recoil a single shot produces."), Range(0f,0.02f)]
    public float Amount_Recoil;

    [Tooltip("How many times the gun can fire per second. Only applies to full-auto. Set to -1 if not full-auto.")]
    public int fireRatePerSecond = -1;
    public int count_singleClipCapacity;
    public float dist_BulletRaycast;
    public float duration_cd_DrawGun, duration_Light_MuzzleFlash_On;

    [Header("SOUNDS")]
    [SerializeField] public AudioClip audioClip_fireBullet;
    [SerializeField] public AudioClip audioClip_click, audioClip_reload, audioClip_switchFireMode;

    [Header("[----------ANIMATION----------]")]
    public string AcString_WeaponDrawn = "b_WeaponDrawn";
    public string AcString_Recoil = "t_recoil";
}
