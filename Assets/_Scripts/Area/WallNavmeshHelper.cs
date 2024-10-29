using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is meant as a helper class for any future things I might want to do with the objects in the heirarchy 
/// that exist solely to generate wall navmeshes. Currently it doesn't do a whole lot.
/// </summary>
public class WallNavmeshHelper : MonoBehaviour
{
    [SerializeField] List<GameObject> gameObjects = new List<GameObject>();

    List<Collider> colliders = new List<Collider>();
    List<MeshRenderer> renderers = new List<MeshRenderer>();

    void Start()
    {
        foreach ( GameObject go in gameObjects )
        {
            colliders.Add( go.GetComponent<Collider>() );
            renderers.Add( go.GetComponent<MeshRenderer>() );
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
