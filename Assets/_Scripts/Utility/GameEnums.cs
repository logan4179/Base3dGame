using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PV_Enums
{
    public enum GameState
    {
        Unpaused,
        Paused_options,
        Paused_inventory,
        Paused_readingPrompt,
        InCinematic,
    }

    public enum EntityState
    { 
        Alive,
        Dead,
    }

    public enum EntityMovementMode
    {
        Stationary,
        TerrestrialMovement,
        Airborn,
    }

	/// <summary>
	/// Specifies a type of movement.
	/// <para>Idle, Walking, Running, Charging, TurningInPlace, FreeFalling, Sliding, FlyingSlow, FlyingFast</para>
	/// </summary>
	public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Charging,
        TurninngInPlace,
        FreeFalling,
        Sliding,
        FlyingSlow,
        FlyingFast,
    }

    /// <summary>
    /// Method that enemy uses for 'patrol' type movement.
    /// <para>Stationary: No patrolling.</para>
    /// <para>Random: Chooses a random spot inside the radius of a random patrol point.</para>
    /// <para>LinearSequential: Moves from the lowest index of the patrol points list to the largest index of the patrol points list, and back again, following them like a line.</para>
    /// <para>LinearRandom: Will always randomly choose either the next patrol point in the list, or the previous patrol point in the list.</para>
    /// </summary>
    public enum PatrolMode
    {
        Stationary,
        Random,
        LinearSequential,
        LinearRandom,
    }

    public enum PV_Directions
    {
        forward,
        backward,
        left,
        right,
        up,
        down,
    }

    #region ITEMS -----------------------------------------------
    public enum ItemName : ushort
    {
        empty,
        fullAutoRifle,
        pistol,
        ammo_556,
        ammo_9mm,
        ammo_12ga,
        healthPack_small,
        healthPack_medium,
        healthPack_large
    }

    /// <summary>This is currently used for making quick and efficient decisions about what to do when picking up an item in certain occasions.</summary>
    public enum GeneralItemCategory : ushort
    {
        undefined,
        weapon,
        ammunition,
        health,
        key,
    }
    /// <summary>
    /// Determines an item's method of being 'used'. 
    /// <para>None: use implementation will be completely handled by an external script, such as the gun managing how ammunition in the inventory is used. The inventory won't do any of the implementation</para>
    /// <para>Automatic: Use implementation can be triggered in the inventory menu, and also  'automatically' upon the player interacting with something 
    /// (IE: a key that triggers use when the player interacts with it's door, rather than requiring the player to explicitely find it in the inventory).</para>
    /// <para>OnlyViaInventory: Use of this item can only be triggered by selecting the 'Use' option in the inventory.</para>
    /// </summary>
    public enum UseMethod
    {
        None,
        Automatic,
        OnlyViaInventory,
    }
    #endregion

    public enum DoorStates
    {
        closed,
        opening,
        open,
        closing,
    };

    #region ENEMIES----------------------------------------------
    public enum EnemyType
    {
        Bug,
        BasicFlyer,
        Worm,
    }
    
    /// <summary>General action that the enemy is currently engaged in.</summary>
    public enum EnemyActionState 
    { 
        Patrolling,
        CaughtAGlimpse,
        HeardSuspiciousNoise,
        Suspicious,
        Chasing, 
        Engaging,
        ApproachingToStrike,
        Attacking,
        Dead
    };

    public enum Actions_bugEnemy
    {
        DoNothing,
        ChargeAttack,
        BasicStrike,
        RunAway,
    }

    public enum Actions_BasicFlyer
    {
        DoNothing,
        BasicStrike,
        Swooping,
        RunAway,
    }

    /// <summary>
    /// Describes the different states of a single attack.
    /// <para>Striking: Is used to determine if an attack is currently dealing damage, with a 'hot' damage collider.</para>
    /// </summary>
    public enum AttackState
    {
        None,
        Preparing,
        MidStrike,
        Recovering,
    }

    #endregion

    /// <summary>
    /// Allows for quick identification for what type of interactive object an instance of the Base_interactiveObject class is supposed to represent.
    /// </summary>
    public enum InteractiveObjectType
    {
        Prompt,
        ItemPickup,
        StandardDoor,
        AutoOpenDoor,
        AutoOpenUpAndDownDoor,
    }

    /// <summary>
    /// Describes not only what the current mode is for guns that have the ability to switch fire modes, but also can be extended to describe how a weapon fires
    /// </summary>
    public enum GunFireMode
    {
        SingleShot,
        SemiAutomatic,
        ThreeRoundBurst,
        FullyAutomatic,
    }

    public enum PV_LogFormatting
    {
        Standard,
        UserMethod,
        UnityAPIMethod,
    }

    public enum PV_LogDestination
    {
        Hidden, //Hidden only logs tot he session logger
        Console,
        MomentaryLogger,
        ConsoleAndMomentaryLogger,
        Everywhere
    }
}

