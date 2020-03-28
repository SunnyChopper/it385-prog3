using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HouseManager : MonoBehaviour
{

	private HouseData myData;
	private int curLevel;
	[SerializeField] Material[] levelMats;

    private bool gettingResource2;
	

	public IntPoint2D GetHouseLocation ()
	{
		return myData.GetLoc ();
	}

	public void AssignHouseData (HouseData theHouse)
	{
		myData = theHouse;
	}

	public void StartUpdates ()
	{
		StartCoroutine ("HouseUpdate");
        StartCoroutine("CheckResources");
	}

	IEnumerator HouseUpdate ()
	{
		yield return new WaitForSeconds (1);
		while (true) {
			if (myData != null) {
				myData.ProcessTimeTick ();
				int theLevel = myData.GetLevel ();
				if (theLevel != curLevel) {
					curLevel = theLevel;
					//fix the level
					GetComponent<Renderer> ().material = levelMats [curLevel];
				}
			}
			yield return new WaitForSeconds (1);
		}
	}

	public int GetRoom ()
	{
		return myData.GetRoom ();
	}

	public int GetPop ()
	{
		return myData.GetPop ();
	}

	public void AddPerson ()
	{
		myData.AddPerson ();
	}

	public void RemovePeople (int numRemoved)
	{
		myData.RemovePeople (numRemoved);
	}

	public void ReceiveWater ()
	{
		myData.ReceiveWater ();
	}

	public void RecordArrival ()
	{
		myData.RecordArrival ();
	}

    public void ReceiveResource2(int goods)
    {
        gettingResource2 = false;
        myData.ReceiveResource2(goods);
    }

    private void ObtainResource2()
    {

    }

    private void StartRetrieval()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        ScenarioMgr scenarioMgr = gameController.GetComponent("ScenarioMgr") as ScenarioMgr;
        ScenarioData scenarioInfo = scenarioMgr.GetInfo();
        // first lets find the possible places to go
        GameObject[] stores = GameObject.FindGameObjectsWithTag("Store");
        if (stores.Length > 0)
        {
            //Debug.Log("found some stores");
            List<IntPoint2D> startTiles = scenarioInfo.GetAdjacentRoadTiles(myData.GetLoc());
            List<IntPoint2D> endTiles;
            Stack<IntPoint2D> bestPath = new Stack<IntPoint2D>();
            StoreManager bestmgr = null;
            foreach (GameObject store in stores)
            {
                //  get the store manager
                StoreManager mgr = store.GetComponent("StoreManager") as StoreManager;
                // see if there's available capacity
                if (mgr.GetGoodsAmt() > 0)
                {
                    // get adjacent roads
                    endTiles = scenarioInfo.GetAdjacentRoadTiles(mgr.GetLoc());
                    Stack<IntPoint2D> path = scenarioInfo.ShortestPath(startTiles, endTiles);
                    if (path.Count > 0 && (bestPath.Count == 0 || path.Count < bestPath.Count))
                    {
                        bestPath = path;
                        bestmgr = mgr;
                    }
                }

            }

            gettingResource2 = false;
            if (bestPath.Count > 0)
            {
                IntPoint2D start = bestPath.Pop();
                GetGoods(start, bestPath, bestmgr, scenarioInfo);
                gettingResource2 = true;
            }

        }
    }

    public void GetGoods(IntPoint2D startingTile, Stack<IntPoint2D> path, StoreManager storeMgr, ScenarioData scenarioInfo)
    {
        GameObject level = GameObject.FindWithTag("GameController");

        WalkerPool pool = level.GetComponent("WalkerPool") as WalkerPool;

        GameObject citizen = pool.GetCitizen(startingTile,ScenarioMgr.Direction.Up);
        
        CitizenManager citMgr = citizen.GetComponent("CitizenManager") as CitizenManager;
        citMgr.SetUp(scenarioInfo, startingTile,  ScenarioMgr.Direction.Up, path, storeMgr, this);

    }

    IEnumerator CheckResources()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            if (!gettingResource2 && myData.NeedResource2())
            {
                StartRetrieval();
            }
            yield return new WaitForSeconds(1);
        }
    }
}
