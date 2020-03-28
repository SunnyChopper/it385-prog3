using UnityEngine;
using System.Collections;

public class ObjectManager : MonoBehaviour {

	public GameObject[] greenPrefabs;
	public GameObject[] redPrefabs;
	public GameObject[] actualPrefabs;
	public float[] leftOffset;
	public float[] rightOffset;
	public float[] vertOffset;
	public int[] objectSize;
	private ScenarioMgr scenario;

	// Use this for initialization
	void Start () {
		scenario = gameObject.GetComponent("ScenarioMgr") as ScenarioMgr;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GameObject GetGreenGhost(int arrayIndex, IntPoint2D tileLoc)
	{
		Vector3 topLeft = scenario.ComputeTopLeftPointOfTile (tileLoc);
		GameObject obj = (GameObject) Instantiate(greenPrefabs[arrayIndex]);
		obj.transform.position=topLeft+GetObjTransform(arrayIndex);
		return obj;
	}

	public GameObject GetRedGhost(int arrayIndex, IntPoint2D tileLoc)
	{
		Vector3 topLeft = scenario.ComputeTopLeftPointOfTile (tileLoc);
		GameObject obj = (GameObject) Instantiate(redPrefabs[arrayIndex]);
		obj.transform.position=topLeft+GetObjTransform(arrayIndex);
		return obj;
	}

	public GameObject GetActualObject(int arrayIndex, IntPoint2D tileLoc)
	{
		Vector3 topLeft = scenario.ComputeTopLeftPointOfTile (tileLoc);
		GameObject obj = (GameObject) Instantiate(actualPrefabs[arrayIndex]);
		obj.transform.position=topLeft+GetObjTransform(arrayIndex);
		return obj;
	}

	// the standard transformation of the object for a given item from the topleft of the tile the mouse is in
	public Vector3 GetObjTransform(int arrayIndex)
	{
		return new Vector3(leftOffset[arrayIndex],vertOffset[arrayIndex],rightOffset[arrayIndex]);
	}

	public int GetObjSize(int arrayIndex)
	{
		return objectSize[arrayIndex];
	}
}
