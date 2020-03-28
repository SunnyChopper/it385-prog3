using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HousePool : MonoBehaviour {

	[SerializeField] GameObject greenPrefab;
	[SerializeField] GameObject redPrefab;
	[SerializeField] GameObject actualHousePrefab;
	private ScenarioMgr scenario;
	public const float houseOffset = 0.5f;

	// Use this for initialization
	void Start () {
		scenario = gameObject.GetComponent("ScenarioMgr") as ScenarioMgr;
	}
	
	public GameObject GetGreenGhost (IntPoint2D tileLoc)
	{
		GameObject house = this.CreateHouse (greenPrefab, tileLoc);
		house.SetActive (true);
		return house;
	}
	
	public GameObject GetRedGhost (IntPoint2D tileLoc)
	{
		GameObject house = this.CreateHouse (redPrefab, tileLoc);
		house.SetActive (true);
		return house;
	}
	
	public GameObject GetActualHouse (IntPoint2D tileLoc)
	{
		GameObject house = this.CreateHouse (actualHousePrefab, tileLoc);
		house.SetActive (true);
		return house;
	}

	
	private GameObject CreateHouse (GameObject prefabHouse, IntPoint2D tileIndex)
	{
		Vector3 topLeft = scenario.ComputeTopLeftPointOfTile (tileIndex);
		
		GameObject house = (GameObject)Instantiate (prefabHouse);
		house.transform.position = topLeft + new Vector3 (houseOffset, houseOffset, houseOffset);

		return house;
	}

}
