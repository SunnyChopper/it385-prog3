using UnityEngine;
using System.Collections;
//using UnityStandardAssets.Characters.ThirdPerson;

public class WellWalkerManager : MonoBehaviour
{

	[SerializeField] int maxWellWalkerDistance;
	[SerializeField] float tileMoveTime;
	[SerializeField] float turnTime;
	private ScenarioData scenarioInfo;
	private ScenarioMgr.Direction facing;
	private int tilesToGo;
	private bool turningLeft;
	private IntPoint2D curTile;
	


	public void SetUp (ScenarioData scenario, IntPoint2D startTile, ScenarioMgr.Direction startingDir, bool leftTurns)
	{
		//Debug.Log ("in well walker set up");
		scenarioInfo = scenario;
		facing = startingDir;
		tilesToGo = maxWellWalkerDistance;
		turningLeft = leftTurns;
		curTile = startTile;
		StartCoroutine ("Walk");
	}

	IEnumerator Walk ()
	{
		Vector3 startPos = gameObject.transform.position, endPos = startPos;
		Quaternion startAngle = gameObject.transform.rotation;
		Quaternion endAngle = startAngle;
		bool rotating = false;
		ScenarioMgr.Direction nextFacing = facing;
		IntPoint2D nextTile = curTile;
		while (tilesToGo > 0) {
			//Debug.Log("doing distribution");
			// do water delivery
			scenarioInfo.DistributeWater (new IntPoint2D (curTile.xCoord, curTile.yCoord - 1));
			scenarioInfo.DistributeWater (new IntPoint2D (curTile.xCoord, curTile.yCoord + 1));
			scenarioInfo.DistributeWater (new IntPoint2D (curTile.xCoord - 1, curTile.yCoord));
			scenarioInfo.DistributeWater (new IntPoint2D (curTile.xCoord + 1, curTile.yCoord));
			// check for road tile ahead and turn appropriately
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
			startPos = transform.position;
			startAngle = gameObject.transform.rotation;
			bool goingNowhere = false;
			if (scenarioInfo.IsRoadTile (tileAhead)) {
				// no rotation required, just moving straight ahead
				rotating = false;
				nextFacing = facing;
				nextTile = tileAhead;
			} else {
				rotating = true;
				// we can't just go straight ahead, so we're doing some sort of rotation
				if (turningLeft) {
					// check left first
					if (scenarioInfo.IsRoadTile (tileLeft)) {
						// turning left
						nextTile = tileLeft;
						nextFacing = ScenarioMgr.GetLeftTurn (facing);
					} else if (scenarioInfo.IsRoadTile (tileRight)) {
						// turn right because we can't turn left
						nextTile = tileRight;
						nextFacing = ScenarioMgr.GetRightTurn (facing);
					} else if (scenarioInfo.IsRoadTile (tileBehind)) {	
						nextTile = tileBehind;
						nextFacing = ScenarioMgr.GetReverseDirection (facing);
					} else {
						goingNowhere = true;
						rotating = false;

					}

				} else {
					// check right first
					if (scenarioInfo.IsRoadTile (tileRight)) {
						nextTile = tileRight;
						nextFacing = ScenarioMgr.GetRightTurn (facing);
					} else if (scenarioInfo.IsRoadTile (tileLeft)) {
						// turning left because we can't turn right
						nextTile = tileLeft;
						nextFacing = ScenarioMgr.GetLeftTurn (facing);
					} else if (scenarioInfo.IsRoadTile (tileBehind)) {	
						nextTile = tileBehind;
						nextFacing = ScenarioMgr.GetReverseDirection (facing);
					} else {
						goingNowhere = true;
						rotating = false;
					}
				}

			}


			if (goingNowhere) {
				//Debug.Log("no road to move to");
				// no rotation, no movement, no nothing -- just wait things out for a second
				yield return new WaitForSeconds (tileMoveTime);
			} else {
				// compute ending position
				switch (nextFacing) {
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
				elapsedTime = 0;
				while (elapsedTime < this.tileMoveTime) {
					this.gameObject.transform.position = Vector3.Lerp (startPos, endPos, (elapsedTime / this.tileMoveTime));
					elapsedTime += Time.deltaTime;
					yield return 0;
				}
				// fix data for next tile
				this.facing = nextFacing;
				this.curTile = nextTile;
				startPos = endPos;
				startAngle = endAngle;


			}
		
			tilesToGo--;
		}
        Destroy(gameObject);
	}

}