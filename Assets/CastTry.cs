using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastTry : MonoBehaviour
{
    /// <summary>0 = raycast, 1 = boxcast, </summary>
    public int mode = 0;
    public int qMode = 0;
    public float rayDistance = 2f;

    public Color RayColor = Color.green;

    Ray rayForward;
    public RaycastHit rcHit;

    public bool AmIntersecting;

    public Vector3 vDBG;

    public GameObject CubeVisual;

    void Start()
    {
        if( CubeVisual != null )
        {
            CubeVisual.transform.localScale = new Vector3(1f, 1f, rayDistance);
        }
    }

    void Update()
    {
        //rayForward = new Ray( transform.position, transform.TransformPoint(Vector3.forward * rayDistance) );
        //rayForward = new Ray( transform.position, transform.position + transform.forward );
        rayForward = new Ray( transform.position, transform.forward );


        AmIntersecting = false;

        if ( mode == 0 )
        {
            if ( Physics.Raycast(rayForward, out rcHit, rayDistance, PV_Environment.Instance.Mask_WalkableJumpable) )
            {
                AmIntersecting = true;
                
            }

            if ( CubeVisual != null && CubeVisual.gameObject.activeSelf )
            {
                CubeVisual.gameObject.SetActive(false);
            }
        }
        else if( mode == 1 )
        {
            Quaternion qRot = Quaternion.identity;

            if( qMode == 1 )
            {
                qRot = transform.rotation; //works
            }
            else if( qMode == 2 )
            {
                qRot = Quaternion.FromToRotation(Vector3.forward, transform.forward);
            }
            else if( qMode == 3 )
            {
                //qRot = Quaternion.
            }

            if ( Physics.BoxCast(transform.position, (Vector3.one*0.5f), transform.forward, out rcHit, qRot, rayDistance, PV_Environment.Instance.Mask_WalkableJumpable) )
            {
                AmIntersecting = true;
            }

            if (CubeVisual != null)
            {
                CubeVisual.transform.position = transform.position + (transform.forward*(0.5f*rayDistance) );
                CubeVisual.transform.rotation = transform.rotation;

                if ( !CubeVisual.gameObject.activeSelf )
                {
                    CubeVisual.gameObject.SetActive( true );

                }
            }

        }       
        
       // Debug.DrawRay( transform.position, transform.TransformPoint(Vector3.forward*rayDistance), RayColor );
        Debug.DrawRay( transform.position, transform.forward * rayDistance, AmIntersecting ? RayColor : Color.white );
        vDBG = transform.TransformPoint(transform.forward);

    }

    private void OnDrawGizmos()
    {
        if( mode == 0 )
        {

        }
        else if ( mode == 1 )
        {
            //Gizmos.DrawCube( transform.TransformPoint(Vector3.forward*2f), Vector3.one*2f );
        }
    }
}
