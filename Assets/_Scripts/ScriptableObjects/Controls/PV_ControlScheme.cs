using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VertigoObjects/ControlScheme", fileName = "controlScheme")]
public class PV_ControlScheme : ScriptableObject
{
    // GENERAL ---------------------------------------------------------------------
    public KeyCode Keycode_Interact, Keycode_Interact_gamepad;
    public KeyCode Keycode_FlashlightToggle, Keycode_FlashlightToggle_gamepad;
    public KeyCode Keycode_PauseToSettings, Keycode_PauseToSettings_gamepad;
    public KeyCode Keycode_PauseToInventory, Keycode_PauseToInventory_gamepad;
    /// <summary>Key for declining a prompt or going backwards in a menu.</summary>
    public KeyCode Keycode_BackOneLevel, Keycode_BackOneLevel_gamepad;

    [Header("GUN")]
    public KeyCode KeyCode_Fire_mouse;
    public string AxisString_Fire_gamepad;

    public KeyCode Keycode_Aim_mouse;
    public string AxisString_Aim_gamepad;

    public KeyCode Keycode_Reload;
    public KeyCode Keycode_Reload_gamepad;
    public KeyCode Keycode_SwitchFireMode, Keycode_SwitchFireMode_gamepad;
    public KeyCode Keycode_FirstWeapon, Keycode_SecondWeapon;
    [Space(20)]

    public string AxisString_horizontal;
    public string AxisString_vertical, AxisString_RHorizontal, AxisString_RVertical,
    AxisString_MouseX, AxisString_MouseY;

    // Dpad-----------
    public string AxisString_dpad_horizontal, AxisString_dpad_vertical;

    [Header("STATS")]
    public float Speed_movementLerp = 10f;
}
