using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Priority_Queue;

public class ScenarioData
{
	private GridTileInfo[,] tileArray;

	private int gridWidth;
	private int gridHeight;

	public ScenarioData (int xTiles, int zTiles)
	{
		gridWidth = xTiles;
		gridHeight = zTiles;
		tileArray = new GridTileInfo[xTiles, zTiles];
		for (int i = 0; i < xTiles; i++) {
			for (int j = 0; j < zTiles; j++) {
				tileArray [i, j] = new GridTileInfo ();
			}
		}
	}

	public bool MakeRoad (IntPoint2D tileIndex, GameObject tile)
	{
		bool canMake = IsValidTile (tileIndex) && tileArray [tileIndex.xCoord, tileIndex.yCoord].IsClear ();

		if (canMake) {
			tileArray [tileIndex.xCoord, tileIndex.yCoord].MakeRoad (tile, tileIndex);
		}
		return canMake;
	}

	public bool MakeHouse (IntPoint2D houseTile, GameObject house)
	{
		bool canMake = IsValidTile (houseTile) && tileArray [houseTile.xCoord, houseTile.yCoord].IsClear ();
		
		if (canMake) {
			tileArray [houseTile.xCoord, houseTile.yCoord].MakeHouse (house, houseTile);
		}
		return canMake;
	}

	public void MakeOther (IntPoint2D topLeft, GameObject building, int size = 1)
	{
		int sides = ConvertSizeToSides (size);
		for (int i = topLeft.xCoord; i < topLeft.xCoord + sides; i++) {
			for (int j = topLeft.yCoord; j < topLeft.yCoord + sides; j++) {
				tileArray [i, j].MakeOther (building, sides, topLeft);
			}
		}
	}

	private int ConvertSizeToSides (int size)
	{
		int sides = 1;
		if (size == 4)
			sides = 2;
		else if (size == 9)
			sides = 3;
		else if (size == 16)
			sides = 4;
		return sides;
	}

    public Vector3 ComputeTopLeftPointOfTile(IntPoint2D tileIndex)
    {
        //Debug.Log ("In ComputeTopLeftPointOfTile");
        Vector3 point = new Vector3(tileIndex.xCoord - gridWidth / 2, 0, tileIndex.yCoord - gridHeight / 2);
        //Debug.Log(tileIndex);
        //Debug.Log (point);
        return point;
    }


    public bool CanBuildHere (IntPoint2D tileIndex)
	{
		return IsValidTile (tileIndex) && tileArray [tileIndex.xCoord, tileIndex.yCoord].IsClear ();
	}

	public bool CanBuildHere (IntPoint2D tileIndex, int numTiles)
	{
		bool goodToBuild = CanBuildHere (tileIndex); // check the base tile
		if (numTiles > 1) { // check tiles 2, 3, and 4
			if (goodToBuild && CanBuildHere (new IntPoint2D (tileIndex.xCoord, tileIndex.yCoord + 1))) {
				if (goodToBuild && CanBuildHere (new IntPoint2D (tileIndex.xCoord + 1, tileIndex.yCoord))) {
					if (!CanBuildHere (new IntPoint2D (tileIndex.xCoord + 1, tileIndex.yCoord + 1)))
						goodToBuild = false;
				} else {
					goodToBuild = false;
				}
			} else {
				goodToBuild = false;
			}
		}
		if (goodToBuild && numTiles > 4) { // check tiles 5-9
			for (int i = tileIndex.xCoord - 1; goodToBuild && i <= tileIndex.xCoord + 1; i++) {
				if (!CanBuildHere (new IntPoint2D (i, tileIndex.yCoord - 1)))
					goodToBuild = false;
			}
			for (int j = tileIndex.yCoord; goodToBuild && j <= tileIndex.yCoord + 1; j++) {
				if (!CanBuildHere (new IntPoint2D (tileIndex.xCoord - 1, j)))
					goodToBuild = false;
			}
		}
		// will need to add logic for larger buildings
		return goodToBuild;
	}

			                      
	public bool IsValidTile (IntPoint2D tileIndex)
	{
		return tileIndex.xCoord >= 0 && tileIndex.xCoord < gridWidth && tileIndex.yCoord >= 0 && tileIndex.yCoord < gridHeight;
	}

	public bool IsRoadTile (IntPoint2D tileIndex)
	{
		return IsValidTile (tileIndex) && tileArray [tileIndex.xCoord, tileIndex.yCoord].IsRoad ();
	}

	public bool IsPassableTile (IntPoint2D tileIndex)
	{
		return IsValidTile (tileIndex) && tileArray [tileIndex.xCoord, tileIndex.yCoord].CanPass ();
	}

	public bool IsHouse (IntPoint2D tileIndex)
	{
		return tileArray [tileIndex.xCoord, tileIndex.yCoord].IsHouse ();
	}

