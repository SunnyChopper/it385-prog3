
using UnityEngine;
using System.Collections;

public abstract class BuildingData
{
	protected IntPoint2D size;
	protected IntPoint2D topLeft;

	public BuildingData ()
	{
	}

	public IntPoint2D GetLoc()
	{
		return topLeft;
	}

	public IntPoint2D GetSize()
	{
		return size;
	}
}


