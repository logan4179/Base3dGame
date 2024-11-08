using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PV_Events;
using PV_Enums;

public class PV_Input : MonoBehaviour
{
    public static PV_Input Instance { get; private set; }
    /// <summary>
    /// Allows me to selectively bypass the update() logic that reads the axes. Currently being used for things like 
    /// waiting one second at game start to start reading input.
    /// </summary>
	public bool AmReadingInput = false;

	public static bool MouseControlOn;

    [SerializeField] private PV_ControlScheme myControlScheme;
    public static PV_ControlScheme Controls;

    #region [[------------------------ AXES -----------------------------]]
    // Movement/Camera orbit-----------
    public static float LookSensitivity;
    public static float Val_Axis_horizontal, Val_Axis_vertical, Val_Axis_horizontal_lerped, Val_Axis_vertical_lerped,
        Val_Axis_MouseX, Val_Axis_MouseY;

    public static float Val_Axis_RHorizontal, Val_Axis_RVertical, Val_Axis_RHorizontal_lerped, Val_Axis_RVertical_lerped;
    /// <summary>This controls the rate of lerp on the lerped right axis values that are used for player horizontal rotation and camera vertical rotation.</summary>
    public static float MaxLerpSpeed_rightAxis = 30f;
    public static float LookSmoothing;

    // Dpad-----------
    public static float Val_Axis_dpad_horizontal, Val_Axis_dpad_vertical;
    private float Val_Axis_dpad_horizontal_prev, Val_Axis_dpad_vertical_prev;
    #endregion

    /// <summary>Determines how long to wait before polling for if the dpad axis is considered "pressed". Prevents accidental rapid pressing.</summary>
    private float wait_cd_dpadAxisPoll = 0.4f;
    /// <summary>Alarm for counting down the dpad axis poll wait.</summary>
    private float cd_dpadAxisPoll;

    // [[------------------------ EVENTS-----------------------------]]
    public static DirectionEvent Event_OnHorizontalDpadPressed, Event_OnVerticalDpadPressed;

    //TODO: Should make properties here for verifying if a control is being triggered, like "TriggerPulled", etc. Other scripts like the gun script
    //or the camera script could rely on this during certain logic.

    #region [[------------------------ TRUTH -----------------------------]]
    /// <summary>Use this to determine the magnitude of the controller movement.</summary>
    public static float MoveAxisMagnitude
    {
        get
        {
            return Mathf.Max( Mathf.Abs(Val_Axis_horizontal_lerped), Mathf.Abs(Val_Axis_vertical_lerped) );
        }
    }
    /// <summary>This Vector is calculated each frame from 'Val_Axis_horizontal_lerped' and 'Val_Axis_vertical_lerped'. Use this for determining player movement.</summary>
    public static Vector2 V_LerpedMoveAxes_calculated
    {
        get
        {
            return new Vector2(Val_Axis_horizontal_lerped, Val_Axis_vertical_lerped);
        }
    }
    /// <summary>Property for telling if the aim is being properly triggered at this moment via player input. Does not take into account outside scripts (IE: whether the gun is in a condition where it can be aimed, etc...)</summary>
    public static bool AmPressingAimInput
    {
        get
        {
            return (Input.GetKeyDown(Controls.Keycode_Aim_mouse) || Input.GetAxis(Controls.AxisString_Aim_gamepad) > 0f);
        }
    }

    /// <summary>Property for telling if the aim is being properly held via player input. Does not take into account outside scripts (IE: whether the gun is in a condition where it can be aimed, etc...)</summary>
    public static bool AmHoldingAimInput
    {
        get
        {
            return (Input.GetKey(Controls.Keycode_Aim_mouse) || Input.GetAxis(Controls.AxisString_Aim_gamepad) > 0f);
        }
    }

    /// <summary>Property for telling if the aim is being properly released at this moment via player input. Does not take into account outside scripts (IE: whether the gun is in a condition where it can be aimed, etc...)</summary>
    public static bool AmReleasingAimInput
    {
        get
        {
            return (Input.GetKeyUp(Controls.Keycode_Aim_mouse) || Input.GetAxis(Controls.AxisString_Aim_gamepad) > 0f );
        }
    }

