using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallNavmeshManager : MonoBehaviour
{
    [SerializeField] List<GameObject> gameObjects = new List<GameObject>();

    List<Collider> colliders = new List<Collider>();
    List<MeshRenderer> renderers = new List<MeshRenderer>();

    void Start()
    {
        foreach ( GameObject go in gameObjects )
        {
            colliders.Add(go.GetComponent<Collider>());
            renderers.Add(go.GetComponent<MeshRenderer>());
        }

        SetAllInactive();
        enabled = false;
    }

    [ContextMenu("call SetAllInactive()")]
    public void SetAllInactive()
    {
        foreach ( Collider col in colliders )
        {
            col.enabled = false;
        }
        
        foreach( MeshRenderer mr in renderers )
        {
            mr.enabled = false;
        }
    }

    [ContextMenu("call SetAllActive()")]
    public void SetAllActive()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        foreach (MeshRenderer mr in renderers)
        {
            mr.enabled = true;
        }
    }

}
