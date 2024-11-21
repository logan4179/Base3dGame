using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using PV_Enums;
using PV_Utils;
using PV_DebugUtils;
using LogansReverbManagementSystem;
using System.Collections;
using LogansAreaManagementSystem;

/// <summary>
/// This is for things like debug drawing. This needed to be a separate class from the debug manager (PV_Debug) to separate responsibilities because
/// PV_Debug will exist in the build, but this will not.
/// </summary>
public class PV_SceneDebugger : MonoBehaviour
{
	public static PV_SceneDebugger Instance;

	public Stats_debug MyStats;
	/// <summary>
	/// Stats object for general living entity debugging.
	/// </summary>
	public Stats_DebugLiving MyStats_debugLiving;

	[Header("[------ PLAYER ------]")]
	private bool debugPlayerToggle = false;
	[SerializeField] private PV_Player playerScript;
	private Player_debugInfo _playerDebugInfo;

	[SerializeField] private Transform trans_testDmgOnPlr;

	[Header("[------ BUG ENEMIES ------]")]
	[SerializeField, Tooltip("Orders all enemies to go into \"debug mode\" whenever the debug toggle is triggered on the debug canvas. Only relevant in play mode")]
	private bool debugAllBugEnemiesToggle = false;
	[SerializeField, Tooltip("This is for referencing the bug manager in the inspector, which can't use the bugmanager's singleton instance, because it's static and doesn't exist in the editor")]
	private MGR_BugEnemy BugManager_cached;
	[SerializeField] private List<Enemy_bug_debugInfo> info_registeredBugEnemies;

	[Header("[------ BASIC FLYER ENEMIES ------]")]
	[SerializeField, Tooltip("Orders all enemies to go into \"debug mode\" whenever the debug toggle is triggered on the debug canvas. Only relevant in play mode")]
	private bool debugAllBasicFlyerEnemiesToggle = false;
	[SerializeField, Tooltip("This is for referencing the basic flyer manager in the inspector, which can't use the bugmanager's instance, because it's static and doesn't exist in the editor")]
	private MGR_BasicFlyer BasicFlyerManager_cached;
	[SerializeField] private List<Enemy_basicFlyer_debugInfo> info_registeredBasicFlyerEnemies;

	[Header("[------ ENVIRONMENTAL ------]")]
	[SerializeField, Tooltip("Orders all environmentals to go into \"debug mode\" whenever the debug toggle is triggered on the debug canvas. Only relevant in play mode")]
	private bool debugEnvironmentToggle = false;
	[SerializeField] private PV_Environment environmentManager_cached;
	[SerializeField] private List<SphereCollider> sphereColliders_debug_pickup, sphereColliders_debug_prompt;
	[SerializeField] private List<BoxCollider> boxColliders_debug_area;
	public List<ReverbFollower> reverbFollowers = new List<ReverbFollower>();
	public List<ReverbZoneManager> reverbZoneManagers = new List<ReverbZoneManager>();

	//[Header("[------ TRUTH ------]")]
	public static bool SlowAttacksOn = false;

	[Header("[------ OTHER ------]")]
	[Tooltip("If enabled, runs debug gizmos on an item that is selected.")] 
	public bool DebugOnSelection = true;
	[Tooltip("Assign this instance through the inspector. This is so that you can debug in the editor wihtout having to rely on the env manager's singleton.")]
	PV_Environment EnvironmentDebugInstance;

	[ContextMenu("say count")]
	public void SayCount()
	{
		if (info_registeredBugEnemies == null)
		{
			print("list is null");
		}
		else
		{
			print($"info_registeredBugEnemies count: '{info_registeredBugEnemies.Count}'");

		}
	}
	private void OnEnable()
	{
		if (BugManager_cached == null)
		{
			Debug.LogWarning($"{nameof(PV_SceneDebugger)}.{nameof(BugManager_cached)} reference was null. Was this intentional?");
		}

		if (BasicFlyerManager_cached == null)
		{
			Debug.LogWarning($"{nameof(PV_SceneDebugger)}.{nameof(BasicFlyerManager_cached)} reference was null. Was this intentional?");
		}

		if (environmentManager_cached == null)
		{
			Debug.LogWarning($"{nameof(PV_SceneDebugger)}.{nameof(environmentManager_cached)} reference was null. Was this intentional?");
		}
	}

	private void Awake()
	{
		Instance = this;

		sphereColliders_debug_pickup = new List<SphereCollider>();
		sphereColliders_debug_prompt = new List<SphereCollider>();

	}

