using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Utils;

[CreateAssetMenu(menuName = "VertigoObjects/Stats_Enemy_bug", fileName = "stats_enemy_bug")]
public class Stats_Enemy_bug : ScriptableObject
{
    [Header("[----------MOVEMENT----------]")]
    [Tooltip("Speed during charge attack.")]
    public float Speed_charging = 13f;

    [Header("[----------ATTACKING----------]")]
    public PV_Attack Attack_basicStrike;
    public PV_Attack Attack_charge;
	[Tooltip("Height, beyond which, this enemy will go from engaging to chasing")]
	public float Height_TriggerChasing_fromEngaging = 2f; //todo: is this being used? Actually, I think this is so that the bug enemies can decide if they need to crawl up a ledge further to attack...

	[Header("[----------ANIMATION----------]")]
    public string AcString_Walking = "b_Walking";
    public string AcString_Running = "b_Running";
    public string AcString_Attack = "t_Attack";
    public string AcString_AttackChoice = "i_AttackChoice";
    public string AcString_TakingDamage = "t_TakingDamage";
    public string AcString_Dying = "t_Dying";
    /// <summary>This integer decides the random damage clip to play.</summary>
    public string AcString_i_DamageClip = "i_DamageClip";
}
