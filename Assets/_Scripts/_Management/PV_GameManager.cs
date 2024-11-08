using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PV_Events;
using LogansUINavigator;
using PV_Enums;
using LogansThirdPersonCamera;

/// <summary>This is the Game-manager class.</summary>
public class PV_GameManager : MonoBehaviour
{
    public static PV_GameManager Instance;

    [Header("STATE")]
    //public static bool AmDebugging;     //The actual Debug variable, static for quick-access
    [SerializeField] private GameState myGameState;
    public GameState MyGameState => myGameState;


    //[Header("STATS")]
    /// <summary>Keeps track of what the canvas time slider wants the time scale to be.  Needed for the slow attacks UI toggle.</summary>
    public static float GlobalTimeScale;

    [Header("REFERENCE")] //For 
    [HideInInspector] public Transform Trans_Player = null;
    [HideInInspector] public Transform Trans_PlayerPerspective = null;
    [HideInInspector] public PV_Player PlayerScript = null;

	[HideInInspector] public Transform T_Camera = null;
	[HideInInspector] public ThirdPersonCamera M_ThirdPersonCamera = null;
	[HideInInspector] public Camera Camera_inGame = null;

	[HideInInspector] public Canvas_inGame CanvasScript_inGame = null;

	[HideInInspector] public DebugCanvas CanvasScript_debug = null;
    [HideInInspector] public PV_Inventory InventoryScript = null;

    //[Header("EVENTS")]
    public static UnityEvent Event_PauseButton;

    public static PickupEvent Event_OnItemPickedUp;

    public static GunEvent Event_OnGunChanged;

    public static DirectionEvent Event_OnHorizontalDpadPressed, Event_OnVerticalDpadPressed;

	//[Header("---------------[[ OTHER ]]-----------------")]
	/// <summary>Amount of times per second to fire the level 2 logic coroutine.</summary>
	[Tooltip("Amount of times per second to fire the level 2 logic coroutine. Gets recalculated to appropriate floating point at start.")]
	private float level2Logic_frequency = 10;
	/// <summary>Amount of times per second to fire the level 2 logic coroutine.</summary>
	[Tooltip("Amount of times per second to fire the level 3 logic coroutine. Gets recalculated to appropriate floating point at start.")]
	private float level3Logic_frequency = 2;
	[Tooltip("Amount of times per second to fire the level 4 logic coroutine. Gets recalculated to appropriate floating point at start.")]
	private float level4Logic_frequency = 0.15f;

    //[Header("---------------[[ TAGS ]]-----------------")]
    // The idea of these variables is that any script using these tags can refer to these variables, that way if I change the tags through the inspector, I will only have to update
    // it in code in one single place (here).

    public static string Tag_GameManager = "GameManager";
    public static string Tag_EnvironmentManager = "EnvironmentManager";
    public static string Tag_BugEnemyManager = "BugEnemyManager";
    public static string Tag_area = "PV_Area";
    public static string Tag_Player = "Player";
    public static string Tag_SceneDebugger = "SceneDebugger";

    //These are the triggers that define the enemy shape for hit detection. Typically they'll 
    //have a debug material/color applied to them that needs to be cleared by the scene debugger.
	public static string Tag_triggerCollider = "TriggerCollider";

	public static string Tag_bugEnemy = "Enemy_bug";
	public static string Tag_basicFlyerEnemy = "Enemy_basicFlyer";
    public static string Tag_ReverbFollower = "ReverbFollower";

    //[Header("OTHER")]
    /// <summary>
    /// Flag that gets set true immediately when pause is triggered, then false on the next frame for cleanup.
    /// </summary>
    private bool flag_pauseTriggeredLastFrame = false;
    private bool flag_unpauseTriggeredLastFrame = false;

	[Header("DEBUG")]
    [TextArea(1, 10), SerializeField] private string DebugCanonString;


	private void OnEnable()
    {
        Event_PauseButton = new UnityEvent();

        //Event_OnPickup = new PickupEvent();
        Event_OnItemPickedUp = new PickupEvent();

        Event_OnGunChanged = new GunEvent();

        Event_OnHorizontalDpadPressed = new DirectionEvent();
        Event_OnVerticalDpadPressed = new DirectionEvent();
    }

    private void OnDisable()
    {
        Event_PauseButton.RemoveAllListeners();

        //Event_OnPickup.RemoveAllListeners();
        Event_OnItemPickedUp.RemoveAllListeners();

        Event_OnGunChanged.RemoveAllListeners();

        Event_OnHorizontalDpadPressed.RemoveAllListeners();
        Event_OnVerticalDpadPressed.RemoveAllListeners();
    }

    private void Awake()
    {
        PV_Debug.Log( "Game Manager Awake()", ref DebugCanonString, PV_LogFormatting.UnityAPIMethod );
        Instance = this;
        Camera_inGame = GameObject.Find("In-Game Camera").GetComponent<Camera>();
        M_ThirdPersonCamera = Camera_inGame.gameObject.GetComponent<ThirdPersonCamera>();
        T_Camera = Camera_inGame.GetComponent<Transform>();

        PV_Debug.Log( "Game Manager Awake end", ref DebugCanonString, PV_LogFormatting.UnityAPIMethod);
    }