	void Start()
	{
		BugEnemies_DebugAction(false);

		#region AREAS --------------------------------------///////////////
		foreach (LAMS_Area area in PV_Environment.Instance.AllAreas)
		{
			//area.DebugAction();
		}
		#endregion
	}
	private void Update()
	{
		if (playerScript != null && Input.GetKeyDown(KeyCode.Mouse2)) //tests player damage
		{
			playerScript.TakeDmg( 10, 0f, Vector3.zero, new RaycastHit() );
		}
	}

	[ContextMenu("z call FetchAll()")]
	public void FetchAll()
	{
		FetchBugEnemiesInScene();

		FetchBasicFlyerEnemiesInScene();

		FetchEnvironmentals();

		if (playerScript == null)
		{
			Debug.LogError("Remember that you need to manually assign the player script through the inspector...");
		}
		else
		{
			_playerDebugInfo = new Player_debugInfo(playerScript); //go ahead and refresh this here, because I need a way to trigger this.
		}

		foreach (Enemy_bug_debugInfo info in info_registeredBugEnemies)
		{
			Debug.DrawLine(info.MyEnemyReference.transform.position, info.MyEnemyReference.transform.position + (Vector3.up * 2f), Color.yellow, 3f);
		}
	}

	[ContextMenu("z call FetchBugEnemiesInScene()")]
	public void FetchBugEnemiesInScene()
	{
		PV_Debug.Log($"Fetching bug enemies tagged: '{PV_GameManager.Tag_bugEnemy}'");
		List<Enemy_Bug> foundEnemies =
			PV_Utilities.GetComponentsWithTag_inScene<Enemy_Bug>(PV_GameManager.Tag_bugEnemy);
		info_registeredBugEnemies = new List<Enemy_bug_debugInfo>();

		foreach (Enemy_Bug bug in foundEnemies)
		{
			info_registeredBugEnemies.Add(new Enemy_bug_debugInfo(bug));
		}

		PV_Debug.Log($"Found '{info_registeredBugEnemies.Count}' bug enemies.");
	}

	/// <summary>
	/// Use this method when removing a bug enemy from the scene via the editor
	/// (IE: when NOT in play mode)
	/// </summary>
	/// <param name="bug"></param>
	public void RemoveBugFromSceneViaEditor(Enemy_Bug bug)
	{
		if (!Application.isEditor)
		{
			PV_Debug.LogError("This method is not intended for runtime removal. Use the manager classes for that instead.");
			return;
		}

		Enemy_bug_debugInfo foundBugInfo = null;
		foreach (Enemy_bug_debugInfo bugInfo in info_registeredBugEnemies)
		{
			if (bugInfo.MyEnemyReference == bug)
			{
				foundBugInfo = bugInfo;
				break;
			}
		}

		if (foundBugInfo != null)
		{
			PV_Debug.Log("Found matching bug info instance. Removing from SceneDebuger...");
			info_registeredBugEnemies.Remove(foundBugInfo);
		}
		else
		{
			PV_Debug.Log("Matching bug info instance was NOT found in the SceneDebugger...");
		}
	}

	[ContextMenu("z call FetchBasicFlyerEnemiesInScene()")]
	public void FetchBasicFlyerEnemiesInScene()
	{
		List<Enemy_BasicFlyer> foundEnemies = PV_Utilities.GetComponentsWithTag_inScene<Enemy_BasicFlyer>(PV_GameManager.Tag_basicFlyerEnemy);
		info_registeredBasicFlyerEnemies = new List<Enemy_basicFlyer_debugInfo>();

		foreach (Enemy_BasicFlyer flyr in foundEnemies)
		{
			info_registeredBasicFlyerEnemies.Add(new Enemy_basicFlyer_debugInfo(flyr));
		}

		Debug.Log($"Found '{info_registeredBasicFlyerEnemies.Count}' basic flyer enemies.");
	}
	private void FetchGenericEnemiesInScene<T>()
	{

	}

	#region ENVIRONMENT---------------------------------------------
	[ContextMenu("z call FetchEnvironmentals()")]
	public void FetchEnvironmentals()
	{
		if (environmentManager_cached == null)
		{
			Debug.LogWarning($"{nameof(PV_SceneDebugger)}.{nameof(environmentManager_cached)} reference was null. Was this intentional?");
		}
		else
		{
			sphereColliders_debug_pickup = PV_Utilities.GetComponentsWithTag_inScene<SphereCollider>("Pickup");
			sphereColliders_debug_prompt = PV_Utilities.GetComponentsWithTag_inScene<SphereCollider>("Prompt");

			boxColliders_debug_area = PV_Utilities.GetComponentsWithTag_inScene<BoxCollider>("PV_Area");
			Debug.Log($"Found '{sphereColliders_debug_pickup.Count}' pickups, and '{sphereColliders_debug_prompt.Count}' prompts");
		}
	}

