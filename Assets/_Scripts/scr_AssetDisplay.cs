using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_AssetDisplay : MonoBehaviour
{
    public List<Transform> ts;

    public float speed_rotation;

    void Start()
    {
        //ts = U.GetComponents_OnlyChildren( transform, transform, transform.Find("Canvas") ); //Unfortunately, this returns even heirarchical children...
    }

    void Update()
    {
        if( ts != null && ts.Count > 0 )
        {
            foreach( Transform t in ts )
            {
                t.Rotate(Vector3.up, speed_rotation*Time.deltaTime, Space.World);
            }
        }
    }
}
