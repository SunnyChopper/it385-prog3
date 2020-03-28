using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PopulationManager : MonoBehaviour
{

	public int population;
	public bool immigrationAllowed;
	public bool emigrationAllowed;
	public float workforcePercentage;
	public int citizenEntryX;
	public int citizenEntryY;
	public Text popInfoText;
	public Text workforceInfoText;
	public Text employedInfoText;
	public Text homelessInfoText;

	private Dictionary<IntPoint2D,HouseManager> houseCollection;
	private Queue<CitizenManager> homelessCollection;
	// need a collection of homeless bodies

	private WalkerPool walkerPool;
	private ScenarioData scenario;
	private IntPoint2D startTile;

	public int homeless;
	private int workforce;
	private int employed;
	private int unfilledWorkerRequests;
	private const int minImmigration = 2;
	private const int maxImmigration = 20;
	private const float immigrationPercentage = .1f;

	// Use this for initialization
	void Start ()
	{
        Debug.Log("in PopulationManager start");
		homeless = population;
		workforce = (int)(population * workforcePercentage);
		employed = 0;
		unfilledWorkerRequests = 0;
		startTile = new IntPoint2D (citizenEntryX, citizenEntryY);
		houseCollection = new Dictionary<IntPoint2D, HouseManager> ();
		homelessCollection = new Queue<CitizenManager> ();
		walkerPool = gameObject.GetComponent ("WalkerPool") as WalkerPool;
		ScenarioMgr scenMgr = gameObject.GetComponent ("ScenarioMgr") as ScenarioMgr;
		scenario = scenMgr.GetInfo ();
		StartCoroutine ("InitPopulation");
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void AddHouse (IntPoint2D tileLoc, HouseManager houseMgr)
	{
		houseCollection.Add (tileLoc, houseMgr);
	}

	public void RemoveHouse (IntPoint2D tileLoc)
	{
		HouseManager house;
		if (houseCollection.TryGetValue (tileLoc, out house)) {
			int pop = house.GetPop ();
			homeless = homeless + pop;
			for (int i = 0; i < pop; i++) {
				GameObject citizen = walkerPool.GetCitizen (tileLoc, ScenarioMgr.Direction.Left);
				CitizenManager mgr = citizen.GetComponent ("CitizenManager") as CitizenManager;
				mgr.SetUp (scenario, tileLoc, ScenarioMgr.Direction.Right, true, startTile, null,false);
				homelessCollection.Enqueue (mgr);
			}
			houseCollection.Remove (tileLoc);
			homelessInfoText.text = "Homeless:  " + homeless;
		}
	}

	public int RequestWorkers (int numNeeded, int curEmployed = 0, int previousUnfilled = 0)
	{
		int unemployed = workforce - employed;
		int workersProvided = 0;
		if (previousUnfilled > 0) {
			unfilledWorkerRequests = unfilledWorkerRequests - previousUnfilled;
		}
		if (unemployed < 0) {
			if (curEmployed > 0) {
				workersProvided = -1;
				unfilledWorkerRequests = unfilledWorkerRequests + previousUnfilled + 1;
			}
		} else if (unemployed > 0 && numNeeded > 0) {
			// we have any workers
			if (numNeeded * 10 <= (unemployed - unfilledWorkerRequests)) {
				// just give me all of my workers, please
				unemployed = unemployed - numNeeded;
				workersProvided = numNeeded;
			} else {
				workersProvided = Mathf.Max (1, (unemployed - unfilledWorkerRequests) / 10);
				unfilledWorkerRequests = unfilledWorkerRequests + numNeeded - workersProvided;
			}

		} else if (unemployed == 0) {
			workersProvided = 0;
			unfilledWorkerRequests = unfilledWorkerRequests + numNeeded;
		} 
		employed = employed + workersProvided;
		return workersProvided;
	}

	IEnumerator InitPopulation ()
	{
		yield return 0;
		for (int i = 0; i < homeless; i++) {
			GameObject citizen = walkerPool.GetCitizen (startTile, ScenarioMgr.Direction.Right);
			CitizenManager mgr = citizen.GetComponent ("CitizenManager") as CitizenManager;
			mgr.SetUp (scenario, startTile, ScenarioMgr.Direction.Right, true, startTile, null, false);
			homelessCollection.Enqueue (mgr);
			yield return 0;
		}
		StartCoroutine ("PopulationUpdate");
	}

	IEnumerator PopulationUpdate ()
	{
		CitizenManager mgr;
		yield return new WaitForSeconds (1);
		while (true) {
			//int numHouses = houseCollection.Count;
			//Debug.Log ("/pdating with " + numHouses.ToString());
			//Debug.Log ("population: " + this.population.ToString() + "  homeless: " + this.homeless.ToString());

			int openSpots = 0;
			HouseManager[] tempHouseColl = new HouseManager[houseCollection.Count];
			houseCollection.Values.CopyTo (tempHouseColl, 0);

			// check housing for spaces (adding and deleting pop)
			foreach (HouseManager house in tempHouseColl) {
				int numSpaces = house.GetRoom ();

				IntPoint2D houseLoc = house.GetHouseLocation ();
				if (numSpaces < 0) {
					int kickedOut = 0 - numSpaces;
					house.RemovePeople (kickedOut);
					IntPoint2D clearTile = scenario.CloseClearTile (houseLoc);
					homeless = homeless + kickedOut;
					for (int i = 0; i < kickedOut; i++) {
						GameObject citizen = walkerPool.GetCitizen (clearTile, ScenarioMgr.Direction.Left);
						mgr = citizen.GetComponent ("CitizenManager") as CitizenManager;
						mgr.SetUp (scenario, clearTile, ScenarioMgr.Direction.Right, true, startTile, null,false);
						homelessCollection.Enqueue (mgr);
						yield return new WaitForSeconds (0.1f);
					}

				} else if (numSpaces > 0) {
					if (homeless > 0 && UnityEngine.Random.Range (1, 5) < 4) {
						//Debug.Log("adding a person to a house");
						house.AddPerson ();
						mgr = homelessCollection.Dequeue ();
						mgr.SendToDest (houseLoc, house,true);
						homeless--;
						openSpots--;
						yield return new WaitForSeconds (0.1f);
					}
					openSpots = openSpots + numSpaces;
				}
			}

			//Debug.Log ("Open spots: " + openSpots.ToString());
			// have pop come or go
			if (emigrationAllowed && homeless > openSpots) {
				population--;
				homeless--;
				mgr = homelessCollection.Dequeue ();
				mgr.SendToDest (startTile, null, false);
				yield return 0;
			}

			openSpots = openSpots - homeless;
			if (immigrationAllowed && openSpots > 0) {
				int letIn = (int)(openSpots * immigrationPercentage);
				if (letIn > maxImmigration)
					letIn = maxImmigration;
				else if (letIn < minImmigration)
					letIn = Math.Min (minImmigration, openSpots);
				population = population + letIn;
				homeless = homeless + letIn;
				// handle people entering the map
				for (int i = 0; i < letIn; i++) {
					GameObject citizen = walkerPool.GetCitizen (startTile, ScenarioMgr.Direction.Right);
					mgr = citizen.GetComponent ("CitizenManager") as CitizenManager;
					mgr.SetUp (scenario, startTile, ScenarioMgr.Direction.Right, true, startTile, null, false);
					homelessCollection.Enqueue (mgr);
					//yield return new WaitForSeconds (0.1f);
				}
			} 
			workforce = (int)(population * workforcePercentage);
			popInfoText.text = "Population: " + population;
			workforceInfoText.text = "Workforce:  " + workforce;
			employedInfoText.text = "Employed:  " + employed;
			homelessInfoText.text = "Homeless:  " + homeless;
			yield return new WaitForSeconds (1);
		}
	}

}
