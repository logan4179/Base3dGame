using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PV_Enums;
using LogansUINavigator;
using LHM;

public class Canvas_inGame : PV_Object
{
    [SerializeField] private bool amDebuggingScript;

    [Header("---------------[[ REFERENCE ]]-----------------")]
    [SerializeField] private Transform t;
    
    [SerializeField] private Canvas Canvas_settings;
    [SerializeField] private Image image_momentaryPickupDisplay;
    [SerializeField] private LHM_RectangularFrame momentaryPickupHighlight;
    [Space(5f)]

    [Header("---------------[[ REFERENCE - Inventory Canvas ]]-----------------")]
    [SerializeField] private Canvas Canvas_inventory;
    [SerializeField] Button btn_Inventory, btn_Files, btn_Map;
    public TextMeshProUGUI _txt_ctr_SelectedItemInfo;
    public TextMeshProUGUI Txt_resultOfUseAttempt;

    [Header("In-Game UI elements")]
    public GameObject GameObject_ctr_PromptText, _go_ri_PromptButton;
    public TextMeshProUGUI _txtHP, _txt_Ammunition, txt_Prompt;
    [SerializeField] private Slider _sldrHP;
    [SerializeField] private List<GameObject> bulletImageObjects;
    /// <summary>Bottom screen image showing which weapon is equipped.</summary>
    [SerializeField] private Image _img_Weapon;

    [Header("Paused (Settings)")]
    [SerializeField] private TextMeshProUGUI _txt_ControlSensitivity;
    [SerializeField] private TextMeshProUGUI _txt_AxisLerpSpeed, _txt_SoundEffectsVolume, _txt_MusicVolume,
        _txt_MasterVolume;
	[SerializeField] private Slider _sldr_LookSensitivity, _sldr_AxisLerpSpeed, _sldr_SoundEffectsVolume, _sldr_MusicVolume,
	_sldr_MasterVolume;

	[Header("TRUTH")]
    private bool havePassedStart;

    [Header("---------------[[ ALARMS ]]-----------------")]
    private float duration_cd_momentaryPickupDisplay = 3f;
    private float cd_momentaryPickupDisplay;

    [Header("Debug UI")]
    [SerializeField] private Toggle _tglMouseControl;

    private void Awake()
    {
        LogHistoric( "Awake() begin", PV_LogFormatting.UnityAPIMethod );
        PV_GameManager.Instance.CanvasScript_inGame = this;

        havePassedStart = false;
        LogHistoric( "Awake() start", PV_LogFormatting.UnityAPIMethod );

    }
    void Start()
    {
        LogHistoric("Start() begin", PV_LogFormatting.UnityAPIMethod);

        GameObject_ctr_PromptText.SetActive(false);
        _sldr_LookSensitivity.value = PV_Input.LookSensitivity;
        //_sldr_AxisLerpSpeed.value = G.LerpSpeed_controlAxis; //old way of handling this value...
        _sldr_AxisLerpSpeed.value = PV_Input.LookSmoothing*10f;

        _sldr_SoundEffectsVolume.value = PV_AudioManager.volume_Effects;
        _sldr_MusicVolume.value = PV_AudioManager.volume_Music;
        _sldr_MasterVolume.value = PV_AudioManager.volume_Master;

        _sldrHP.value = PV_GameManager.Instance.PlayerScript.HP;
        _txtHP.text = PV_GameManager.Instance.PlayerScript.HP.ToString();

        _txt_ctr_SelectedItemInfo.text = "";
        Txt_resultOfUseAttempt.text = "";
        _go_ri_PromptButton.SetActive(false);

        List<Canvas> canvases = PV_Utils.PV_Utilities.GetComponentsInChildren_selective<Canvas>(gameObject, this.GetComponent<Canvas>() );
        foreach (Canvas c in canvases)
        {
            c.transform.position = transform.position;
        }

        image_momentaryPickupDisplay.color = Color.clear;
        momentaryPickupHighlight.color = Color.clear;

        PV_GameManager.Event_OnPromptEntered.AddListener(OnPrompEntered_action);
        PV_GameManager.Event_OnPromptReadNext.AddListener(OnPromptReadNext_action);
        PV_GameManager.Event_OnPromptExited.AddListener(OnPrompExited_action);

        //PV_GameManager.Event_OnPickup.AddListener(OnPickup_action);
        PV_GameManager.Event_OnItemPickedUp.AddListener(OnItemPickedUp_action);

        PV_GameManager.Event_OnGunChanged.AddListener(OnGunChanged_action);

        UI_ControlSlidersChangedAction();
        UI_SoundSliders_ChangedAction();

        havePassedStart = true;

        LogHistoric("Start() end", PV_LogFormatting.UnityAPIMethod);

    }

