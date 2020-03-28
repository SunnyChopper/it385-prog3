//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Priority_Queue;

public class SearchNode : PriorityQueueNode
{
	public SearchNode parent;
	public IntPoint2D data;
	public int cost;
	public int heuristic;

	public SearchNode(SearchNode prev, IntPoint2D data, int cost, int heuristic)
	{
		parent = prev;
		this.data = data;
		this.cost = cost;
		this.heuristic = heuristic;
		this.Priority=cost+heuristic;
	}

	public List<SearchNode> GetChildren(IntPoint2D dest, ScenarioData scenario, bool openTravel)
	{
		// Get list of child nodes
		List<SearchNode> children = new List<SearchNode>();

		// Get this cost plus one
		int childCost = this.cost + 1;

		// Integer to hold child heuristic number
		int childHeuristic = 0;

		// Create child data
		IntPoint2D childData = new IntPoint2D(data.xCoord - 1, data.yCoord);
		if ((scenario.IsRoadTile(childData) || (openTravel && scenario.IsPassableTile(childData)) || childData.Equals(dest)) && scenario.IsValidTile(childData) && !InAncestorNode(childData)) {
            childCost = this.cost + 1;
            if (!scenario.IsRoadTile(childData))
                childCost = childCost + 2;
			childHeuristic = Math.Abs(childData.xCoord-dest.xCoord) + Math.Abs(childData.yCoord-dest.yCoord);
			children.Add(new SearchNode(this,childData,childCost,childHeuristic));
		}

		childData = new IntPoint2D(data.xCoord+1,data.yCoord);
		if ((scenario.IsRoadTile(childData) || (openTravel&&scenario.IsPassableTile(childData)) || childData.Equals(dest)) && scenario.IsValidTile(childData) && !InAncestorNode(childData))
		{
            childCost = this.cost + 1;
            if (!scenario.IsRoadTile(childData))
                childCost = childCost + 2;
            childHeuristic = Math.Abs(childData.xCoord-dest.xCoord) + Math.Abs(childData.yCoord-dest.yCoord);
			children.Add(new SearchNode(this,childData,childCost,childHeuristic));
		}

		childData = new IntPoint2D(data.xCoord,data.yCoord-1);
		if ((scenario.IsRoadTile(childData) || (openTravel&&scenario.IsPassableTile(childData)) || childData.Equals(dest)) && scenario.IsValidTile(childData) && !InAncestorNode(childData))
		{
            childCost = this.cost + 1;
            if (!scenario.IsRoadTile(childData))
                childCost = childCost + 2;
            childHeuristic = Math.Abs(childData.xCoord-dest.xCoord) + Math.Abs(childData.yCoord-dest.yCoord);
			children.Add(new SearchNode(this,childData,childCost,childHeuristic));
		}

		childData = new IntPoint2D(data.xCoord,data.yCoord+1);
		if ((scenario.IsRoadTile(childData) || (openTravel&&scenario.IsPassableTile(childData)) || childData.Equals(dest)) && scenario.IsValidTile(childData) && !InAncestorNode(childData))
		{
            childCost = this.cost + 1;
            if (!scenario.IsRoadTile(childData))
                childCost = childCost + 2;
            childHeuristic = Math.Abs(childData.xCoord-dest.xCoord) + Math.Abs(childData.yCoord-dest.yCoord);
			children.Add(new SearchNode(this,childData,childCost,childHeuristic));
		}

		return children;
	}

	public bool AtGoal(IntPoint2D dest)
	{
		return data.Equals(dest);
	}

	private bool InAncestorNode(IntPoint2D tile)
	{
		bool found = false;
		SearchNode curNode = parent;
		while (curNode != null)
		{
			if (tile.Equals(curNode.data)) {
				return true;
			} else {
				curNode = curNode.parent;
			}
		}
		return false;
	}
}