	public IntPoint2D CloseClearTile (IntPoint2D tileIndex)
	{
		IntPoint2D clearTile;
		int x = tileIndex.xCoord;
		int y = tileIndex.yCoord;
		for (int i = 1; i <= 10; i++) {
			clearTile = new IntPoint2D (x + i, y);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x, y + i);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x - i, y);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x, y - i);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x + i, y + i);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x - i, y - i);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x + i, y - i);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
			clearTile = new IntPoint2D (x - i, y + i);
			if (IsValidTile (clearTile) && IsPassableTile (clearTile)) {
				return clearTile;
			}
		}
		return new IntPoint2D (0, 0);
	}

	public Stack<IntPoint2D> GetRoadPath (IntPoint2D startLoc, IntPoint2D endLoc)
	{
		List<IntPoint2D> startList = GetAdjacentRoadTiles (startLoc);
		List<IntPoint2D> endList = GetAdjacentRoadTiles (endLoc);
		return ShortestPath (startList, endList, true);
	}

	public List<IntPoint2D> GetAdjacentRoadTiles (IntPoint2D startTile)
	{
		List<IntPoint2D> tileList = new List<IntPoint2D> ();
		GridTileInfo tileInfo = tileArray [startTile.xCoord, startTile.yCoord];
		if (tileInfo.IsRoad ()) {
			tileList.Add (startTile);
		} else {
			IntPoint2D topLeft = startTile;
			int sideLen = 1;
			if (!tileInfo.IsClear ()) {
				topLeft = tileInfo.GetTopLeftOfBuilding ();
				sideLen = tileInfo.GetSides ();
			}
			// now we need to check each adjacent tile
			for (int step = 0; step < sideLen; step++) {
				IntPoint2D tileLoc = new IntPoint2D (topLeft.xCoord + step, topLeft.yCoord - 1);
				if (IsRoadTile (tileLoc)) {
					tileList.Add (tileLoc);
				}
				tileLoc = new IntPoint2D (topLeft.xCoord + step, topLeft.yCoord + sideLen);
				if (IsRoadTile (tileLoc)) {
					tileList.Add (tileLoc);
				}
				tileLoc = new IntPoint2D (topLeft.xCoord - 1, topLeft.yCoord + step);
				if (IsRoadTile (tileLoc)) {
					tileList.Add (tileLoc);
				}
				tileLoc = new IntPoint2D (topLeft.xCoord + sideLen, topLeft.yCoord + step);
				if (IsRoadTile (tileLoc)) {
					tileList.Add (tileLoc);
				}	
			}
		}
		return tileList;	
	}

	public void DistributeWater (IntPoint2D tileIndex)
	{
		if (IsValidTile (tileIndex)) {
			tileArray [tileIndex.xCoord, tileIndex.yCoord].DistributeWater ();
		}
	}

	public Stack<IntPoint2D> ComputePath (IntPoint2D start, IntPoint2D end, bool openTravel)
	{
		//Debug.Log ("Computing path from "+start.ToString()+ " to " + end.ToString());
		HashSet<IntPoint2D> closed = new HashSet<IntPoint2D> ();
		closed.Add (start);
		int queueSize = (this.gridHeight * this.gridWidth) * 16;
		HeapPriorityQueue<SearchNode> searchList = new HeapPriorityQueue<SearchNode> (queueSize);
		Stack<IntPoint2D> path = new Stack<IntPoint2D> ();
		SearchNode root = new SearchNode (null, start, 0, Math.Abs (start.xCoord - end.xCoord) + Math.Abs (start.yCoord - end.yCoord));
		searchList.Enqueue (root, root.Priority);
		SearchNode curNode = null;
		bool found = false;
		List<SearchNode> children;
		while (!found && searchList.Count > 0) {
			curNode = searchList.Dequeue ();
			//Debug.Log("checking node: "+curNode.data.ToString());
			if (curNode.AtGoal (end))
				found = true;
			else {
				children = curNode.GetChildren (end, this, openTravel);
				foreach (SearchNode child in children) {
					if (searchList.Count == queueSize)
						Debug.Log ("Priority queue size: " + queueSize.ToString () + " exceeded");
					//Debug.Log("enqueueing node: "+child.data.ToString());
					//Debug.Log ("  Priority: "+child.Priority.ToString());
					if (!closed.Contains (child.data)) {
						searchList.Enqueue (child, child.cost + child.heuristic);
						closed.Add (child.data);
					}
				}
			}
		}
		if (found) {
			//Debug.Log ("pushing path");
			while (curNode != null) {
				path.Push (curNode.data);
//				Debug.Log(curNode.data.ToString());
				curNode = curNode.parent;
			}
			path.Pop ();
		} else {
			//Debug.Log ("no path found");
		}
		return path;
	}

	public GameObject GetBuilding (IntPoint2D tile)
	{
		return tileArray [tile.xCoord, tile.yCoord].GetBuilding ();
	}

	public void DeleteBuilding (IntPoint2D tileLoc)
	{
		
		IntPoint2D topLeft = tileArray [tileLoc.xCoord, tileLoc.yCoord].GetTopLeftOfBuilding ();
		int sides = tileArray [tileLoc.xCoord, tileLoc.yCoord].GetSides ();
		int x = topLeft.xCoord;
		int y = topLeft.yCoord;
		for (int i = x; i < x + sides; i++) {
			for (int j = y; j < y + sides; j++) {
				Debug.Log ("deleting stuff at " + i + j);
				tileArray [i, j].DeleteTileContents ();
			}
		}

	}

	public Stack<IntPoint2D> ShortestPath (List<IntPoint2D> startPoints, List<IntPoint2D> endPoints, bool roadsOnly = true)
	{
		Stack<IntPoint2D> curSPath = new Stack<IntPoint2D> ();
		bool pathFound = false;
		foreach (IntPoint2D tileLoc in startPoints) {
			if (curSPath.Contains (tileLoc)) {  // consider reversing contains for efficiency?
				while (!tileLoc.Equals (curSPath.Peek ())) {
					curSPath.Pop ();
				}
			} else {
				foreach (IntPoint2D endLoc in endPoints) {
					Stack<IntPoint2D> tempPath = ComputePath (tileLoc, endLoc, !roadsOnly);  
					// seriously consider whether there are some efficiency gains to be made

					if (tempPath.Count > 0) {
						tempPath.Push (tileLoc);
						if (!pathFound || tempPath.Count < curSPath.Count) {
							curSPath = tempPath;
							pathFound = true;
						}

					}
				}


			}

		}
		return curSPath;

	}

}

