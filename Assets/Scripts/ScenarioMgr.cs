using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// primary class to manage a scenario
public class ScenarioMgr : MonoBehaviour
{
	public Material greenMat;
	public Material redMat;
	public int initRoadXStart;
	public int initRoadXEnd;
	public int initRoadYStart;
	public int initRoadYEnd;
	public int initRoadVertX;
	public int initRoadHorizY;
	public Texture2D deleteCursor;
	public Texture2D blankCursor;

	private ScenarioData scenarioInfo;
	private int numTilesInZAxis;
	private int numTilesInXAxis;

	private GameObject curObject;
	private int curObjectSizeInTiles;
	private bool curObjectIsGreen;
	private Dictionary<IntPoint2D,GameObject> tempObjects;
	private bool creatingObject;
	private bool deleting;
	private Material savedMat;
	private RoadPool roadPool;
	private HousePool housePool;
	private ObjectManager objMgr;
	private PopulationManager popMgr;
	private UIState tempObjectType;

	public enum UIState
	{
		Pointer = 0,
		Delete = 1,
		Road = 10,
		Housing = 11,
		EliteHousing = 12,
		Well = 20,
		Farm1 = 21,
		Store1 = 22}

	;

	public enum Direction
	{
		Undefined,
		Left,
		Right,
		Up,
		Down}

	;

	
	private UIState uiState;

	private IntPoint2D startTile;

	private Direction firstDir;

	public const int OFFSET_UISTATE_INDEX = 20;

	public int GetUIArrayIndex ()
	{
		return (int)uiState - OFFSET_UISTATE_INDEX;
	}

	public ScenarioData GetInfo ()
	{
		return scenarioInfo;
	}

	// Use this for initialization
	void Start ()
	{
        Debug.Log("in ScenarioMgr Start");
		numTilesInXAxis = Mathf.FloorToInt (10 * gameObject.transform.localScale.x);
		numTilesInZAxis = Mathf.FloorToInt (10 * gameObject.transform.localScale.z);
		scenarioInfo = new ScenarioData (numTilesInXAxis, numTilesInZAxis);
		uiState = UIState.Pointer;
		curObject = null;
		firstDir = Direction.Undefined;
		tempObjects = new Dictionary<IntPoint2D, GameObject> ();
		creatingObject = false;
		deleting = false;
		roadPool = gameObject.GetComponent ("RoadPool") as RoadPool;
		housePool = gameObject.GetComponent ("HousePool") as HousePool;
		popMgr = gameObject.GetComponent ("PopulationManager") as PopulationManager;
		objMgr = gameObject.GetComponent ("ObjectManager") as ObjectManager;
		tempObjectType = UIState.Pointer;     
		StartCoroutine ("SetUpInitRoad");
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 mouseLoc;
		// right mouse button pressed -- clear out any temp objects and revert to pointer state
		if (Input.GetMouseButtonDown (1))
		{
			if (deleting)
			{
				deleting = false;
				curObject.GetComponent<MeshRenderer> ().material = savedMat;
				curObject = null;
			} else
			{
				this.CleanUpTempObjects ();
				if (curObject != null)
				{
					Destroy (curObject);
					curObject = null;
				}
			}
			Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);
			uiState = UIState.Pointer;
		}
		UIState curUIState = uiState;

