using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavmeshBakeHelper : MonoBehaviour
{
	[SerializeField] private string tagString_navmeshIslandOccluders;
	[SerializeField] private GameObject[] gmObs_navmeshOccluders_islands;
	[Space(10f)]

	[SerializeField] private string tagString_navmeshOccluders_positiveY;
    [SerializeField] private GameObject[] gmObs_navmeshOccluders_positiveY;
	[Space(10f)]

	/*
	[SerializeField] private string tagString_navmeshOccluders_positiveX;
	[SerializeField] private GameObject[] gmObs_navmeshOccluders_positiveX;
	[Space(10f)]

	[SerializeField] private string tagString_navmeshOccluders_negativeX;
	[SerializeField] private GameObject[] gmObs_navmeshOccluders_negativeX;
	[Space(10f)]

	[SerializeField] private string tagString_navmeshOccluders_positiveZ;
	[SerializeField] private GameObject[] gmObs_navmeshOccluders_positiveZ;
	[Space(10f)]

	[SerializeField] private string tagString_navmeshOccluders_negativeZ;
	[SerializeField] private GameObject[] gmObs_navmeshOccluders_negativeZ;
	//[Space(10f)]
	*/

	[SerializeField] private string tagString_navmeshOccluders_wall;
	[SerializeField] private GameObject[] gmObs_navmeshOccluders_wall;

	void Start()
    {
		foreach ( GameObject go in gmObs_navmeshOccluders_positiveY )
		{
			Destroy(go);
		}

		foreach ( GameObject go in gmObs_navmeshOccluders_wall )
		{
			Destroy(go);
		}
	}

	[ContextMenu("z call FetchAllOccluders()")]
	public void FetchAllOccluders()
	{
		gmObs_navmeshOccluders_positiveY = GameObject.FindGameObjectsWithTag( tagString_navmeshOccluders_positiveY );

		/*
		gmObs_navmeshOccluders_negativeX = GameObject.FindGameObjectsWithTag( tagString_navmeshOccluders_negativeX );
		gmObs_navmeshOccluders_positiveX = GameObject.FindGameObjectsWithTag( tagString_navmeshOccluders_positiveX );

		gmObs_navmeshOccluders_positiveZ = GameObject.FindGameObjectsWithTag( tagString_navmeshOccluders_positiveZ );
		gmObs_navmeshOccluders_negativeZ = GameObject.FindGameObjectsWithTag( tagString_navmeshOccluders_negativeZ );
		*/

		gmObs_navmeshOccluders_wall = GameObject.FindGameObjectsWithTag( tagString_navmeshOccluders_wall );

		gmObs_navmeshOccluders_islands = GameObject.FindGameObjectsWithTag( tagString_navmeshIslandOccluders );

	}

	[ContextMenu("z call PrepForFloorBake()")]
	public void PrepForFloorBake()
	{
		EnablePositiveYOccluders();
		DisableWallOccluders();
	}

	[ContextMenu("z call PrepForWallBake()")]
	public void PrepForWallBake()
	{
		DisablePositiveYOccluders();
		EnableWallOccluders();
	}

	[ContextMenu("z call EnablePositiveYOccluders()")]
	public void EnablePositiveYOccluders()
	{
		foreach( GameObject go in gmObs_navmeshOccluders_positiveY )
		{
			go.SetActive( true );
		}
	}

	[ContextMenu("z call DisablePositiveYOccluders()")]
	public void DisablePositiveYOccluders()
	{
		foreach ( GameObject go in gmObs_navmeshOccluders_positiveY )
		{
			go.SetActive( false );
		}
	}

	[ContextMenu("z call EnableWallOccluders()")]
	public void EnableWallOccluders()
	{
		foreach ( GameObject go in gmObs_navmeshOccluders_wall )
		{
			go.SetActive( true );
		}
	}

	[ContextMenu("z call DisableWallOccluders()")]
	public void DisableWallOccluders()
	{
		foreach ( GameObject go in gmObs_navmeshOccluders_wall )
		{
			go.SetActive( false );
		}
	}
}
