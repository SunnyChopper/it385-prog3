using UnityEngine;
using System.Collections;

public class WellManager : MonoBehaviour {
	private const int MAX_TRAVEL_DISTANCE = 50;
	private const int kMaxWorkers = 3;

	private ScenarioData scenarioInfo;
	//private ScenarioMgr scenario;
	private WalkerPool walkerPool;
	private GameObject myWalker;
	private PopulationManager popMgr;

	private IntPoint2D buildingTile;

	private int numWorkers;


	public void SetUp(GameObject ground, ScenarioData scenInfo, IntPoint2D loc)
	{
//		scenario = ground.GetComponent("ScenarioMgr") as ScenarioMgr;
		walkerPool = ground.GetComponent("WalkerPool") as WalkerPool;
		popMgr = ground.GetComponent("PopulationManager") as PopulationManager;
		scenarioInfo = scenInfo;
		buildingTile = loc;
		numWorkers = 0;
		StartCoroutine("GetWorkers");
		StartCoroutine("Distribute");
	}

	IEnumerator GetWorkers()
	{
		int numNewWorkers = 0;
		int unfilledWorkers = 0;
		while (true)
		{
			//Debug.Log("requesting workers");
			numNewWorkers = popMgr.RequestWorkers(kMaxWorkers-numWorkers,numWorkers,unfilledWorkers);
			numWorkers = numWorkers+numNewWorkers;
			unfilledWorkers = kMaxWorkers-numWorkers;
			yield return new WaitForSeconds(1);
		}
	}

	//this is a well, so it always has plenty of its resource -- we don't have to worry about gathering, just distributing
	IEnumerator Distribute()
	{
		bool hadRoadLastTime = false;
		int roadTileIndex = -1;
		IntPoint2D [] neighborTiles = new IntPoint2D[4];
		bool goingLeft = true;
		//The building isn't going to move, so compute the 4 neighboring tiles up front left=0, up=1, right=2, down=3;
		neighborTiles[0] = new IntPoint2D(buildingTile.xCoord-1,buildingTile.yCoord);
		neighborTiles[1] = new IntPoint2D(buildingTile.xCoord,buildingTile.yCoord-1);
		neighborTiles[2] = new IntPoint2D(buildingTile.xCoord+1,buildingTile.yCoord);
		neighborTiles[3] = new IntPoint2D(buildingTile.xCoord,buildingTile.yCoord+1);

		yield return new WaitForSeconds(1);
		while (true)
		{
			bool haveRoad = false;
			// check for a road
			if (hadRoadLastTime)
			{
				if (this.scenarioInfo.IsRoadTile(neighborTiles[roadTileIndex]))
					haveRoad = true;
			}
			for (int i =0; !haveRoad&&i<4;i++)
			{
				if (this.scenarioInfo.IsRoadTile(neighborTiles[i]))
				{
					haveRoad=true;
					roadTileIndex = i;
				}
			}
			hadRoadLastTime=haveRoad;
			// if one is found, set up a walker and sleep for 30 secs
			//Debug.Log ("numWorkers at well is " + numWorkers.ToString());
			if (haveRoad&&numWorkers>=kMaxWorkers)
			{
				//Debug.Log("making a walker from a well");
				ScenarioMgr.Direction facing = ScenarioMgr.Direction.Left;
				// figure out which direction the walker should be facing;
				if (goingLeft)
				{
					if (roadTileIndex==0) {
						facing=ScenarioMgr.Direction.Down;
					} else if (roadTileIndex==1) {
						facing = ScenarioMgr.Direction.Left;
					} else if (roadTileIndex==2) {
						facing = ScenarioMgr.Direction.Up;
					} else {
						facing = ScenarioMgr.Direction.Right;
					}
				}
				else
				{
					if (roadTileIndex==0) {
						facing=ScenarioMgr.Direction.Up;
					} else if (roadTileIndex==1) {
						facing = ScenarioMgr.Direction.Right;
					} else if (roadTileIndex==2) {
						facing = ScenarioMgr.Direction.Down;
					} else {
						facing = ScenarioMgr.Direction.Left;
					}
				}
				//get the walker created and working
				GameObject walker = walkerPool.GetWellWalker(neighborTiles[roadTileIndex],facing);
				walker.SetActive(true);
				//Debug.Log(walker.transform.position);
				//get the walker's script and get things working
				WellWalkerManager walkerMgr = walker.GetComponent("WellWalkerManager") as WellWalkerManager;
				walkerMgr.SetUp(this.scenarioInfo,neighborTiles[roadTileIndex],facing,goingLeft);
				goingLeft = !goingLeft;
				yield return new WaitForSeconds(30);
			} else
			{
			// if not, sleep for 1 second
				yield return new WaitForSeconds(1);
			}
		}
	}
}
