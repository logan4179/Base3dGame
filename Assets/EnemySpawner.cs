using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[SerializeField] private PV_Environment EnvironmentManagerInstance;
	[SerializeField] private MGR_BugEnemy BugManagerInstance;

	[Header("REFERENCE - EXTERNAL")]
	[SerializeField] private GameObject prefab_bugEnemy;
	[SerializeField] private Enemy_Bug focusedBug;

	private void Awake()
	{

	}

	void Start()
    {
		BugManagerInstance = MGR_BugEnemy.Instance;
		EnvironmentManagerInstance = PV_Environment.Instance;

	}

	[ContextMenu("z call SpawnBug()")]
	public Enemy_Bug SpawnBug()
	{
		if ( prefab_bugEnemy == null )
		{
			PV_Debug.LogError($"{nameof(MGR_BugEnemy)}.{nameof(prefab_bugEnemy)} reference was null! Returning early...");
			return null;
		}

		Enemy_Bug bug = PV_Utilities.ConnectedInstantiate(prefab_bugEnemy, BugManagerInstance.transform, transform).GetComponent<Enemy_Bug>();
		BugManagerInstance.RegisterBug( bug );

		try
		{
			bug.Trans_patrolPointGrabber.GetComponent<PatrolPointGrabber>().cachedEnvironmentManagerReference =
				GameObject.FindGameObjectWithTag(PV_GameManager.Tag_EnvironmentManager).GetComponent<PV_Environment>();
		}
		catch (System.Exception)
		{
			PV_Debug.LogWarning($"Couldn't find the environment manager by tag. You'll need to manually set this for reference in the Patrol Point Grabber.");
		}

		PV_Debug.LogWarning($"You'll now need to explicitely set this enemy's triggering area variable AND patrol points.");

		return bug;
	}

	[ContextMenu("z call RemoveBugFromScene()")]
	public void RemoveBugFromScene()
	{
		BugManagerInstance.RemoveBugFromManager(focusedBug);
	}
}
