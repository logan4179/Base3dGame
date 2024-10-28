using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PV_Attack
{
    public bool AmRanged;
    public float Reach;

    public int Damage;
    public float Force;
    [Range(0f,100f)] public float Likelihood;

    [Header("AMINATION")]
    public int AnimId;
    [Tooltip("This is for the animator sub-state to make a choice.")]
    public int AnimChoiceNumber;
}
