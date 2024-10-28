using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_Enemy_basicFlyer", fileName = "stats_Enemy_basicFlyer")]
public class Stats_Enemy_basicFlyer : ScriptableObject
{
    [Header("[----------MOVEMENT----------]")]
    [Tooltip("Speed during charge attack.")]
    public float Speed_swoopng = 13f;

    [Header("[----------ANIMATION----------]")]
    public string AcString_Flying = "b_Walking";
    public string AcString_Running = "b_Running";
    public string AcString_Attack = "t_Attack";
    public string AcString_AttackChoice = "i_AttackChoice";
    public string AcString_TakingDamage = "t_TakingDamage";
    public string AcString_Dying = "t_Dying";
    /// <summary>This integer decides the random damage clip to play.</summary>
    public string AcString_i_DamageClip = "i_DamageClip";

    [Header("[----------ATTACKING----------]")]
    public PV_Attack Attack_basicStrike;
    public PV_Attack Attack_charge;
}