    private void Update()
    {
        if (cd_momentaryPickupDisplay > 0f)
        {
            cd_momentaryPickupDisplay -= Time.deltaTime;

            if (cd_momentaryPickupDisplay < 1f)
            {
                Color fadedColor = new Color(image_momentaryPickupDisplay.color.r,
                    image_momentaryPickupDisplay.color.g, image_momentaryPickupDisplay.color.b, (cd_momentaryPickupDisplay / 1f) );

                image_momentaryPickupDisplay.color = fadedColor;
                momentaryPickupHighlight.color = fadedColor;

            }

            if (cd_momentaryPickupDisplay < 0f)
            {
                cd_momentaryPickupDisplay = 0f;
                image_momentaryPickupDisplay.color = Color.clear;
                momentaryPickupHighlight.color = Color.clear;
            }
        }
    }

    private void OnPickup_action( PickupObject pickupObject_passed )
    {
        image_momentaryPickupDisplay.color = Color.white;
        image_momentaryPickupDisplay.sprite = pickupObject_passed.MyItemData.ItemSprite;
        cd_momentaryPickupDisplay = duration_cd_momentaryPickupDisplay;
    }

    private void OnItemPickedUp_action( PickupObject pickupObject_passed )
	{
		image_momentaryPickupDisplay.color = Color.white;
		image_momentaryPickupDisplay.sprite = pickupObject_passed.MyItemData.ItemSprite;
		cd_momentaryPickupDisplay = duration_cd_momentaryPickupDisplay;
	}

    public void updateHP(int amt)
    {
        _sldrHP.value = amt;
        _txtHP.text = amt.ToString();
    }

    #region UI METHODS
    public void UI_SlowAttacksToggleAction()
    {
        PV_SceneDebugger.SlowAttacksOn = !PV_SceneDebugger.SlowAttacksOn;
    }

    public void UI_MouseControlToggleAction()
    {
        if (!havePassedStart)
        { return; }

        print("TglMouseControl_Action()");


        if (_tglMouseControl.isOn)
        {
            PV_Input.MouseControlOn = true;
        }
        else
        {
            PV_Input.MouseControlOn = false;
        }
    }

    public void UI_ControlSlidersChangedAction()
    {
        if (!havePassedStart)
        { return; }

        PV_Debug.LogWithConsoleConditional("UI_ControlSlidersChangedAction()", amDebuggingScript, PV_LogFormatting.UserMethod);

        PV_Input.LookSensitivity = Mathf.Max(1, _sldr_LookSensitivity.value);
        _txt_ControlSensitivity.text = _sldr_LookSensitivity.value.ToString("#.##");

        //G.LerpSpeed_controlAxis = _sldr_AxisLerpSpeed.value; //this is the old way of just displaying the float value...
        //G.LerpSpeed_controlAxis = G.LerpSpeed_max / _sldr_AxisLerpSpeed.value; //this is the new way of displaying the value as an intuitive "smoothing" value to the player
        PV_Input.LookSmoothing = (_sldr_AxisLerpSpeed.value/10f);
        _txt_AxisLerpSpeed.text = _sldr_AxisLerpSpeed.value.ToString("#.##");
    }

    public void Ui_ControlsMenu_BackButtonAction()
    {
        PV_Debug.LogWithConsoleConditional("Ui_ControlsMenu_BackButtonAction()", amDebuggingScript, PV_LogFormatting.UserMethod);

        DataManger.Instance.SetLookSensitivityPref(PV_Input.LookSensitivity);
        DataManger.Instance.SetLookSmoothing(PV_Input.LookSmoothing);
    }

