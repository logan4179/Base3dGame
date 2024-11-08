using PV_Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PV_Pool : PV_Object
{
    [Header("---------------[[ REFERENCE ]]-----------------")]
    [SerializeField] private List<GameObject> pooledObjects = new List<GameObject>();

    [Header("---------------[[ FOR PREFAB INSTANTIATION ]]-----------------")]
    [SerializeField, Tooltip("Optional prefab to fill out the pooledObjects list with. If this is set, it will overwrite pooledObjects on start()")] 
    private GameObject poolPrefab;

    [Header("---------------[[ STATS ]]-----------------")]
    [SerializeField] private int count_allowedActiveInScene;
    private int index_lastMadeActive = -1;

    public void Init()
    {
        Log_MethodStart("Start() begin");
        Log( $"count_allowedActiveInScene: '{count_allowedActiveInScene}'" );

        if ( poolPrefab != null && count_allowedActiveInScene > 0 )
        {
            Log("conditions met for overriding pool based on supplied prefab. Creating pool...");
            pooledObjects = new List<GameObject>();
            for ( int i = 0; i < count_allowedActiveInScene; i++ )
            {
                GameObject g = GameObject.Instantiate( poolPrefab, transform );
                pooledObjects.Add( g );
                g.SetActive( false );
            }
        }
        else if ( pooledObjects != null && pooledObjects.Count > 0 )
        {
            Log( "pooledObjects list was set propertly. Setting all objects inactive..." );
            foreach ( GameObject g in pooledObjects )
            {
                g.SetActive( false );
            }
        }
    }

    public GameObject GetNext()
    {
        Log( $"CycleNext(), index_lastMadeActive: '{index_lastMadeActive}'" );

        //int nextPos = (index_lastMadeActive+1) >= pooledObjects.Count ? 0 : index_lastMadeActive+1;
        int nextPos = PV_Utilities.GetLoopedIndex(pooledObjects.Count, index_lastMadeActive + 1);
        GameObject go = pooledObjects[nextPos];
        go.SetActive(true);

        index_lastMadeActive = nextPos;
        return go;
    }

    public GameObject CycleNext(Vector3 pos_passed, Quaternion rot_passed)
    {
        Log( $"[overload]CycleNext(pos_passed: '{pos_passed}', rot_passed: '{rot_passed}'), index_lastMadeActive: '{index_lastMadeActive}'" );
        GameObject go = GetNext();

        go.transform.position = pos_passed;
        go.transform.rotation = rot_passed;
        return go;
    }
}
