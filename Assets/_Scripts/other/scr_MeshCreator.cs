using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_MeshCreator : MonoBehaviour
{
    public Material material;
    public GameObject gOb;

    void Start()
    {
        print("start from meshcreator");
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0,1);
        vertices[1] = new Vector3(1,1);
        vertices[2] = new Vector3(0,0);
        vertices[3] = new Vector3(1,0);

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0,1);
        uvs[1] = new Vector2(1,1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1,0);

        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        gOb = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        gOb.transform.localScale = new Vector3(30, 30, 1);
        gOb.GetComponent<MeshFilter>().mesh = mesh;
        gOb.GetComponent<MeshRenderer>().material = material;
    }

    void Update()
    {
        
    }
}