	public void Environment_debugAction(bool tglVAl)
	{
		Debug.Log($"{nameof(Environment_debugAction)}({tglVAl})");
		debugEnvironmentToggle = tglVAl;
	}
	#endregion

	public void Player_DebugAction(bool tglVal)
	{
		debugPlayerToggle = tglVal;
	}

	public void BugEnemies_DebugAction(bool tglVal)
	{
		debugAllBugEnemiesToggle = tglVal;
	}

	public void BasicFlyerEnemies_DebugAction(bool tglVal)
	{
		debugAllBugEnemiesToggle = tglVal;
	}

	void OnDrawGizmos()
	{
		// gos = Selection.gameObjects;
		//print(gos.Length);
		/*if (Selection.activeGameObject != null && Selection.activeGameObject.CompareTag(PV_GameManager.Tag_bugEnemy))
        {
            print("not null");
            if ( Selection.activeGameObject.CompareTag(PV_GameManager.Tag_bugEnemy) )
            {
                print("round 2");
            }
        }*/

		if (DebugOnSelection && Selection.activeGameObject != null)
		{
			if (Selection.activeGameObject.CompareTag(PV_GameManager.Tag_bugEnemy) && !debugAllBugEnemiesToggle)
			{
				if (info_registeredBugEnemies != null && info_registeredBugEnemies.Count > 0)
				{
					foreach (Enemy_bug_debugInfo info in info_registeredBugEnemies)
					{
						if (info.MyEnemyReference.gameObject == Selection.activeGameObject)
						{
							info.DrawDebugVisuals(MyStats_debugLiving);


						}
					}
				}
			}
			else if (Selection.activeGameObject.CompareTag(PV_GameManager.Tag_basicFlyerEnemy) && !debugAllBasicFlyerEnemiesToggle)
			{
				if (info_registeredBasicFlyerEnemies != null && info_registeredBasicFlyerEnemies.Count > 0)
				{
					foreach (Enemy_basicFlyer_debugInfo info in info_registeredBasicFlyerEnemies)
					{
						if (info.MyEnemyReference.gameObject == Selection.activeGameObject)
						{
							info.DrawDebugVisuals(MyStats_debugLiving);


						}
					}
				}
			}
		}

		if ( Application.isPlaying )
		{
			if ( debugEnvironmentToggle )
			{
				if ( sphereColliders_debug_pickup != null && sphereColliders_debug_pickup.Count > 0 )
				{
					Gizmos.color = MyStats.Color_PickupObjectCollider_debug;
					foreach ( SphereCollider sc in sphereColliders_debug_pickup )
					{
						Gizmos.DrawSphere( sc.transform.position, sc.radius );
					}
				}

				if ( sphereColliders_debug_prompt != null && sphereColliders_debug_prompt.Count > 0 )
				{
					Gizmos.color = MyStats.Color_PromptObjectCollider_debug;
					foreach ( SphereCollider sc in sphereColliders_debug_prompt )
					{
						Gizmos.DrawSphere( sc.transform.position, sc.radius );
					}
				}

				if ( reverbFollowers != null && reverbFollowers.Count > 0 )
				{
					Gizmos.color = MyStats.Color_ReverbZone_debug;

					foreach ( ReverbFollower rf in reverbFollowers )
					{
						rf.DrawDebugVisuals();
					}
				}

				if ( reverbZoneManagers != null && reverbZoneManagers.Count > 0 )
				{
					Gizmos.color = MyStats.Color_ReverbZone_debug;

					foreach ( ReverbZoneManager rzm in reverbZoneManagers )
					{
						//rzm.DrawDebugVisuals(); //todo...
					}
				}

				if ( boxColliders_debug_area != null && boxColliders_debug_area.Count > 0 )
				{
					Gizmos.color = MyStats.Color_AreaTrigger_debug;
					foreach ( BoxCollider bc in boxColliders_debug_area )
					{
						Gizmos.DrawCube( 
							bc.transform.position, 
							new Vector3(bc.size.x * bc.transform.localScale.x, bc.size.y * bc.transform.localScale.y, bc.size.z * bc.transform.localScale.z)
							);
					}
				}
			}


			if (debugPlayerToggle && playerScript != null)
			{
				if (_playerDebugInfo == null)
				{
					_playerDebugInfo = new Player_debugInfo(playerScript);
				}


				_playerDebugInfo.DrawDebugVisuals(MyStats_debugLiving);
			}


			if (debugAllBugEnemiesToggle && info_registeredBugEnemies != null && info_registeredBugEnemies.Count > 0)
			{
				foreach (Enemy_bug_debugInfo info in info_registeredBugEnemies)
				{
					info.DrawDebugVisuals(MyStats_debugLiving);
				}
			}


			if (debugAllBasicFlyerEnemiesToggle && info_registeredBasicFlyerEnemies != null && info_registeredBasicFlyerEnemies.Count > 0)
			{
				foreach (Enemy_basicFlyer_debugInfo info in info_registeredBasicFlyerEnemies)
				{
					info.DrawDebugVisuals(MyStats_debugLiving);
				}
			}
		}
	}

