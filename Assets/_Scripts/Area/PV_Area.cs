using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PV_Area : MonoBehaviour
{
    [Header("[---- REFERENCE ----]")]
    [SerializeField] private GameObject gOb_Area;
    [SerializeField] private GameObject[] gObs_cachedNavmeshOccluders;

    [Header("[---- OTHER ----]")]
    [SerializeField] private string tag_navMeshOccluders = "NavMeshOccluder";

	void Start()
    {
        foreach ( GameObject go in gObs_cachedNavmeshOccluders )
        {
            Destroy( go );
        }
    }

    [ContextMenu("z call FetchNavMeshOccluders()")]
    public void FetchNavMeshOccluders()
    {
        Debug.Log($"FetchNavMeshOccluders()...");
        gObs_cachedNavmeshOccluders = GameObject.FindGameObjectsWithTag( tag_navMeshOccluders );

        Debug.Log($"found '{gObs_cachedNavmeshOccluders.Length}' occluders..." );
    }
}
