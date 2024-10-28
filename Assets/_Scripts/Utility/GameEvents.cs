using UnityEngine;
using UnityEngine.Events;
using PV_Enums;

namespace PV_Events
{
    public class PromptEvent : UnityEvent<PromptObject>
    {

    }

    public class PickupEvent : UnityEvent<PickupObject>
    {

    }

    /*public class CycleFireModeEvent : UnityEvent<GunFireMode>
    {

    }*/

    public class GunEvent : UnityEvent<Base_gun>
    {

    }

    /// <summary>
    /// Event type for use any time you want to call an event based on a direction. For example, this can be invoked when the dpad is pressed in a certain direction to invoke logic based on the dpad directional buttons
    /// </summary>
    public class DirectionEvent : UnityEvent<PV_Directions>
    {

    }
}