	[ContextMenu("z call MakePlayerDie()")]
	public void MakePlayerDie()
	{
		if (playerScript == null)
		{
			Debug.LogWarning($"{nameof(playerScript)} was null. Can't make player die...");
			return;
		}

		StartCoroutine($"{nameof(TestPlayerDeath)}");
	}
	public IEnumerator TestPlayerDeath()
	{
		yield return new WaitForSeconds(0.35f);

		playerScript.TakeDmg( 110, 0f, Vector3.zero, new RaycastHit() );
	}

	public void TestPlayerDamage()
	{
		RaycastHit rcHit_dmg;
		Ray ray_bullet = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
		/*
		if (amDebuggingScript) Debug.DrawRay(ray_bullet.origin, ray_bullet.direction * MyStats.dist_BulletRaycast, Color.white, 1f);

		if (Physics.Raycast(ray_bullet, out rcHit_dmg, MyStats.dist_BulletRaycast, PV_Environment.Instance.Mask_Shootable))
		{
			int lyr = rcHit_dmg.transform.gameObject.layer;

			PV_Debug.LogWithConsoleConditional($"HIT object: '{rcHit_dmg.transform.name}', layer: '{lyr}'.", amDebuggingScript);
			if (lyr == PV_Environment.Instance.Layer_Solid_Environmental)
			{
				PV_Debug.LogWithConsoleConditional("hit solid environment", amDebuggingScript);

				//GameObject go_gunshot = go_gunshot = Instantiate(pfb_DclPrjr_BulletHole, rcHit_Bullet.point + (rcHit_Bullet.normal * 0.1f), Quaternion.AngleAxis(Random.Range(0f, 360f), rcHit_Bullet.normal) * Quaternion.LookRotation(-rcHit_Bullet.normal), PV_Environment.Instance.transform);
				GameObject go_gunshot = PV_Environment.Instance.Pool_gunshotDecalsA.CycleSpawnAtPosition(rcHit_dmg.point + (rcHit_dmg.normal * 0.1f), Quaternion.AngleAxis(Random.Range(0f, 360f), rcHit_dmg.normal) * Quaternion.LookRotation(-rcHit_dmg.normal));

			}
			else if (lyr == PV_Environment.Instance.Layer_Living)
			{
				PV_Debug.LogWithConsoleConditional("hit living", amDebuggingScript);

				Base_living living_hit = rcHit_dmg.transform.gameObject.GetComponent<Base_living>();

				if (living_hit.Flag_amEnemy)
				{
					living_hit.TakeDmg(MyStats.Amount_Damage, MyStats.Force_Damage, transform.position, rcHit_dmg);
				}
			}

		        TakeDmg(5, 0f, Vector3.zero, rcHit );

		}*/
	}

	public void DebugAnimator(Animator anim_passed)
	{
		string s = "<color=yellow><b>---------ANIMATION REPORT---------</b></color>\n";
		for (int i = 0; i < anim_passed.layerCount; i++)
		{
			s += $"<b>Layer: '{anim_passed.GetLayerName(i)}' \n</b>" +
			"{\n" +
				 $"\t Current State: '{anim_passed.GetCurrentAnimatorStateInfo(i)}' \n";
			s += "\t{\n";
			s += "\t\t Clips: \n\t\t{\n";
			foreach (AnimatorClipInfo item in anim_passed.GetCurrentAnimatorClipInfo(i))
			{
				s += $"\t\t\t clip: {item.clip.name} \n";
			}
			s += "\t\t}\n";
			s += "\t{\n";
			s += "}\n";

		}

		Debug.Log(s);
	}

}



