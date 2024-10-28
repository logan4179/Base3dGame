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
			print($"PV_EffectPool - there are '{pooledObjects.Length}' pooled objects");
			pooledMultiEffectHandlers = new MultiEffectHandler[pooledObjects.Length];

			for ( int i = 0; i < pooledObjects.Length; i++ )
			{
				pooledMultiEffectHandlers[i] = pooledObjects[i].GetComponent<MultiEffectHandler>();
			}
		}
	}

	public override GameObject CycleSpawnAtPosition(Vector3 pos_passed, Quaternion rot_passed)
	{
		GameObject go = base.CycleSpawnAtPosition(pos_passed, rot_passed);

		print($"{nameof(index_lastMadeActive)}: '{index_lastMadeActive}'");
		pooledMultiEffectHandlers[index_lastMadeActive].PlayAll();

		return go;
	}
}