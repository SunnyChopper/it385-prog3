using UnityEngine;
using System.Collections;

public class GridTileInfo
{
	enum Moveability
	{
		Road,
		Passable,
		Impassable}

	;

	private Moveability mobility;
	private bool clear;
	private bool isHouse;
	private GameObject building;
	private int buildingSize;
	private IntPoint2D buildingTopLeft;

	public GridTileInfo ()
	{
		mobility = Moveability.Passable;
		clear = true;
		building = null;
		isHouse = false;
		buildingSize = 0;
	}

	public IntPoint2D GetTopLeftOfBuilding ()
	{
		return buildingTopLeft;
	}

	public bool IsRoad ()
	{
		return mobility == Moveability.Road;
	}

	public bool CanPass ()
	{
		return mobility != Moveability.Impassable;
	}

	public void MakeRoad (GameObject tile, IntPoint2D topLeft)
	{
		mobility = Moveability.Road;
		clear = false;
		building = tile;
		buildingSize = 1;
		this.buildingTopLeft = topLeft;
	}

	public void MakeHouse (GameObject house, IntPoint2D topLeft)
	{
		mobility = Moveability.Impassable;
		clear = false;
		building = house;
		isHouse = true;
		buildingSize = 1;
		this.buildingTopLeft = topLeft;
	}

	public void MakeOther (GameObject building, int size, IntPoint2D topLeft)
	{
		mobility = Moveability.Impassable;
		clear = false;
		this.building = building;
		buildingSize = size;
		this.buildingTopLeft = topLeft;
	}

	public bool IsClear ()
	{
		return clear;
	}

	public bool IsHouse ()
	{
		return isHouse;
	}


	public void DeleteTileContents ()
	{
		building = null;
		mobility = Moveability.Passable;
		isHouse = false;
		clear = true;
	}

	public void DistributeWater ()
	{
		if (isHouse) {

			HouseManager mgr = this.building.GetComponent ("HouseManager") as HouseManager;
			mgr.ReceiveWater ();
		}
	}

	public GameObject GetBuilding ()
	{
		return building;
	}

	public int GetSides ()
	{
		return buildingSize;
	}
}
