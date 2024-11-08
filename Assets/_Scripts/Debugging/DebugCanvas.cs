using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugCanvas : MonoBehaviour
{
    [Header("REFERENCE")]
    //public Toggle _tgl_Debug;
    public TMP_Dropdown DD_debugMode;
    public Toggle _tglMouseControl = null;
    [SerializeField] private Canvas Canvas_debug;
    [SerializeField] private Toggle tgl_LockCursor;

	[Header("REFERENCE [GENERAL]")]
    [SerializeField] private GameObject _go_ctr_dbg_general;
	[SerializeField] private TextMeshProUGUI _txt_dbg_general;
    [SerializeField] private Slider sldr_timeScale;
    [SerializeField] private TextMeshProUGUI txt_timescaleValue;

    [Header("REFERENCE [PLAYER]")]
    [SerializeField] private GameObject _go_ctr_dbgPlayer;
    [SerializeField] private TextMeshProUGUI _txt_dbg_player;



    public GameObject _go_ctr_DbgControls;
    public GameObject _go_ctr_bugEnemies, _go_ctr_basicFlyerEnemies;
    public GameObject _go_ctr_Environment;
    public GameObject _go_ctr_Camera;


	[Header("---------------[[ REFERENCE ]]-----------------")]
    [SerializeField] private Toggle tgl_debugEnvironment;
	[SerializeField] private Toggle tgl_cameraDebug;
	[SerializeField] private Toggle tgl_debugPlayer;
    [SerializeField] private Toggle tgl_debugBugEnemies, tgl_debugBasicFlyerEnemies;

    [Header("TRUTH")]
    public bool havePassedStart = false;

    [Header("OTHER")]
    string stringTxtVisIndex;

    [Header("DEBUG")]
    public string debugString = null;
    private string dbgDrpDwnValue = string.Empty;

    public List<string> LogStrings;
    //public string[] LogStrings;

    private void Awake()
    {
        havePassedStart = false;
        PV_GameManager.Instance.CanvasScript_debug = this;

    }
    void Start()
    {
        LogStrings = new List<string>();

        _go_ctr_DbgControls.SetActive(false);
        _go_ctr_dbgPlayer.SetActive(false);
        _go_ctr_dbg_general.SetActive(false);
        _go_ctr_bugEnemies.SetActive(false);
		_go_ctr_basicFlyerEnemies.SetActive(false);
		_go_ctr_Environment.SetActive(false);
        _go_ctr_Camera.SetActive(false);

        sldr_timeScale.value = 1f;
        txt_timescaleValue.text = "1.0";

        havePassedStart = true;
    }

	private void Update()
	{
		if ( dbgDrpDwnValue == "General" )
		{
            _txt_dbg_general.text = PV_GameManager.Instance.GetDebugString();
		}
		else if ( dbgDrpDwnValue == "Controls" )
		{

		}
		else if (dbgDrpDwnValue == "Player")
		{
            _txt_dbg_player.text = PV_GameManager.Instance.PlayerScript.GetDebugString();
		}
		else if (dbgDrpDwnValue == "Bug Enemies")
		{

		}
		else if (dbgDrpDwnValue == "Basic Flyers")
		{

		}
		else if ( dbgDrpDwnValue == "Environment" )
		{

		}
		else if ( dbgDrpDwnValue == "Camera" )
		{

		}
	}

	public void DebugModeDropDownChanged_action()
    {
        dbgDrpDwnValue = DD_debugMode.options[DD_debugMode.value].text;

        _go_ctr_DbgControls.SetActive(false);
        _go_ctr_dbgPlayer.SetActive(false);
        _go_ctr_dbg_general.SetActive(false);
        _go_ctr_bugEnemies.SetActive(false);
		_go_ctr_basicFlyerEnemies.SetActive(false);
		_go_ctr_Environment.SetActive(false);
        _go_ctr_Camera.SetActive(false);

        if ( dbgDrpDwnValue == "General" )
        {
            _go_ctr_dbg_general.SetActive(true);
        }
        else if( dbgDrpDwnValue == "Controls" )
        {
            _go_ctr_DbgControls.SetActive(true);

        }
        else if ( dbgDrpDwnValue == "Player" )
        {
            _go_ctr_dbgPlayer.SetActive(true);
        }
        else if( dbgDrpDwnValue == "Bug Enemies" )
        {
            _go_ctr_bugEnemies.SetActive(true);
        }
		else if ( dbgDrpDwnValue == "Basic Flyers")
		{
			_go_ctr_basicFlyerEnemies.SetActive(true);
		}
		else if ( dbgDrpDwnValue == "Environment")
        {
            _go_ctr_Environment.SetActive(true);
        }
        else if ( dbgDrpDwnValue == "Camera")
        {
            _go_ctr_Camera.SetActive(true);
        }
        else
        {
            Debug.LogError($"choice: '{dbgDrpDwnValue}' wasn't found or yet setup in code...");
        }
    }

    public void Tgl_MouseControl_Action()
    {
        PV_Input.MouseControlOn = !PV_Input.MouseControlOn;
        _tglMouseControl.isOn = PV_Input.MouseControlOn;
    }

    public void Tgl_SlowAttacksAction()
    {
        PV_SceneDebugger.SlowAttacksOn = !PV_SceneDebugger.SlowAttacksOn;

        if( !PV_SceneDebugger.SlowAttacksOn )
        {
            Time.timeScale = 1f;
        }
    }

    public void UI_SliderTimescaleAction()
    {
        if( !havePassedStart )
        {
            return;
        }

        Time.timeScale = sldr_timeScale.value;
        txt_timescaleValue.text = Time.timeScale.ToString("#.##");
    }

    public void UI_Toggle_LockCursor_action()
    {
        if (tgl_LockCursor.isOn)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ToggleEnvironmentDebugAction()
    {
        PV_Environment.AmDebugging = tgl_debugEnvironment.isOn;
        PV_Environment.Instance.DebugAction();
    }
	public void UI_TogglePlayerDebugAction()
	{
        PV_Debug.Log($"{nameof(UI_TogglePlayerDebugAction)}, '{tgl_debugPlayer.isOn}'", PV_Enums.PV_LogFormatting.Standard, PV_Enums.PV_LogDestination.MomentaryLogger);
		PV_SceneDebugger.Instance.Player_DebugAction(tgl_debugPlayer.isOn);
	}

	public void UI_ToggleBugEnemyDebugAction()
    {
		PV_Debug.Log($"{nameof(UI_ToggleBugEnemyDebugAction)}, '{tgl_debugBugEnemies.isOn}'", PV_Enums.PV_LogFormatting.Standard, PV_Enums.PV_LogDestination.MomentaryLogger);

		PV_SceneDebugger.Instance.BugEnemies_DebugAction( tgl_debugBugEnemies.isOn );
    }

	public void ToggleBasicFlyerEnemyDebugAction()
	{
		PV_Debug.Log($"{nameof(ToggleBasicFlyerEnemyDebugAction)}, '{tgl_debugBasicFlyerEnemies.isOn}'", PV_Enums.PV_LogFormatting.Standard, PV_Enums.PV_LogDestination.MomentaryLogger);

		PV_SceneDebugger.Instance.BasicFlyerEnemies_DebugAction( tgl_debugBasicFlyerEnemies.isOn );

	}

	public void ToggleCameraDebugAction()
    {
		PV_Debug.Log($"{nameof(ToggleCameraDebugAction)}, '{tgl_cameraDebug.isOn}'", PV_Enums.PV_LogFormatting.Standard, PV_Enums.PV_LogDestination.MomentaryLogger);

		PV_GameManager.Instance.M_ThirdPersonCamera.DebugToggleAction(tgl_cameraDebug.isOn);
    }
}