    public void UI_SoundSliders_ChangedAction()
    {
        if (!havePassedStart)
        { return; }
        PV_Debug.LogWithConsoleConditional("UI_SoundSliders_ChangedAction()", amDebuggingScript, PV_LogFormatting.UserMethod);

        PV_AudioManager.volume_Effects = _sldr_SoundEffectsVolume.value;
        _txt_SoundEffectsVolume.text = PV_AudioManager.volume_Effects.ToString("#.##");

        PV_AudioManager.volume_Music = _sldr_MusicVolume.value;
        _txt_MusicVolume.text = PV_AudioManager.volume_Music.ToString("#.##");

        PV_AudioManager.volume_Master = _sldr_MasterVolume.value;
        _txt_MasterVolume.text = PV_AudioManager.volume_Master.ToString("#.##");
    }

    public void UI_SoundMenu_BackButtonAction()
    {
        DataManger.SetMasterVolumePref(PV_AudioManager.volume_Master);
        DataManger.SetMusicVolumePref(PV_AudioManager.volume_Music);
        DataManger.SetEffectsVolumePref(PV_AudioManager.volume_Effects);
    }

    public void UI_QuitButtonAction()
    {
        Application.Quit();
    }
    #endregion

    #region INVENTORY-----------------------------------    
    public void DisableTopInventoryCanvasButtons()
    {
        btn_Files.enabled = false;
        btn_Inventory.enabled = false;
        btn_Map.enabled = false;
    }

    public void EnableTopInventoryCanvasButtons()
    {
        btn_Files.enabled = true;
        btn_Inventory.enabled = true;
        btn_Map.enabled = true;
    }
    #endregion

    /// <summary>Call when the player is in the conditions to interact with an interactive object. Displays 
    /// interaction button graphic, or a message instead (not implemented yet).</summary>
    public void ActivateInteractionIndicator(string promptMessage_passed = "")
    {
        if( string.IsNullOrEmpty(promptMessage_passed) )
        {
            _go_ri_PromptButton.SetActive(true);
        }
    }

    /// <summary>Call when the player is no longer in the conditions to interact with an interactive object. 
    /// Disables interaction button graphic, and message text (not implemented yet).</summary>
    public void DeactivateInteractionIndicator()
    {
        _go_ri_PromptButton.SetActive(false);
    }

    void OnPrompEntered_action(PromptObject promptOb_passed)
    {
        GameObject_ctr_PromptText.SetActive(true);
        txt_Prompt.text = promptOb_passed.PromptMessages[0];
        promptOb_passed.Index_PromptMessages = 1;
    }

    void OnPromptReadNext_action(PromptObject promptOb_passed )
    {
        txt_Prompt.text = promptOb_passed.PromptMessages[promptOb_passed.Index_PromptMessages];
        promptOb_passed.Index_PromptMessages++;
    }

    void OnPrompExited_action(PromptObject promptOb_passed)
    {
        txt_Prompt.text = string.Empty;
        GameObject_ctr_PromptText.SetActive(false);
        promptOb_passed.Index_PromptMessages = -1;
    }


    #region EVENT TYPE METHODS-------------
    public void SetFireModeGraphic( GunFireMode fireMode_passed )
    {
        foreach( GameObject g in bulletImageObjects )
        {
            if( fireMode_passed == GunFireMode.FullyAutomatic )
            {
                g.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }
        }

        if( fireMode_passed == GunFireMode.SemiAutomatic )
        {
            bulletImageObjects[0].SetActive(true);
        }
        else if( fireMode_passed == GunFireMode.ThreeRoundBurst )
        {
            bulletImageObjects[0].SetActive(true);
            bulletImageObjects[1].SetActive(true);
            bulletImageObjects[2].SetActive(true);
        }
    }

    public void OnGunChanged_action( Base_gun gunScript_passed )
    {
        LogHistoric( $"OnGunChanged_action() gun passed: '{gunScript_passed}'", PV_LogFormatting.UserMethod );
        _img_Weapon.sprite = gunScript_passed.MyItemData.ItemSprite;
        SetFireModeGraphic(gunScript_passed.MyFireMode);
    }
    #endregion

}