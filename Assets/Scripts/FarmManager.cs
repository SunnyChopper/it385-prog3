using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FarmManager : MonoBehaviour
{

	private const int kMaxWorkers = 10;
	private const int kCapacity = 1000;
	private const int kDeliveryCapacity = 500;

	[SerializeField] Material[] farmStateMats;

	private ScenarioData scenarioInfo;
	private int numWorkers;
	private PopulationManager popMgr;
	private int curCropAmt;


	private ScenarioMgr scenarioMgr;
	private IntPoint2D centerTile;

	[SerializeField] GameObject deliveryPrefab;


	public void SetUp (GameObject ground, ScenarioData scenario, IntPoint2D tileLoc, ScenarioMgr mgr)
	{
		scenarioInfo = scenario;
		centerTile = tileLoc;
		popMgr = ground.GetComponent ("PopulationManager") as PopulationManager;
		numWorkers = 0;
        scenarioMgr = mgr;
		StartCoroutine ("GetWorkers");
		StartCoroutine ("GrowFarm");
	}

	private bool StartDelivery ()
	{
		// first lets find the possible places to go
		GameObject[] stores = GameObject.FindGameObjectsWithTag ("Store");
		if (stores.Length > 0)
		{
            Debug.Log("found some stores");
			List<IntPoint2D> startTiles = scenarioInfo.GetAdjacentRoadTiles (centerTile);
			List<IntPoint2D> endTiles;
			Stack<IntPoint2D> bestPath = new Stack<IntPoint2D> ();
			StoreManager bestmgr = null;
			foreach (GameObject store in stores)
			{
				//  get the store manager
				StoreManager mgr = store.GetComponent ("StoreManager") as StoreManager;
				// see if there's available capacity
				if (mgr.GetRoom () > 0)
				{
					// get adjacent roads
					endTiles = scenarioInfo.GetAdjacentRoadTiles (mgr.GetLoc ());
					Stack<IntPoint2D> path = scenarioInfo.ShortestPath (startTiles, endTiles);
					if (path.Count > 0 && (bestPath.Count == 0 || path.Count < bestPath.Count))
					{
						bestPath = path;
						bestmgr = mgr;
					}
				}

			}

			bool deliveryStarted = false;
            if (bestPath.Count > 0)
            {
                IntPoint2D start = bestPath.Pop();
                DoDelivery(start, bestPath, bestmgr);
                deliveryStarted = true;
            }
			return deliveryStarted;
		}
		else
		{
			return false;
		}
	}

	public void DoDelivery (IntPoint2D startingTile, Stack<IntPoint2D> path, StoreManager storeMgr)
	{
		int amtToDeliver = Mathf.Min (kDeliveryCapacity, curCropAmt);
		curCropAmt = curCropAmt - amtToDeliver;
		GameObject delPerson = (GameObject)Instantiate (deliveryPrefab);
        Vector3 topLeft = scenarioMgr.ComputeTopLeftPointOfTile(startingTile);
        delPerson.transform.position = topLeft + new Vector3(.5f, .2f, .5f);
        DeliveryManager delMgr = delPerson.GetComponent ("DeliveryManager") as DeliveryManager;
		delMgr.SetUp (scenarioInfo, startingTile, path, storeMgr, amtToDeliver);
	}

	IEnumerator TryToDeliver ()
	{
        Debug.Log("Start Delivery");
		while (curCropAmt > 0)
		{
			StartDelivery ();
			yield return new WaitForSeconds (1);
		}
	}

	IEnumerator GetWorkers ()
	{
		int numNewWorkers = 0;
		int unfilledWorkers = 0;
		while (true)
		{
			//Debug.Log("requesting workers");
			numNewWorkers = popMgr.RequestWorkers (kMaxWorkers - numWorkers, numWorkers, unfilledWorkers);
			numWorkers = numWorkers + numNewWorkers;
			unfilledWorkers = kMaxWorkers - numWorkers;
			yield return new WaitForSeconds (1);
		}
	}


	IEnumerator GrowFarm ()
	{
        while (true)
        {
            float workerPercentage = this.numWorkers / kMaxWorkers;
            Debug.Log("in growth");
            Renderer farmRenderer = gameObject.GetComponent<Renderer>();
            // start growth -- might be nice to do something visible at some point
            foreach (Material curMat in farmStateMats)
            {
                yield return new WaitForSeconds(10);
                workerPercentage = workerPercentage + this.numWorkers / kMaxWorkers;
                Debug.Log("changing mat");
                farmRenderer.material = curMat;
            }
            workerPercentage = workerPercentage / farmStateMats.Length;
            int harvestYield = (int)(kCapacity * workerPercentage);
            // handle harvest
            Debug.Log("harvest yield is " + harvestYield.ToString());
            curCropAmt = curCropAmt + harvestYield;

            StartCoroutine("TryToDeliver");
            yield return new WaitForSeconds(10);
        }
	}

	/*private IntPoint2D GetRoadTile()
	{

	}*/

}
