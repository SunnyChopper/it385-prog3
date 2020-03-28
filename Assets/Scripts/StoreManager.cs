using UnityEngine;
using System.Collections;

public class StoreManager : MonoBehaviour
{

	private const int kMaxWorkers = 6;
	private const int kCapacity = 2000;

	private ScenarioData scenarioInfo;
	private int numWorkers;
	private PopulationManager popMgr;
	private int amountStored;
	// will eventually need to add something about types of goods

	//	private ScenarioMgr scenarioMgr;
	private IntPoint2D centerTile;

	public void SetUp (GameObject ground, ScenarioData scenario, IntPoint2D tileLoc)
	{
		scenarioInfo = scenario;
		centerTile = tileLoc;
		popMgr = ground.GetComponent ("PopulationManager") as PopulationManager;
		numWorkers = 0;
		amountStored = 0;
		StartCoroutine ("GetWorkers");
	}

	public int ReceiveGoods (int amount) // will eventually need to add something about types of goods
	{
		if (numWorkers > 0)
		{
			int remaining = 0;
			int space = kCapacity - amountStored;
			if (amount <= space)
			{
				amountStored = amountStored + amount;
			} else
			{
				amountStored = kCapacity;
				remaining = amount - space;
			}			
			return remaining;
		} else
			return amount;
	}

    public int ProvideGoods (int amountRequested)
    {
        if (numWorkers > 2)
        {
            int amount = Mathf.Min(amountRequested, amountStored);
            amountStored = amountStored - amount;
            return amount;
        }
        else return 0;
    }

	public int GetRoom ()
	{
		return kCapacity - amountStored;
	}

    public int GetGoodsAmt()
    {
        return amountStored;
    }

	public IntPoint2D GetLoc ()
	{
		return centerTile;
	}

    IEnumerator GetWorkers()
    {
        int numNewWorkers = 0;
        int unfilledWorkers = 0;
        while (true)
        {
            //Debug.Log("requesting workers");
            numNewWorkers = popMgr.RequestWorkers(kMaxWorkers - numWorkers, numWorkers, unfilledWorkers);
            numWorkers = numWorkers + numNewWorkers;
            unfilledWorkers = kMaxWorkers - numWorkers;
            yield return new WaitForSeconds(1);
        }
    }
}
