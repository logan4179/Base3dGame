using LogansMultiEffectHandler;
using LogansPoolingSystem;
using UnityEngine;

public class PV_EffectPool : Pool
{
	protected MultiEffectHandler[] pooledMultiEffectHandlers;

	protected override void Awake()
	{
		base.Awake();

		if ( pooledObjects != null )
		{
			pooledMultiEffectHandlers = new MultiEffectHandler[pooledObjects.Length];

			for ( int i = 0; i < pooledObjects.Length; i++ )
			{
				pooledMultiEffectHandlers[i] = pooledObjects[i].GetComponent<MultiEffectHandler>();
			}
		}
	}

	public override GameObject CycleSpawn( Vector3 spawnPoint, Vector3 vNormal )
	{
		GameObject go = base.CycleSpawn( spawnPoint, vNormal );

		pooledMultiEffectHandlers[index_lastMadeActive].PlayAll();

		return go;
	}

	public override GameObject CycleSpawnExact( Vector3 pos_passed, Quaternion rot_passed )
	{
		GameObject go = base.CycleSpawnExact(pos_passed, rot_passed);

		print($"{nameof(index_lastMadeActive)}: '{index_lastMadeActive}'");
		pooledMultiEffectHandlers[index_lastMadeActive].PlayAll();

		return go;
	}
}