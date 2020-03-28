using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CitizenManager : MonoBehaviour
{

	[SerializeField] private float tileMoveTime;
	[SerializeField] float turnTime;
	[SerializeField] GameObject basket;
	private ScenarioData scenarioInfo;
	private ScenarioMgr.Direction facing;
	private IntPoint2D curTile;
	private bool wandering;
	private bool startPath=false;
	private IntPoint2D destTile;
	private HouseManager myHouse;
	private bool goingToHouse;
	private bool movingIn;
	private bool goingToStore = false;
	private StoreManager destStore = null;
	private int goods = 0;
	private Stack<IntPoint2D> path = null;
	private bool havePath;
	private Vector3 citizenTileOffset = new Vector3(0.5f, 0.2f, 0.5f);
   // private int searchLimit = 100;

	public void SetUp (ScenarioData scenario, IntPoint2D startTile, ScenarioMgr.Direction startingDir, bool wandering, 
		IntPoint2D dest,HouseManager house,bool movingIn) {
		Debug.Log ("in citizen set up");
		scenarioInfo = scenario;
		facing = startingDir;
		curTile = startTile;
		this.wandering = wandering;
		this.destTile = dest;
		Debug.Log("start tile: " + startTile.ToString());
		Debug.Log("dest tile: " + destTile.ToString());
		this.myHouse = house;
		this.movingIn = movingIn;
		havePath = false;
		startPath = false;
		if (!wandering) {
			Debug.Log("Starting TakePath Coroutine from SetUp");
			StartCoroutine(TakePath());
		} else {
			StartCoroutine (Wander());
		}
	}

	public void SetUp(ScenarioData scenario, IntPoint2D startTile, ScenarioMgr.Direction startingDir, Stack<IntPoint2D> myPath,
		StoreManager store, HouseManager house) {
		Debug.Log("heading to store");
		scenarioInfo = scenario;
		facing = startingDir;
		curTile = startTile;
		this.wandering = false;
		this.destStore = store;
		path = myPath;
		myHouse = house;
		movingIn = false;
		goingToHouse = false;
		goingToStore = true;
		startPath = false;
		havePath = true;
		StartCoroutine(TakePath());
	}

	private void Awake()
	{
		startPath = false;
	}



    // Use this for initialization
	void Start ()
	{
		startPath = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
//		if (startPath) {
//            Debug.Log("Starting TakePath coroutine from Update");
//			startPath = false;
//			StartCoroutine (TakePath());
//		}
	}

	public void SendToDest (IntPoint2D dest,HouseManager house,bool movingIn)
	{
		destTile = dest;
		myHouse = house;
		goingToHouse = true;
		wandering = false;
		this.movingIn = movingIn;
		goingToStore = false;
		havePath = false;
        //startPath = true;
		path = null;
		StartCoroutine(TakePath());

	}

	private void DoStoreStuff()
	{
		Debug.Log("collecting goods from store");
		goods = destStore.ProvideGoods(50);
		goingToStore = false;
		goingToHouse = true;
		basket.SetActive(true);
		SendToDest(myHouse.GetHouseLocation(), myHouse, false);
	}


	IEnumerator Wander ()
	{
		Vector3 startPos = gameObject.transform.position, endPos = startPos;
		Quaternion startAngle = gameObject.transform.rotation;
		Quaternion endAngle = startAngle;
		bool rotating = false;
		ScenarioMgr.Direction nextFacing = facing;
		IntPoint2D nextTile = curTile;
		while (wandering) {
			bool goingNowhere = false;
			int x = curTile.xCoord, y = curTile.yCoord;
			IntPoint2D tileAhead = curTile, tileLeft = curTile, tileRight = curTile, tileBehind = curTile;
			switch (facing) {
				case ScenarioMgr.Direction.Left:
				tileAhead = new IntPoint2D (x - 1, y);
				tileRight = new IntPoint2D (x, y - 1);
				tileLeft = new IntPoint2D (x, y + 1);
				tileBehind = new IntPoint2D (x + 1, y);
				break;
				case ScenarioMgr.Direction.Up:
				tileLeft = new IntPoint2D (x - 1, y);
				tileAhead = new IntPoint2D (x, y - 1);
				tileBehind = new IntPoint2D (x, y + 1);
				tileRight = new IntPoint2D (x + 1, y);
				break;
				case ScenarioMgr.Direction.Right:
				tileBehind = new IntPoint2D (x - 1, y);
				tileRight = new IntPoint2D (x, y + 1);
				tileLeft = new IntPoint2D (x, y - 1);
				tileAhead = new IntPoint2D (x + 1, y);
				break;
				case ScenarioMgr.Direction.Down:
				tileRight = new IntPoint2D (x - 1, y);
				tileBehind = new IntPoint2D (x, y - 1);
				tileAhead = new IntPoint2D (x, y + 1);
				tileLeft = new IntPoint2D (x + 1, y);
				break;
			}
			// pick direction
			int p = Random.Range (0, 100);
			if (p < 30) {
				goingNowhere = true;
				rotating = false;
			} else if (p < 60 && scenarioInfo.IsPassableTile (tileAhead)) {
				rotating = false;
				nextFacing = facing;
				nextTile = tileAhead;
			} else if (p < 80 && scenarioInfo.IsPassableTile (tileLeft)) {
				rotating = true;
				nextTile = tileLeft;
				nextFacing = ScenarioMgr.GetLeftTurn (facing);
			} else if (scenarioInfo.IsPassableTile (tileRight)) {
				rotating = true;
				nextTile = tileRight;
				nextFacing = ScenarioMgr.GetRightTurn (facing);
			} else if (scenarioInfo.IsPassableTile (tileBehind)) {
				rotating = true;
				nextTile = tileBehind;
				nextFacing = ScenarioMgr.GetReverseDirection (facing);
			} else {
				goingNowhere = true;
				rotating = false;
			}

			startPos = transform.position;
			startAngle = gameObject.transform.rotation;

			if (goingNowhere) {
				//Debug.Log("no road to move to");
				// no rotation, no movement, no nothing -- just wait things out for a second
				yield return new WaitForSeconds (tileMoveTime);
			} else {
				endPos = scenarioInfo.ComputeTopLeftPointOfTile(nextTile) + citizenTileOffset;
				// compute ending position
				switch (nextFacing) {
					case ScenarioMgr.Direction.Left:
					endAngle = Quaternion.Euler (0, 180, 0);
					break;
					case ScenarioMgr.Direction.Up:
					endAngle = Quaternion.Euler (0, 90, 0);
					break;
					case ScenarioMgr.Direction.Down:
					endAngle = Quaternion.Euler (0, 270, 0);
					break;
					case ScenarioMgr.Direction.Right:
					endAngle = Quaternion.Euler (0, 0, 0);
					break;
				}
				float elapsedTime = 0;
				if (rotating) {
					// first handle the rotation
					while (elapsedTime < this.turnTime) {
						this.gameObject.transform.rotation = Quaternion.Slerp (startAngle, endAngle, (elapsedTime / this.turnTime));
						elapsedTime += Time.deltaTime;
						yield return 0;
					}
					
				}
				//Debug.Log("starting movement from " + startPos.ToString() + " to " + endPos.ToString());
				//Debug.Log("from tile " + curTile.ToString() + " to " + nextTile.ToString());
				// move
				float moveTime = this.tileMoveTime;
				if (!scenarioInfo.IsRoadTile (curTile))
				moveTime = moveTime * 2;
				elapsedTime = 0;
				while (elapsedTime <= moveTime) {
					this.gameObject.transform.position = Vector3.Lerp (startPos, endPos, (elapsedTime / moveTime));
					elapsedTime += Time.deltaTime;
					yield return 0;
				}
				// fix data for next tile
				this.facing = nextFacing;
				this.curTile = nextTile;
				startPos = this.gameObject.transform.position;
				startAngle = endAngle;
			}
		}
        //Debug.Log("Starting takepath coroutine from Wander");
		//startPath = true;
	}

	private bool GetPathToStore()
	{
		List<IntPoint2D> startTiles = new List<IntPoint2D>();
		startTiles.Add(curTile);
		List<IntPoint2D> endTiles = scenarioInfo.GetAdjacentRoadTiles(destStore.GetLoc());
		Stack<IntPoint2D> newPath = scenarioInfo.ShortestPath(startTiles, endTiles);
		if (newPath.Count > 0)
		{
			newPath.Pop();
			path = newPath;
			return true;
		}
		else
		{
			return GetPathToAllStores();
		}

	}

	private bool GetPathToAllStores()
	{
		GameObject[] stores = GameObject.FindGameObjectsWithTag("Store");
		if (stores.Length > 0)
		{
			List<IntPoint2D> startTiles = new List<IntPoint2D>();
			startTiles.Add(curTile);
			List<IntPoint2D> endTiles;
			Stack<IntPoint2D> bestPath = new Stack<IntPoint2D>();
			StoreManager bestmgr = null;
			foreach (GameObject store in stores)
			{
                //  get the store manager
				StoreManager mgr = store.GetComponent("StoreManager") as StoreManager;
                // see if there's available capacity
				if (mgr.GetRoom() > 0)
				{
                    // get adjacent roads
					endTiles = scenarioInfo.GetAdjacentRoadTiles(mgr.GetLoc());
					Stack<IntPoint2D> newPath = scenarioInfo.ShortestPath(startTiles, endTiles);
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
				bestPath.Pop();
				path = bestPath;
			}
			return havePath;
		}
		else
		return false;
	}

	IEnumerator TakePath ()
	{
		bool stillTraveling = true;
		
		Vector3 startPos = gameObject.transform.position, endPos = startPos;
		Quaternion startAngle = gameObject.transform.rotation;
		Quaternion endAngle = startAngle;
		while (stillTraveling) {
			if (goingToHouse && curTile.Equals (destTile) || goingToStore && havePath && path.Count == 0) 
			stillTraveling = false;
			else {
				if (!havePath) {
                    // compute path from curTile to dest
					if (goingToHouse)
					{
						path = scenarioInfo.ComputePath(curTile, this.destTile, true);
						havePath = true;
					}
					else if (goingToStore)
					{
						havePath = GetPathToAllStores();
					}
					else stillTraveling = false;
					yield return 0;
				}
				if (path.Count == 0) // got no path: wait for the world to change
				yield return new WaitForSeconds (1);
				else {
					IntPoint2D nextTile = path.Pop ();
					startPos = gameObject.transform.position;
					ScenarioMgr.Direction nextFacing = facing;
					//check validity of tile
					if (!scenarioInfo.IsPassableTile (nextTile)) {
						if (goingToHouse && nextTile.Equals(destTile) || goingToStore && havePath && path.Count == 0)
						{
							stillTraveling = false;
						}
						else if (goingToStore)
						{
							havePath = GetPathToStore();
						}
						else { 
							havePath = false;
							yield return new WaitForSeconds (1);
						}
					} else {
						// compute direction
						if (curTile.xCoord > nextTile.xCoord) {
							nextFacing = ScenarioMgr.Direction.Left;
						} else if (curTile.xCoord < nextTile.xCoord) {
							nextFacing = ScenarioMgr.Direction.Right;
						} else if (curTile.yCoord > nextTile.yCoord) {
							nextFacing = ScenarioMgr.Direction.Up;
						} else if (curTile.yCoord < nextTile.yCoord) {
							nextFacing = ScenarioMgr.Direction.Down;
						} else {
							stillTraveling = false;
						}
						if (stillTraveling) {
							endPos = scenarioInfo.ComputeTopLeftPointOfTile(nextTile) + citizenTileOffset;
							// compute ending position
							switch (nextFacing) {
								case ScenarioMgr.Direction.Left:
								endAngle = Quaternion.Euler (0, 180, 0);
								break;
								case ScenarioMgr.Direction.Up:
								endAngle = Quaternion.Euler (0, 90, 0);
								break;
								case ScenarioMgr.Direction.Down:
								endAngle = Quaternion.Euler (0, 270, 0);
								break;
								case ScenarioMgr.Direction.Right:
								endAngle = Quaternion.Euler (0, 0, 0);
								break;
							}
							float elapsedTime = 0;
							if (facing != nextFacing) {
								// first handle the rotation
								while (elapsedTime < this.turnTime) {
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
							while (elapsedTime < moveTime) {
								this.gameObject.transform.position = Vector3.Lerp (startPos, endPos, (elapsedTime / moveTime));
								elapsedTime += Time.deltaTime;
								yield return 0;
							}
							// fix data for next tile
							this.facing = nextFacing;
							this.curTile = nextTile;
							startPos = gameObject.transform.position;
							startAngle = endAngle;
							
						}
					}
				}
			}
			yield return 0;
		}
		Debug.Log("at destination");
		if (movingIn && myHouse!=null)
		{
			myHouse.RecordArrival();
			} else if (goingToHouse && myHouse != null&& goods > 0)
			{
				myHouse.ReceiveResource2(goods);
			}
			if (goingToStore && destStore != null)
			{
				DoStoreStuff();
			}
			else
			{
            // done with this citizen
				basket.SetActive(false);
            Destroy(gameObject); // destroy me
        }
    }

}
