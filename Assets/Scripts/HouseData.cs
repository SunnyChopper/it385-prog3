using System;
using UnityEngine;

public class HouseData : BuildingData
{
	private const int kMaxWater = 50;
    private const int kMaxResource2 = 50;
	private int level;
	private int assignedPopulation;
	private int arrivedPopulation;
	private int water;
	private int resource2;
	private int[] maxPop;

	public HouseData (IntPoint2D location, int population = 0)
	{
		level = 0;
		this.size = new IntPoint2D (1, 1);
		this.topLeft = location;
		this.assignedPopulation = this.arrivedPopulation = population;
		maxPop = new int[4];
		maxPop [0] = 1;
		maxPop [1] = 3;
		maxPop [2] = 7;
		maxPop [3] = 12;
	}

	public int GetLevel ()
	{
		return level;
	}

	public int GetPop ()
	{
		return assignedPopulation;
	}

	public void ProcessTimeTick ()
	{
		if (water > 0) {
			water--;
		}
		if (resource2 > 0) {
			resource2--;
		}
		if (arrivedPopulation <= 0) {
			level = 0;
		} else {
			switch (level) {
			case 0:
				if (arrivedPopulation > 0)
					level++;
				break;
			case 1:
				if (water > 0)
					level++;
				break;
			case 2:
				if (water == 0)
					level--;
				if (resource2 > 0)
					level++;
				break;
			case 3:
				if (resource2 == 0)
					level--;
				break;
			}
		}
	}

	public void AddPerson ()
	{
		if (assignedPopulation < maxPop [level]) {
			assignedPopulation++;
		}
	}

	public void RecordArrival ()
	{
		if (arrivedPopulation < assignedPopulation) {
			arrivedPopulation++;
		}
	}

	public int GetRoom ()
	{
		return maxPop [level] - this.assignedPopulation;
	}

	public void RemovePeople (int numRemoved)
	{
		this.assignedPopulation = this.assignedPopulation - numRemoved;
		this.arrivedPopulation = Math.Min (this.arrivedPopulation, this.assignedPopulation);
	}

	public void ReceiveWater ()
	{
		if (water < kMaxWater) {
			water = Math.Min (water + 40, kMaxWater);
		}
	}

    public void ReceiveResource2(int amount)
    {
        resource2 = resource2 + amount;
    }

    public bool NeedResource2()
    {
        return level >= 2 && resource2 <= kMaxResource2 / 2;
    }
}

