
using UnityEngine;
using System.Collections;

public class ExampleScript : MonoBehaviour
{
	Camera cam;

	void Start()
	{
		cam = GetComponent<Camera>();
	}

	public bool RaycastDidHit = false;
	public bool MCcheckSuccess = false;

	public Vector3 MousePos;

	void Update()
	{
		RaycastDidHit = false;

		MousePos = Input.mousePosition;

		RaycastHit hit;
		if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
		{
			return;
		}
		else
		{
			RaycastDidHit = true;
		}

		MCcheckSuccess = false;
		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (meshCollider == null || meshCollider.sharedMesh == null)
		{
			return;
		}
		else
		{
			MCcheckSuccess = true;
		}

		//hit.collider.bounds.
		Mesh mesh = meshCollider.sharedMesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
		Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
		Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
		Transform hitTransform = hit.collider.transform;
		p0 = hitTransform.TransformPoint(p0);
		p1 = hitTransform.TransformPoint(p1);
		p2 = hitTransform.TransformPoint(p2);
		Debug.DrawLine(p0, p1);
		Debug.DrawLine(p1, p2);
		Debug.DrawLine(p2, p0);
	}
}