		if (Input.GetMouseButtonDown (0))
		{
			if (MouseLocationOnGround (out mouseLoc))
			{
				if (curUIState != UIState.Pointer)
				{
					if (curUIState == UIState.Delete)
					{
						// handle a deletion start
						// need to identify object at mouseLoc 

						if (MouseLocationOnGround (out mouseLoc))
						{
							
							StartDelete (mouseLoc);
						}
					} else
					{
						//handle a creation start
						StartObjectCreation (mouseLoc);
					}
				}
			}
		} else if (Input.GetMouseButton (0))
		{
			// deal with drag state if in a relevant mode
			if (deleting)
			{
				// handle deletion dragging
				// actually not going to change anything in the dragging situation right now
			} else if (creatingObject)
			{
				// handle object creation dragging
				if (MouseLocationOnGround (out mouseLoc))
				{
					DragObject (mouseLoc);
				}
			}

		} else if (Input.GetMouseButtonUp (0))
		{
			if (deleting)
			{
				// complete deletion
				bool onGround = MouseLocationOnGround (out mouseLoc);
				CompleteDelete (onGround, mouseLoc);
				deleting = false;
			} else if (creatingObject)
			{
				// handle actual object creation
				if (MouseLocationOnGround (out mouseLoc))
				{
					MakeObject (mouseLoc);
				}
				CleanUpTempObjects ();
				creatingObject = false;
			}

		} else
		{
			// move curObject if there is one
			if (curObject != null)
			{ // no point if there's nothing to move
				if (MouseLocationOnGround (out mouseLoc))
				{
					// actually over the ground, need to handle moving the object
					AdjustCurObjectLoc (mouseLoc);
				} else
				{
					if (curObject.activeSelf)
					{
						curObject.SetActive (false);
					}
				}

			}
		}



	}

	/* old Road creation code
	 * 			Vector3 clickLoc;
			if (ClickLocationForced (out clickLoc)) {
				IntPoint2D tileIndex = ConvertPointToTileIndex (clickLoc);
				CreateRoad (tileIndex);
			}
*/


	bool MouseLocationOnGround (out Vector3 point)
	{
		Collider myCollider = GetComponent<Collider> ();
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
		RaycastHit hitInfo = new RaycastHit ();
		if (myCollider.Raycast (ray, out hitInfo, Mathf.Infinity))
		{
			point = hitInfo.point;
			return true;
		} 
		point = Vector3.zero;
		return false;
	}

	/* Turn a point on the ground plane into the tile index in which the point is located.
	 * Note that we know the y coord is 0 because this is a plane. */
	public IntPoint2D ConvertPointToTileIndex (Vector3 point)
	{
		//Debug.Log ("In ConvertPointToTileIndex");
		//Debug.Log(point);
		int x;
		int z;

		// do the computation
		x = (int)Mathf.Floor (point.x + numTilesInXAxis / 2);
		z = (int)Mathf.Floor (point.z + numTilesInZAxis / 2);
		IntPoint2D tileIndex = new IntPoint2D (x, z);
		//Debug.Log (tileIndex);
		return tileIndex;
	}



	void StartObjectCreation (Vector3 mouseLoc)
	{
		curObject.SetActive (false);
		if (uiState == UIState.Road)
		{
			StartRoad (mouseLoc);
		} else if (uiState == UIState.Housing)
		{
			StartHousing (mouseLoc);
		} else if (uiState >= UIState.Well)
		{
			AdjustCurObjectLoc (mouseLoc);
		}
		creatingObject = true;
	}

	void StartRoad (Vector3 mouseLoc)
	{
		this.startTile = this.ConvertPointToTileIndex (mouseLoc);
		this.firstDir = Direction.Undefined;
		GameObject roadTile;
		if (scenarioInfo.CanBuildHere (startTile))
			roadTile = roadPool.GetGreenGhost (startTile);
		else
			roadTile = roadPool.GetRedGhost (startTile);
		tempObjects.Add (startTile, roadTile);
		tempObjectType = UIState.Road;
	}

	void StartHousing (Vector3 mouseLoc)
	{
		this.startTile = this.ConvertPointToTileIndex (mouseLoc);
		GameObject house;
		if (scenarioInfo.CanBuildHere (startTile))
			house = housePool.GetGreenGhost (startTile);
		else
			house = housePool.GetRedGhost (startTile);
		tempObjects.Add (startTile, house);
		tempObjectType = UIState.Housing;
	}

	void StartDelete (Vector3 mouseLoc)
	{
		// we're on the ground, so figure out which tile we're on
		IntPoint2D tile = ConvertPointToTileIndex (mouseLoc);
		// and see if we have an object there
		this.curObject = scenarioInfo.GetBuilding (tile);
		if (curObject != null)
		{
			deleting = true;
			savedMat = curObject.GetComponent<MeshRenderer> ().material;
			curObject.GetComponent<MeshRenderer> ().material = redMat;
		}
	}

	void UpdateDelete (Vector3 mouseLoc)
	{
		// actually not going to do anything right now
	}

	void CompleteDelete (bool goodMouseLoc, Vector3 mouseLoc)
	{
		bool actuallyDeleting = false;
		if (goodMouseLoc)
		{
			// we're on the ground, so figure out which tile we're on
			IntPoint2D tile = ConvertPointToTileIndex (mouseLoc);
			// and see if we have an object there
			GameObject delEndObj = scenarioInfo.GetBuilding (tile);
			if (delEndObj == curObject)
			{ // if the two objects are the same
				if (scenarioInfo.IsHouse (tile))
				{
					popMgr.RemoveHouse (tile);
				}
				actuallyDeleting = true;
				scenarioInfo.DeleteBuilding (tile);
				Destroy (delEndObj);
			}
		}
		if (!actuallyDeleting)
		{
			// fix the mat back
			curObject.GetComponent<MeshRenderer> ().material = savedMat;
		}
		curObject = null;
	}


	void DragObject (Vector3 mouseLoc)
	{
		if (uiState == UIState.Road)
		{
			DragRoad (mouseLoc);
		} else if (uiState == UIState.Housing)
		{
			DragHousing (mouseLoc);
		} else if (uiState >= UIState.Well)
		{
			AdjustCurObjectLoc (mouseLoc);
		}
	}

	void DragRoad (Vector3 mouseLoc)
	{
		IntPoint2D curTile = this.ConvertPointToTileIndex (mouseLoc);
		FixFirstDirection (curTile);
		if (firstDir == Direction.Undefined)
		{
			IntPoint2D[] keys = new IntPoint2D[tempObjects.Count];
			tempObjects.Keys.CopyTo (keys, 0);
			// clean up all but first tile
			foreach (IntPoint2D curKey in keys)
			{
				if (curKey != curTile)
				{
					Destroy (tempObjects [curKey]);
					tempObjects.Remove (curKey);
				}
			}
		} else
		{
			int xLeft, xRight, yUp, yDown, horizY, vertX;
			DetermineEndPoints (curTile, out xLeft, out xRight, out yUp, out yDown, out horizY, out vertX);
			// deal with the interesting problem
			AdjustRoadTiles (xLeft, xRight, yUp, yDown, horizY, vertX);

		}
	}

	void DragHousing (Vector3 mouseLoc)
	{
		IntPoint2D curTile = this.ConvertPointToTileIndex (mouseLoc);
		int xStart, xEnd, yStart, yEnd;
		
		FindTopLeftBottomRight (startTile, curTile, out yStart, out xStart, out yEnd, out xEnd);
		int xLen = xEnd - xStart + 1;
		int yLen = yEnd - yStart + 1;
		bool[,] table = new bool[xLen, yLen];
		// check current tempObjects
		IntPoint2D[] keys = new IntPoint2D[tempObjects.Count];
		tempObjects.Keys.CopyTo (keys, 0);
		// clean up anything that needs to be cleaned up
		foreach (IntPoint2D curKey in keys)
		{
			if (curKey.xCoord >= xStart && curKey.xCoord <= xEnd && curKey.yCoord >= yStart && curKey.yCoord <= yEnd)
			{
				table [curKey.xCoord - xStart, curKey.yCoord - yStart] = true;
			} else
			{
				Destroy (tempObjects [curKey]);
				tempObjects.Remove (curKey);

			}
		}
		// create any needed new ghosts
		for (int i = 0; i < xLen; i++)
		{
			for (int j = 0; j < yLen; j++)
			{
				if (!table [i, j])
				{
					GameObject house;
					IntPoint2D tempTile = new IntPoint2D (i + xStart, j + yStart);
					if (this.scenarioInfo.CanBuildHere (tempTile))
					{
						house = housePool.GetGreenGhost (tempTile);
					} else
					{
						house = housePool.GetRedGhost (tempTile);
					}
					house.SetActive (true);
					tempObjects.Add (tempTile, house);
				}
			}
		}

	}

	private void AdjustRoadTiles (int xLeft, int xRight, int yUp, int yDown, 
	                              int horizY, int vertX)
	{
		int horizLen = xRight - xLeft + 1;
		if (xRight == -1)
			horizLen = 0;
		int vertLen = yDown - yUp + 1;
		if (yUp == -1)
			vertLen = 0;
		//Debug.Log("horizLen " + horizLen.ToString() + "   vertLen " + vertLen.ToString());
		bool[] horizArray = new bool[horizLen];
		bool[] vertArray = new bool[vertLen];

		IntPoint2D[] keys = new IntPoint2D[tempObjects.Count];
		tempObjects.Keys.CopyTo (keys, 0);

		foreach (IntPoint2D curKey in keys)
		{
			// if it is the startTile, we do nothing with it -- that's stuck until this pass is done
			if (curKey != startTile)
			{
				// check the horizontal row
				if (curKey.yCoord == horizY && curKey.xCoord >= xLeft && curKey.xCoord <= xRight)
				{
					// current object is in the horizontal row -- mark in array
					horizArray [curKey.xCoord - xLeft] = true;
				} else if (curKey.xCoord == vertX && curKey.yCoord >= yUp && curKey.yCoord <= yDown)
				{
					// current object is in the vertical column -- mark in array
					vertArray [curKey.yCoord - yUp] = true;
				} else
				{
					// current object is not part of current road, so destroy and remove
					Destroy (tempObjects [curKey]);
					tempObjects.Remove (curKey);
				}
			}
		}
		// having checked all current objects, create any missing road tiles
		// run through horizArray
		GameObject roadTile;
		IntPoint2D tempTile;
		for (int i = 0; i < horizLen; i++)
		{
			if (!horizArray [i])
			{
				// need to make the road tile
				tempTile = new IntPoint2D (i + xLeft, horizY);
				if (this.scenarioInfo.CanBuildHere (tempTile))
				{
					roadTile = roadPool.GetGreenGhost (tempTile);
				} else
				{
					roadTile = roadPool.GetRedGhost (tempTile);
				}
				roadTile.SetActive (true);
				tempObjects.Add (tempTile, roadTile);
			}
		}
		for (int i = 0; i < vertLen; i++)
		{
			if (!vertArray [i])
			{
				// need to make the road tile
				tempTile = new IntPoint2D (vertX, i + yUp);
				if (this.scenarioInfo.CanBuildHere (tempTile))
				{
					roadTile = roadPool.GetGreenGhost (tempTile);
				} else
				{
					roadTile = roadPool.GetRedGhost (tempTile);
				}
				roadTile.SetActive (true);
				tempObjects.Add (tempTile, roadTile);
			}
		}
	}

	private void DetermineEndPoints (IntPoint2D curTile, out int xLeft, out int xRight, out int yUp, out int yDown, 
	                                 out int horizY, out int vertX)
	{
		xLeft = xRight = yUp = yDown = horizY = vertX = 0;
		if (firstDir == Direction.Left || firstDir == Direction.Right)
		{
			horizY = startTile.yCoord;
			vertX = curTile.xCoord;
			if (curTile.yCoord == startTile.yCoord)
			{
				yUp = 0;
				yDown = -1;
			} else if (curTile.yCoord > startTile.yCoord)
			{ // headed down
				yUp = startTile.yCoord + 1;
				yDown = curTile.yCoord;
			} else
			{ // headed up
				yUp = curTile.yCoord;
				yDown = startTile.yCoord - 1;
			}

			if (firstDir == Direction.Left)
			{
				// we're headed left first
				xLeft = curTile.xCoord;
				xRight = startTile.xCoord - 1;
			} else if (firstDir == Direction.Right)
			{
				xLeft = startTile.xCoord + 1;
				xRight = curTile.xCoord;
			}
		} else
		{
			horizY = curTile.yCoord;
			vertX = startTile.xCoord;
			if (curTile.xCoord == startTile.xCoord)
			{
				xLeft = 0;
				xRight = -1;
			} else if (curTile.xCoord > startTile.xCoord)
			{ // headed right
				xLeft = startTile.xCoord + 1;
				xRight = curTile.xCoord;
			} else
			{ // headed left
				xLeft = curTile.xCoord;
				xRight = startTile.xCoord - 1;
			}
				
			if (firstDir == Direction.Up)
			{
				// we're headed up first
				yUp = curTile.yCoord;
				yDown = startTile.yCoord - 1;
			} else if (firstDir == Direction.Down)
			{
				yUp = startTile.yCoord + 1;
				yDown = curTile.yCoord;
			}
		}

	}

    public Vector3 ComputeTopLeftPointOfTile(IntPoint2D tileIndex)
    {
        return scenarioInfo.ComputeTopLeftPointOfTile(tileIndex);
    }


    void FixFirstDirection (IntPoint2D curTile)
	{
		int startX = startTile.xCoord;
		int startY = startTile.yCoord;
		int curX = curTile.xCoord;
		int curY = curTile.yCoord;
		if (firstDir == Direction.Undefined)
		{
			if (curX > startX)
				firstDir = Direction.Right;
			else if (curX < startX)
				firstDir = Direction.Left;
			else if (curY > startY)
				firstDir = Direction.Down;
			else if (curY < startY)
				firstDir = Direction.Up;
		} else if ((firstDir == Direction.Left || firstDir == Direction.Right) && startX == curX)
		{
			if (startY == curY)
				firstDir = Direction.Undefined;
			else if (curY > startY)
				firstDir = Direction.Down;
			else if (curY < startY)
				firstDir = Direction.Up;
		} else if ((firstDir == Direction.Down || firstDir == Direction.Up) && startY == curY)
		{
			if (startX == curX)
				firstDir = Direction.Undefined;
			else if (curX > startX)
				firstDir = Direction.Right;
			else if (curX < startX)
				firstDir = Direction.Left;
		} else if (firstDir == Direction.Left && startX < curX)
		{
			firstDir = Direction.Right;
		} else if (firstDir == Direction.Right && startX > curX)
		{
			firstDir = Direction.Left;
		} else if (firstDir == Direction.Up && startY < curY)
		{
			firstDir = Direction.Down;
		} else if (firstDir == Direction.Down && startY > curY)
		{
			firstDir = Direction.Up;
		}
	}

	private void MakeObject (Vector3 mouseLoc)
	{
		if (uiState == UIState.Road)
		{
			MakeRoad (mouseLoc);
		} else if (uiState == UIState.Housing)
		{
			MakeHousing (mouseLoc);
		} else if (uiState >= UIState.Well)
		{
			MakeSoloObject (mouseLoc);
		}

	}

	private void MakeRoad (Vector3 mouseLoc)
	{
		IntPoint2D curTile = this.ConvertPointToTileIndex (mouseLoc);
		FixFirstDirection (curTile);
		if (firstDir == Direction.Undefined)
		{
			// just make the one road tile at the start location/cur location
			MakeRoadTile (curTile);
		} else
		{
			int xLeft, xRight, yUp, yDown, horizY, vertX;
			DetermineEndPoints (curTile, out xLeft, out xRight, out yUp, out yDown, out horizY, out vertX);
			// deal with the interesting problem
			PlaceRoadTiles (xLeft, xRight, yUp, yDown, horizY, vertX);
			
		}
	}

	private void FindTopLeftBottomRight (IntPoint2D tile1, IntPoint2D tile2, out int top, out int left, out int bottom, out int right)
	{
		if (tile1.xCoord < tile2.xCoord)
		{
			left = tile1.xCoord;
			right = tile2.xCoord;
		} else
		{
			left = tile2.xCoord;
			right = tile1.xCoord;
		}
		if (tile1.yCoord < tile2.yCoord)
		{
			top = tile1.yCoord;
			bottom = tile2.yCoord;
		} else
		{
			top = tile2.yCoord;
			bottom = tile1.yCoord;
		}
	}

	private void MakeHousing (Vector3 mouseLoc)
	{
		IntPoint2D curTile = this.ConvertPointToTileIndex (mouseLoc);
		int xStart, xEnd, yStart, yEnd;
		 
		FindTopLeftBottomRight (startTile, curTile, out yStart, out xStart, out yEnd, out xEnd);
		for (int i = xStart; i <= xEnd; i++)
		{
			for (int j = yStart; j <= yEnd; j++)
			{
				IntPoint2D tileLoc = new IntPoint2D (i, j);
				if (scenarioInfo.CanBuildHere (tileLoc))
				{
					this.MakeHouse (tileLoc);
				}
			}
		}
	}

	private void MakeHouse (IntPoint2D tileLoc)
	{
		GameObject house = housePool.GetActualHouse (tileLoc);
		HouseData data = new HouseData (tileLoc);
		this.scenarioInfo.MakeHouse (tileLoc, house);
		HouseManager houseMgr = house.GetComponent<HouseManager> () as HouseManager;
		houseMgr.AssignHouseData (data);
		houseMgr.StartUpdates ();
		popMgr.AddHouse (tileLoc, houseMgr);
	}

	private void MakeSoloObject (Vector3 mouseLoc)
	{
		IntPoint2D curTile = this.ConvertPointToTileIndex (mouseLoc);
		bool goodLocation = scenarioInfo.CanBuildHere (curTile, curObjectSizeInTiles);
		if (goodLocation)
		{
			GameObject obj = objMgr.GetActualObject (this.GetUIArrayIndex (), curTile);
			IntPoint2D topLeft = curTile;
			if (curObjectSizeInTiles > 4)
				topLeft = new IntPoint2D (curTile.xCoord - 1, curTile.yCoord - 1);
			scenarioInfo.MakeOther (topLeft, obj, curObjectSizeInTiles); // need to fix this later for objects bigger than 4 tiles
			SetUpBuilding (obj, curTile);
		}
	}

	private void PlaceRoadTiles (int xLeft, int xRight, int yUp, int yDown, 
	                             int horizY, int vertX)
	{
		// Make the startTile
		MakeRoadTile (startTile);
		// Make the horizontal row, if any
		for (int i = xLeft; i <= xRight; i++)
		{
			MakeRoadTile (new IntPoint2D (i, horizY));
		}
		// Make the vertical column, if any
		for (int i = yUp; i <= yDown; i++)
		{
			MakeRoadTile (new IntPoint2D (vertX, i));
		}

	}

	private void MakeRoadTile (IntPoint2D tileLoc)
	{
		if (scenarioInfo.CanBuildHere (tileLoc))
		{
			GameObject tile = roadPool.GetActualTile (tileLoc);
			scenarioInfo.MakeRoad (tileLoc, tile);
		}
	}


	// destroy and clear out all temporary objects
	void CleanUpTempObjects ()
	{
		Dictionary<IntPoint2D, GameObject>.ValueCollection valueColl =
			tempObjects.Values;
		foreach (GameObject temp in valueColl)
		{
            Destroy(temp);
		}
		tempObjects.Clear ();
		tempObjectType = UIState.Pointer;
	}

	public void SetUIStateDelete ()
	{
		this.CleanUpTempObjects ();
		uiState = UIState.Delete;
		Cursor.SetCursor (deleteCursor, new Vector2 (8, 8), CursorMode.Auto);
	}

	public void SetUIStateRoad ()
	{
		CleanUpDeletion ();
		this.CleanUpTempObjects ();
		this.uiState = UIState.Road;
		SetUpRoadGhost ();
		Cursor.SetCursor (blankCursor, Vector2.zero, CursorMode.Auto);
	}

	public void SetUIStateCommonHousing ()
	{
		CleanUpDeletion ();
		this.CleanUpTempObjects ();
		this.uiState = UIState.Housing;
		SetUpHouseGhost ();
		Cursor.SetCursor (blankCursor, Vector2.zero, CursorMode.Auto);
	}

	public void SetUIStateWell ()
	{
		CleanUpDeletion ();
		this.CleanUpTempObjects ();
		this.uiState = UIState.Well;
		SetUpObjectGhost ();
		Cursor.SetCursor (blankCursor, Vector2.zero, CursorMode.Auto);
	}

	public void SetUIStateFarm1 ()
	{
		CleanUpDeletion ();
		this.CleanUpTempObjects ();
		this.uiState = UIState.Farm1;
		SetUpObjectGhost ();
		Cursor.SetCursor (blankCursor, Vector2.zero, CursorMode.Auto);
	}

	public void SetUIStateStore1 ()
	{
		CleanUpDeletion ();
		this.CleanUpTempObjects ();
		this.uiState = UIState.Store1;
		SetUpObjectGhost ();
		Cursor.SetCursor (blankCursor, Vector2.zero, CursorMode.Auto);
	}

	private void CleanUpDeletion ()
	{
		if (deleting)
		{
			deleting = false;
			curObject.GetComponent<MeshRenderer> ().material = savedMat;
			curObject = null;
		}
	}


	// move the current object associated with the mouse to match the current mouse location
	public void AdjustCurObjectLoc (Vector3 origPoint)
	{
		//Debug.Log (curObject.ToString());
		// we know we have an object and that we're over the ground
		if (!curObject.activeSelf)
		{
			curObject.SetActive (true);
		}


		IntPoint2D mouseTile = this.ConvertPointToTileIndex (origPoint);

		if (this.scenarioInfo.CanBuildHere (mouseTile, curObjectSizeInTiles))
		{
			if (!curObjectIsGreen)
			{
				// change material to green
				curObject.GetComponent<Renderer> ().material = greenMat;
				curObjectIsGreen = true;
			}
		} else
		{
			if (curObjectIsGreen)
			{
				//change material to red
				curObject.GetComponent<Renderer> ().material = redMat;
				curObjectIsGreen = false;
			}
		}
		Vector3 roadPoint = scenarioInfo.ComputeTopLeftPointOfTile (mouseTile);
		if (uiState == UIState.Road)
			curObject.transform.position = roadPoint + RoadPool.roadOffset;
		else if (uiState == UIState.Housing)
			curObject.transform.position = roadPoint + new Vector3 (HousePool.houseOffset, HousePool.houseOffset, HousePool.houseOffset);
		else
			curObject.transform.position = roadPoint + objMgr.GetObjTransform (this.GetUIArrayIndex ());

	}

	void SetUpRoadGhost ()
	{
		Vector3 mouseLoc;
		IntPoint2D tileLoc = IntPoint2D.zero;
		bool disableObject = false;
		if (this.MouseLocationOnGround (out mouseLoc))
		{
			tileLoc = this.ConvertPointToTileIndex (mouseLoc);
			if (this.scenarioInfo.CanBuildHere (tileLoc))
			{
				curObject = roadPool.GetGreenGhost (tileLoc);
				this.curObjectIsGreen = true;
			} else
			{
				curObject = roadPool.GetRedGhost (tileLoc);
				this.curObjectIsGreen = false;
			}
		} else
		{
			// making a disabled green one
			curObject = roadPool.GetGreenGhost (tileLoc);
			this.curObjectIsGreen = true;
			disableObject = true;
		}
		// need to make it
		curObject.SetActive (disableObject);
		this.curObjectSizeInTiles = 1;

	}

	void SetUpHouseGhost ()
	{
		Vector3 mouseLoc;
		IntPoint2D tileLoc = IntPoint2D.zero;
		bool disableObject = false;
		if (this.MouseLocationOnGround (out mouseLoc))
		{
			tileLoc = this.ConvertPointToTileIndex (mouseLoc);
			if (this.scenarioInfo.CanBuildHere (tileLoc))
			{
				curObject = housePool.GetGreenGhost (tileLoc);
				this.curObjectIsGreen = true;
			} else
			{
				curObject = housePool.GetRedGhost (tileLoc);
				this.curObjectIsGreen = false;
			}
		} else
		{
			// making a disabled green one
			curObject = housePool.GetGreenGhost (tileLoc);
			this.curObjectIsGreen = true;
			disableObject = true;
		}
		// need to make it
		curObject.SetActive (disableObject);
		this.curObjectSizeInTiles = 1;
		
	}

	void SetUpObjectGhost ()
	{
		Vector3 mouseLoc;
		IntPoint2D tileLoc = IntPoint2D.zero;
		bool disableObject = false;
		int arrayIndex = this.GetUIArrayIndex ();
		this.curObjectSizeInTiles = objMgr.GetObjSize (arrayIndex);
		if (this.MouseLocationOnGround (out mouseLoc))
		{
			tileLoc = this.ConvertPointToTileIndex (mouseLoc);
			if (this.scenarioInfo.CanBuildHere (tileLoc, curObjectSizeInTiles))
			{
				curObject = objMgr.GetGreenGhost (arrayIndex, tileLoc);
				this.curObjectIsGreen = true;
			} else
			{
				curObject = objMgr.GetRedGhost (arrayIndex, tileLoc);
				this.curObjectIsGreen = false;
			}
		} else
		{
			// making a disabled green one
			curObject = objMgr.GetGreenGhost (arrayIndex, tileLoc);
			this.curObjectIsGreen = true;
			disableObject = true;
		}
		// need to make it
		curObject.SetActive (disableObject);
	}


	private void SetUpBuilding (GameObject building, IntPoint2D tileLoc)
	{
		switch (uiState)
		{
		case UIState.Well: 
			this.SetUpWell (building, tileLoc);
			break;
		case UIState.Farm1:
			this.SetUpFarm (building, tileLoc);
			break;
		case UIState.Store1:
			this.SetUpStore (building, tileLoc);
			break;
		}
	}

	private void SetUpWell (GameObject well, IntPoint2D tileLoc)
	{
		WellManager mgr = well.GetComponent<WellManager> () as WellManager;
		mgr.SetUp (gameObject, scenarioInfo, tileLoc);
	}

	private void SetUpFarm (GameObject farm, IntPoint2D tileLoc)
	{
		FarmManager mgr = farm.GetComponent<FarmManager> () as FarmManager;
		mgr.SetUp (gameObject, scenarioInfo, tileLoc,this);
	}

	private void SetUpStore (GameObject store, IntPoint2D tileLoc)
	{
		StoreManager mgr = store.GetComponent<StoreManager> () as StoreManager;
		mgr.SetUp (gameObject, scenarioInfo, tileLoc);
	}

	public static Direction GetLeftTurn (Direction origDir)
	{
		switch (origDir)
		{
		case Direction.Down:
			return Direction.Right;
		case Direction.Left:
			return Direction.Down;
		case Direction.Up:
			return Direction.Left;
		case Direction.Right:
			return Direction.Up;
		}
		return origDir;
	}

	public static Direction GetRightTurn (Direction origDir)
	{
		switch (origDir)
		{
		case Direction.Down:
			return Direction.Left;
		case Direction.Left:
			return Direction.Up;
		case Direction.Up:
			return Direction.Right;
		case Direction.Right:
			return Direction.Down;
		}
		return origDir;
	}

	public static Direction GetReverseDirection (Direction origDir)
	{
		switch (origDir)
		{
		case Direction.Down:
			return Direction.Up;
		case Direction.Left:
			return Direction.Right;
		case Direction.Up:
			return Direction.Down;
		case Direction.Right:
			return Direction.Left;
		}
		return origDir;
	}

	IEnumerator SetUpInitRoad ()
	{
        Debug.Log("in SetUpInitRoad");
		yield return 0;
		PlaceRoadTiles (initRoadXStart, initRoadXEnd, initRoadYStart, initRoadYEnd, initRoadHorizY, initRoadVertX);
	}

    public void DoQuit()
    {
        Application.Quit();
    }

}
