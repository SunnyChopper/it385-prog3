using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeliveryManager : MonoBehaviour
{

    [SerializeField] float tileMoveTime;
    [SerializeField] float turnTime;

    private ScenarioData scenarioInfo;
	private IntPoint2D curTile;
	private ScenarioMgr.Direction facing = ScenarioMgr.Direction.Up;
	private StoreManager destStore;
	private Stack<IntPoint2D> path;
	private int amtToDeliver;
	private bool haveDestStore;



	public void SetUp (ScenarioData scenario, IntPoint2D startTile, Stack<IntPoint2D> path, StoreManager store, int amount)
	{
		//Debug.Log ("in delivery set up");
		scenarioInfo = scenario;
		this.path = path;
		curTile = startTile;
		destStore = store;
		amtToDeliver = amount;
		haveDestStore = true;
		StartCoroutine ("TakePath");
	}

	private bool GetPathToStore ()
	{
		List<IntPoint2D> startTiles = new List<IntPoint2D> ();
		startTiles.Add (curTile);
		List<IntPoint2D> endTiles = scenarioInfo.GetAdjacentRoadTiles (destStore.GetLoc ());
		Stack<IntPoint2D> newPath = scenarioInfo.ShortestPath (startTiles, endTiles);
		if (newPath.Count > 0)
		{
			newPath.Pop ();
			path = newPath;
			return true;
		} else
		{
			return GetPathToAllStores ();
		}

	}

	private bool GetPathToAllStores ()
	{
		GameObject[] stores = GameObject.FindGameObjectsWithTag ("Store");
		if (stores.Length > 0)
		{
			List<IntPoint2D> startTiles = new List<IntPoint2D> ();
			startTiles.Add (curTile);
			List<IntPoint2D> endTiles;
			Stack<IntPoint2D> bestPath = new Stack<IntPoint2D> ();
			StoreManager bestmgr = null;
			foreach (GameObject store in stores)
			{
				//  get the store manager
				StoreManager mgr = store.GetComponent ("StoreManager") as StoreManager;
				// see if there's available capacity
				if (mgr.GetRoom () > 0)
				{
					// get adjacent roads
					endTiles = scenarioInfo.GetAdjacentRoadTiles (mgr.GetLoc ());
					Stack<IntPoint2D> newPath = scenarioInfo.ShortestPath (startTiles, endTiles);
					if (newPath.Count > 0 && (bestPath.Count == 0 || newPath.Count < bestPath.Count))
                    {
						bestPath = newPath;
						bestmgr = mgr;
					}
				}

			}

			bool havePath = false;
			if (bestPath.Count > 0)
			{
				havePath = true;
				destStore = bestmgr;
				bestPath.Pop ();
				path = bestPath;
				haveDestStore = true;

			}
			return havePath;
		} else
			return false;
	}

	IEnumerator TakePath ()
	{
		bool havePath = true;
		bool stillTraveling = true;
		Vector3 startPos = gameObject.transform.position, endPos = startPos;
		Quaternion startAngle = gameObject.transform.rotation;
		Quaternion endAngle = startAngle;
		while (stillTraveling)
		{
			if (havePath && path.Count == 0)
			{
				// handle arrival at store
				amtToDeliver = destStore.ReceiveGoods (amtToDeliver);
				if (amtToDeliver == 0)
				{
					stillTraveling = false;
					Destroy(gameObject, 0.1f);
				} else
				{
					haveDestStore = havePath = false;
				}
					
			}
			while (stillTraveling && !havePath)
			{
				if (haveDestStore)
				{
					havePath = GetPathToStore ();
				} else
				{
					havePath = GetPathToAllStores ();
				}
				if (!havePath)
					yield return new WaitForSeconds (1);
			}
			if (stillTraveling && havePath)
			{
				
				IntPoint2D nextTile = path.Pop ();
				//Debug.Log ("moving to " + nextTile.ToString ());
				startPos = gameObject.transform.position;
				//Debug.Log ("StartPos: " + startPos.ToString ());
				ScenarioMgr.Direction nextFacing = facing;
				//check validity of tile
				if (!scenarioInfo.IsRoadTile (nextTile))
				{
					havePath = false;
					yield return new WaitForSeconds (1);
				} else
				{
					// compute direction
					if (curTile.xCoord > nextTile.xCoord)
					{
						//Debug.Log ("headed left");
						nextFacing = ScenarioMgr.Direction.Left;
					} else if (curTile.xCoord < nextTile.xCoord)
					{
						//Debug.Log ("headed right");
						nextFacing = ScenarioMgr.Direction.Right;
					} else if (curTile.yCoord > nextTile.yCoord)
					{
						//Debug.Log ("headed up");
						nextFacing = ScenarioMgr.Direction.Up;
					} else if (curTile.yCoord < nextTile.yCoord)
					{
						//Debug.Log ("headed down");
						nextFacing = ScenarioMgr.Direction.Down;
					} else
					{
						stillTraveling = false;
					}
					if (stillTraveling)
					{
						// compute ending position
						switch (nextFacing)
						{
						case ScenarioMgr.Direction.Left:
							endPos = startPos + new Vector3 (-1, 0, 0);
							endAngle = Quaternion.Euler (0, 180, 0);
							break;
						case ScenarioMgr.Direction.Up:
							endPos = startPos + new Vector3 (0, 0, -1);
							endAngle = Quaternion.Euler (0, 90, 0);
							break;
						case ScenarioMgr.Direction.Down:
							endPos = startPos + new Vector3 (0, 0, 1);
							endAngle = Quaternion.Euler (0, 270, 0);
							break;
						case ScenarioMgr.Direction.Right:
							endPos = startPos + new Vector3 (1, 0, 0);
							endAngle = Quaternion.Euler (0, 0, 0);
							break;
						}
						//Debug.Log ("endPos: " + endPos.ToString ());
						float elapsedTime = 0;
						if (facing != nextFacing)
						{
							// first handle the rotation
							while (elapsedTime < this.turnTime)
							{
								this.gameObject.transform.rotation = Quaternion.Slerp (startAngle, endAngle, (elapsedTime / this.turnTime));
								elapsedTime += Time.deltaTime;
								yield return 0;
							}
						}
						// move to tile
						float moveTime = this.tileMoveTime;
						if (!scenarioInfo.IsRoadTile (curTile))
							moveTime = moveTime * 2;
						elapsedTime = 0;
						while (elapsedTime < moveTime)
						{
							this.gameObject.transform.position = Vector3.Lerp (startPos, endPos, (elapsedTime / moveTime));
							elapsedTime += Time.deltaTime;
							yield return 0;
						}
						// fix data for next tile
						this.facing = nextFacing;
						this.curTile = nextTile;
						startPos = endPos;
						startAngle = endAngle;


					}
				}
			}
			yield return 0;
		}
	}
}