    /// <summary>Property for telling if the trigger is being properly pressed at this moment via player input. Does not take into account outside scripts (IE: whether the gun is in a condition where it can be fired, etc...)</summary>
    public static bool AmPressingTriggerInput
    {
        get
        {
            return (Input.GetKeyDown(Controls.KeyCode_Fire_mouse) || Input.GetAxisRaw(Controls.AxisString_Fire_gamepad) > 0f );
        }
    }

    /// <summary>Property for telling if the trigger is being properly held via player input. Does not take into account outside scripts (IE: whether the gun is in a condition where it can be fired, etc...)</summary>
    public static bool AmHoldingTriggerInput
    {
        get
        {
            return (Input.GetKey(Controls.KeyCode_Fire_mouse) || Input.GetAxisRaw(Controls.AxisString_Fire_gamepad) > 0f );
        }
    }

    /// <summary>Property for telling if the reload input is being properly pressed at this moment via player input.</summary>
    public static bool ReloadIsBeingPressed
    {
        get
        {
            return (Input.GetKeyDown(Controls.Keycode_Reload_gamepad) || Input.GetKeyDown(Controls.Keycode_Reload) );
        }
    }
    #endregion

    [Header("[-------- DEBUG --------]")]
    [SerializeField, TextArea(10, 20)] private string debugString;

    private void OnEnable()
    {
        Event_OnHorizontalDpadPressed = new DirectionEvent();
        Event_OnVerticalDpadPressed = new DirectionEvent();
    }

    private void OnDisable()
    {
        Event_OnHorizontalDpadPressed.RemoveAllListeners();
        Event_OnVerticalDpadPressed.RemoveAllListeners();
    }

    private void Awake()
    {
        PV_Debug.Log($"PV_Input Awake()", PV_LogFormatting.UnityAPIMethod );

        if ( Instance == null )
        {
            Instance = this;
        }
        PV_Debug.Log($"PV_Input Awake()", PV_LogFormatting.UnityAPIMethod );
    }

    void Start()
    {
        PV_Debug.Log($"PV_Input Awake()", PV_LogFormatting.UnityAPIMethod );

        MouseControlOn = true;

        LookSensitivity = DataManger.Instance.GetLookSensitivityPref();
        LookSmoothing = DataManger.Instance.GetLookSmoothing();
        if( myControlScheme == null )
        {
            PV_Debug.LogError( $"VERTIGO ERROR! No control scheme was provided to input manager!" );
        }
        else
        {
            Controls = myControlScheme;
        }

        PV_Debug.Log( $"End of PV_Input Start(). Got LookSensitivity from playerprefs as: '{LookSensitivity}', and LookSmoothing as: '{LookSmoothing}'", PV_LogFormatting.UnityAPIMethod, PV_LogDestination.Console );

    }


    float duration_waitToStartInput = 1f;