    void Start()
    {
        PV_Debug.Log("Game Manager Start()", ref DebugCanonString, PV_LogFormatting.UnityAPIMethod );
        //[Header("STATE")]-------------------------
        GlobalTimeScale = Time.timeScale;

        Event_PauseButton.AddListener(PauseButton_action);

        myGameState = GameState.Unpaused;
        Random.InitState(System.DateTime.Now.Millisecond);

		level2Logic_frequency = 1f / level2Logic_frequency;
		StartCoroutine("level2Logic");

		level3Logic_frequency = 1 / level3Logic_frequency;
		StartCoroutine("level3Logic");

		level4Logic_frequency = 1 / level4Logic_frequency;
		StartCoroutine("level4Logic");

		PV_Debug.Log($"end of Game Manager Start()", ref DebugCanonString, PV_LogFormatting.UnityAPIMethod );

    }

	private void LateUpdate()
	{
		if( flag_pauseTriggeredLastFrame )
        {
			flag_pauseTriggeredLastFrame = false;
			Time.timeScale = 0f;
			myGameState = GameState.Paused_options;

		}
		else if( flag_unpauseTriggeredLastFrame)
        {
			myGameState = GameState.Unpaused;
			flag_unpauseTriggeredLastFrame = false;
			Time.timeScale = 1f;
		}
	}

	#region OBJECT REGISTRATION -----------------/////////////////
	public void RegisterPlayer( PV_Player plrScript )
    {
	    PlayerScript = plrScript;
		Trans_Player = plrScript.GetComponent<Transform>();
    }

	public void RegisterCamera( ThirdPersonCamera camScript)
	{
        M_ThirdPersonCamera = camScript;
        T_Camera = camScript.GetComponent<Transform>();
        Camera_inGame = camScript.GetComponent<Camera>();
	}

	#endregion

	/// <summary>This simply provides a way for the inspector to invoke this. Primarily intended for the LogansCanvasNavigator system</summary>
	public void InvokePauseButtonEvent()
    {
        Event_PauseButton.Invoke();
    }
    void PauseButton_action() //todo: might git rid of this one when satisfied...
    {
        PV_Debug.Log( "PauseButton_action()", PV_LogFormatting.UserMethod );

        if( myGameState == GameState.Unpaused )
        {
			flag_pauseTriggeredLastFrame = true;
		}
		else
        {
            flag_unpauseTriggeredLastFrame = true;
        }
    }

    /// <summary>This was added just in case I want to force pause (no state reversing/negating/swizzling) the game from some script trigger.  </summary>
    public void PauseGameToOptions()
    {
        myGameState = GameState.Paused_options;

        Time.timeScale = 0f;
    }

    [ContextMenu("z call UnpauseGame()")]
	public void UnPauseGame()
	{
		myGameState = GameState.Unpaused;

		Time.timeScale = 1f;
	}

	public void EnterPrompt( PromptObject promptOb_passed )
    {
        print($"{nameof(EnterPrompt)}, object: '{promptOb_passed.gameObject.name}'");
        Time.timeScale = 0f;
        myGameState = GameState.Paused_readingPrompt;

        CanvasScript_inGame.EnterPrompt( promptOb_passed );
    }

    public void PromptReadNext( PromptObject promptOb_passed )
    {
        print($"{nameof(PromptReadNext)}, object: '{promptOb_passed.gameObject.name}'");

        CanvasScript_inGame.PromptReadNext( promptOb_passed );
    }

    public void ExitPrompt( PromptObject promptOb_passed )
    {
        CanvasScript_inGame.ExitPrompt( promptOb_passed );
        UnPauseGame();
    }

	IEnumerator level2Logic()
	{
		for (; ; )
		{
			if( MGR_BugEnemy.Instance != null )
            {
                MGR_BugEnemy.Instance.level2Logic_action( level2Logic_frequency );
            }

            if( MGR_BasicFlyer.Instance != null )
            {
               MGR_BasicFlyer.Instance.level2Logic_action( level2Logic_frequency );
            }

			yield return new WaitForSeconds( level2Logic_frequency );
		}
	}

	IEnumerator level3Logic()
	{
		for (; ; )
		{
			if( MGR_BugEnemy.Instance != null )
            {
                MGR_BugEnemy.Instance.level3Logic_action( level3Logic_frequency );
            }

			if ( MGR_BasicFlyer.Instance != null )
			{
				MGR_BasicFlyer.Instance.level3Logic_action( level3Logic_frequency );
			}

			yield return new WaitForSeconds(level3Logic_frequency);
		}
	}

	IEnumerator level4Logic()
	{
		for (; ; )
		{
			if ( MGR_BugEnemy.Instance != null )
			{
				MGR_BugEnemy.Instance.level4Logic_action( level4Logic_frequency );
			}

			if ( MGR_BasicFlyer.Instance != null )
			{
				MGR_BasicFlyer.Instance.level4Logic_action( level4Logic_frequency );
			}

			yield return new WaitForSeconds(level4Logic_frequency);

		}
	}

    public void DrawPlayModeGizmos()
    {

    }

    public void DrawEditorGizmos()
    {

    }

    public string GetDebugString()
    {
        string dbgString = "<b>[---- GAME MANAGER ----]</b>\n" +
            $"{nameof(myGameState)}: '{myGameState}'\n" +
            $"time.timeScale: '{Time.timeScale}'\n" +
            $"{nameof(GlobalTimeScale)}: '{GlobalTimeScale}'\n" +
            $"";

        return dbgString;
    }
}
