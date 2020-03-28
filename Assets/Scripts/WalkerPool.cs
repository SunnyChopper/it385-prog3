using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkerPool : MonoBehaviour
{

	[SerializeField] GameObject wellWalkerPrefab;
	[SerializeField] GameObject citizenPrefab;

	private ScenarioMgr scenario;

	public const float walkerHorizOffset = 0.5f;
	public const float walkerVertOffset = 0.2f;

	// Use this for initialization
	void Start ()
	{
		
		scenario = gameObject.GetComponent ("ScenarioMgr") as ScenarioMgr;

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public GameObject GetWellWalker (IntPoint2D tileLoc, ScenarioMgr.Direction facing)
	{
		//Debug.Log ("Retrieving well walker");
		Vector3 topLeft = scenario.ComputeTopLeftPointOfTile (tileLoc);
		GameObject walker;

		walker = (GameObject)Instantiate (wellWalkerPrefab);
		walker.transform.position = topLeft + new Vector3 (walkerHorizOffset, walkerVertOffset, walkerHorizOffset);
		walker.transform.rotation = Quaternion.identity;
		if (facing == ScenarioMgr.Direction.Up)
			walker.transform.Rotate (0, 90, 0);
		else if (facing == ScenarioMgr.Direction.Left)
			walker.transform.Rotate (0, 180, 0);
		else if (facing == ScenarioMgr.Direction.Down)
			walker.transform.Rotate (0, 270, 0);

		walker.SetActive (true);
		return walker;
	}

	public GameObject GetCitizen (IntPoint2D tileLoc, ScenarioMgr.Direction facing)
	{
		Vector3 topLeft = scenario.ComputeTopLeftPointOfTile (tileLoc);
		GameObject walker;

		walker = (GameObject)Instantiate (citizenPrefab);
		walker.transform.position = topLeft + new Vector3 (walkerHorizOffset, walkerVertOffset, walkerHorizOffset);
		walker.transform.rotation = Quaternion.identity;
		if (facing == ScenarioMgr.Direction.Up)
			walker.transform.Rotate (0, 90, 0);
		else if (facing == ScenarioMgr.Direction.Left)
			walker.transform.Rotate (0, 180, 0);
		else if (facing == ScenarioMgr.Direction.Down)
			walker.transform.Rotate (0, 270, 0);
		
		walker.SetActive (true);
		return walker;
	}

}