    void Update()
    {
        if( !AmReadingInput )
        {
            if( Time.time >= duration_waitToStartInput )
            {
                AmReadingInput = true;
                PV_Debug.Log($"Input manager has now waited for '{duration_waitToStartInput}' seconds.  Now accepting input...", PV_LogFormatting.Standard, PV_LogDestination.Console );
            }
            return;
        }
            

        if( cd_dpadAxisPoll > 0 ) 
        {
            cd_dpadAxisPoll -= Time.deltaTime;
            if( cd_dpadAxisPoll <= 0 )
            {
                cd_dpadAxisPoll = 0;
            }
        }

        // GET INPUT----------------------------------//
        Val_Axis_horizontal = Input.GetAxis( Controls.AxisString_horizontal );
        Val_Axis_horizontal_lerped = Mathf.Lerp( Val_Axis_horizontal_lerped, Val_Axis_horizontal, myControlScheme.Speed_movementLerp * Time.deltaTime );
        Val_Axis_vertical = Input.GetAxis(Controls.AxisString_vertical);
        Val_Axis_vertical_lerped = Mathf.Lerp( Val_Axis_vertical_lerped, Val_Axis_vertical, myControlScheme.Speed_movementLerp * Time.deltaTime );

        Val_Axis_RHorizontal = Input.GetAxis(Controls.AxisString_RHorizontal);
        Val_Axis_RVertical = Input.GetAxis(Controls.AxisString_RVertical);
        Val_Axis_MouseX = Input.GetAxis(Controls.AxisString_MouseX);
        Val_Axis_MouseY = Input.GetAxis(Controls.AxisString_MouseY);


        Val_Axis_dpad_horizontal = Input.GetAxis(Controls.AxisString_dpad_horizontal);
        Val_Axis_dpad_vertical = Input.GetAxis(Controls.AxisString_dpad_vertical);
        PollDpad();
        Val_Axis_dpad_horizontal_prev = Val_Axis_dpad_horizontal;
        Val_Axis_dpad_vertical_prev = Val_Axis_dpad_vertical;


        Val_Axis_RHorizontal_lerped = Mathf.Lerp(Val_Axis_RHorizontal_lerped, (MouseControlOn ? -Val_Axis_MouseX : -Val_Axis_RHorizontal), (MaxLerpSpeed_rightAxis / LookSmoothing) * Time.deltaTime);
        Val_Axis_RVertical_lerped = Mathf.Lerp(Val_Axis_RVertical_lerped, (MouseControlOn ? Val_Axis_MouseY : -Val_Axis_RVertical), (MaxLerpSpeed_rightAxis / LookSmoothing) * Time.deltaTime);

        // For verifying a keycode...
        /*
        if ( Input.GetKeyDown(myControlScheme.Keycode_FirstWeapon) )
        {
            print("hay");
        }
        */

        debugString = $"Val_Axis_horizontal: '{Val_Axis_horizontal}'\n" +
            $"Val_Axis_horizontal_lerped: '{Val_Axis_horizontal_lerped}'\n" +
            $"Val_Axis_vertical: '{Val_Axis_vertical}'\n" +
            $"Val_Axis_vertical_lerped: '{Val_Axis_vertical_lerped}'\n" +
            "\n"+
            "Controller: \n" +
            $"Val_Axis_RHorizontal: '{Val_Axis_RHorizontal}'\n" +
            $"Val_Axis_RHorizontal_lerped: '{Val_Axis_RHorizontal_lerped}'\n" +
            $"Val_Axis_RVertical: '{Val_Axis_RVertical}'\n" +
            $"Val_Axis_RVertical_lerped: '{Val_Axis_RVertical_lerped}'\n" +
            "/n" +
            "Mouse/Keyboard \n" +
            $"Val_Axis_MouseX: '{Val_Axis_MouseX}'\n" +
            $"Val_Axis_MouseY: '{Val_Axis_MouseY}'\n";
    }

    private void PollDpad()
    {
        if (cd_dpadAxisPoll == 0)
        {
            if (Val_Axis_dpad_horizontal_prev == 0)
            {
                if (Val_Axis_dpad_horizontal == 1)
                {
                    PV_Directions myDir = PV_Directions.right;
                    PV_GameManager.Event_OnHorizontalDpadPressed.Invoke(myDir);
                    cd_dpadAxisPoll = wait_cd_dpadAxisPoll;
                    return;
                }
                else if (Val_Axis_dpad_horizontal == -1)
                {
                    PV_GameManager.Event_OnHorizontalDpadPressed.Invoke(PV_Directions.left);
                    cd_dpadAxisPoll = wait_cd_dpadAxisPoll;
                    return;
                }
            }

            if (Val_Axis_dpad_vertical_prev == 0)
            {
                if (Val_Axis_dpad_vertical == 1)
                {
                    Event_OnVerticalDpadPressed.Invoke(PV_Directions.up);
                    cd_dpadAxisPoll = wait_cd_dpadAxisPoll;
                    return;
                }
                else if (Val_Axis_dpad_vertical == -1)
                {
                    Event_OnVerticalDpadPressed.Invoke(PV_Directions.down);
                    cd_dpadAxisPoll = wait_cd_dpadAxisPoll;
                    return;
                }
            }
        }
    }

    public void CreateRecoil( Vector2 v_recoil )
    {
        print($"CreateRecoil('{v_recoil}'). \n" +
            $"Val_Axis_RHorizontal: '{Val_Axis_RHorizontal}', Val_Axis_RVertical: '{Val_Axis_RVertical}'");
        Val_Axis_RHorizontal_lerped += v_recoil.y;
        Val_Axis_RVertical_lerped += v_recoil.x;
        print( $"end Val_Axis_RHorizontal: '{Val_Axis_RHorizontal}', Val_Axis_RVertical: '{Val_Axis_RVertical}'");
    }
}
