using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class EstateJobManager {

	List<EstateJob> haulBuildJobList;
	List<EstateJob> cleanJobList;
	List<EstateJob> gardenJobList;
    List<Tile> reservedTiles; //Used for build sites as well as 

	Action<EstateJob> onJobCreatedCallback; 

	int jobAgeWeighting = 1;
	int jobDistanceWeighting = -2;

	// Use this for initialization

	public EstateJobManager (){
		this.haulBuildJobList = new List<EstateJob> ();
		this.cleanJobList 	  = new List<EstateJob> ();
		this.gardenJobList    = new List<EstateJob> ();
        this.reservedTiles    = new List<Tile>();
	}

	public EstateJobManager(List<EstateJob> loadedJobs){
		foreach (EstateJob job in loadedJobs) {
			AddJob (job);
		}
	}

	public void AddJob(EstateJob job){
		if (JobTileAlreadyHasJobOfType (job.jobTile, job.JobType)) {
			Debug.Log ("EJM::AddJob - Job is already queued up on this tile");
			return;
		}

		//If this job is instant then dont add to queues. just do it
		if (job.JobTime <= 0) {
			job.WorkJob (0);
			return;
		}


		switch (job.JobType) {
		case EstateJobType.BUILD:
		case EstateJobType.HAUL:
			Debug.LogAssertion ("EstateJobManager:: AddJob - Added job to buildlist");
			haulBuildJobList.Add (job);
			break;
		case EstateJobType.CLEAN:
			cleanJobList.Add (job);
			break;
		case EstateJobType.GARDEN:
			gardenJobList.Add (job);
			break;
		default:
			Debug.LogError ("EstateJobManager::AddJob -- This job has no type or unrecognised type");
			break;
		}

        ReserveTiles(job);
		if (onJobCreatedCallback != null)
			onJobCreatedCallback (job);
	}

	protected void RemoveJob(EstateJob job){
		switch (job.JobType) {
		case EstateJobType.BUILD:
		case EstateJobType.HAUL:
			if (haulBuildJobList.Contains (job)) {
				haulBuildJobList.Remove (job);
			} else 
				Debug.LogError("EstateJobManager::RemoveJob -- Trying to remove a job not in the haulBuildJoblist");
			break;
		case EstateJobType.CLEAN:
			if (cleanJobList.Contains (job)) {
				cleanJobList.Remove (job);
			} else 
				Debug.LogError("EstateJobManager::RemoveJob -- Trying to remove a job not in the cleanJobList");
			break;
		case EstateJobType.GARDEN:
			if (gardenJobList.Contains (job)) {
				gardenJobList.Remove (job);
			} else 
				Debug.LogError("EstateJobManager::RemoveJob -- Trying to remove a job not in the gardenJobList");
			break;
		default:
			Debug.LogError ("EstateJobManager::RemoveJob -- This job has no type or unrecognised type");
			break;
		}

        UnreserveTiles(job);
	}

    public EstateJob GiveJob(Tile currCharTile, Character character) {

        EstateJob jobToGive = null;

        switch (character.CharRole) {
            case CharacterRole.BUILDER:
                if (haulBuildJobList.Count > 0)
                    jobToGive = FindBestJobForCharRoleAtTile(currCharTile, haulBuildJobList);
                break;
            case CharacterRole.JANITOR:
                if (cleanJobList.Count > 0)
                    jobToGive = FindBestJobForCharRoleAtTile(currCharTile, cleanJobList);
                break;
            case CharacterRole.GARDERNER:
                if (gardenJobList.Count > 0)
                    jobToGive = FindBestJobForCharRoleAtTile(currCharTile, gardenJobList);
                break;
            default:
                jobToGive = null; //FIXME instead of null job, we'll give them some idle job
                break;
        }
        if (jobToGive != null)
        {
            RemoveJob(jobToGive);
            jobToGive.character = character;
        }
        return jobToGive;
	}

	protected EstateJob FindBestJobForCharRoleAtTile(Tile currTile, List<EstateJob> jobList){
		EstateJob bestJob = jobList.First ();
        Debug.LogError("bestJob is at: " + bestJob.jobTile.X + ":" + bestJob.jobTile.Y);
		int distanceToJob = new PathAstar (WorldController.Instance.World, currTile, bestJob.jobTile).Length (); //FIXME somePathfindingThing(currTile, job);
		float maxJobPriority = (bestJob.JobAge * jobAgeWeighting) + (distanceToJob * jobDistanceWeighting);
		foreach (EstateJob job in jobList) {
			//TODO: All the pf stuff
			distanceToJob = new PathAstar (WorldController.Instance.World, currTile, job.jobTile).Length ();
			float thisJobPriority = (job.JobAge * jobAgeWeighting) + (distanceToJob * jobDistanceWeighting);
			if (thisJobPriority > maxJobPriority) {
				bestJob = job;
				maxJobPriority = thisJobPriority;
			}
		}

		return bestJob;
	}

	public List<EstateJob> GetAllJobs (){
		return haulBuildJobList.Concat (cleanJobList).Concat (gardenJobList).ToList ();
	}

	bool JobTileAlreadyHasJobOfType(Tile jobTile, EstateJobType type){
		//FIXME: For now we are just checking to see if there is any job in that tile and aborting
		//Later on this will need to be more complex were we cancel jobs if they can be overwritten,
		//i.e cleaning and gardening jobs can be cancelled if a construction job is placed on that tile

		List<EstateJob> allJobs = GetAllJobs ();

		foreach (EstateJob job in allJobs) {
			if (job.jobTile == jobTile)
				return true;
		}
		return false;
	}

    public bool DoesTileHavePendingBuildJob(Tile tile)
    {
        foreach(EstateJob job in haulBuildJobList)
        {
            if (job.jobTile == tile)
                return true;
        }

        return false;
    }

    void ReserveTiles(EstateJob job)
    {
        if (job.FixturePrototype == null)
        {
            reservedTiles.Add(job.jobTile);
            return;
        }

        for (int xOffset = job.jobTile.X; xOffset < (job.jobTile.X + job.FixturePrototype.Width); xOffset++)
        {
            for (int yOffset = job.jobTile.Y; yOffset < (job.jobTile.Y + job.FixturePrototype.Height); yOffset++)
            {
                reservedTiles.Add(WorldController.Instance.World.GetTileAt(xOffset, yOffset));
            }
        }
    }

    void UnreserveTiles(EstateJob job)
    {
        if (job.FixturePrototype == null)
        {
            reservedTiles.Remove(job.jobTile);
            return;
        }

        for (int xOffset = job.jobTile.X; xOffset < (job.jobTile.X + job.FixturePrototype.Width); xOffset++)
        {
            for (int yOffset = job.jobTile.Y; yOffset < (job.jobTile.Y + job.FixturePrototype.Height); yOffset++)
            {
                reservedTiles.Remove(WorldController.Instance.World.GetTileAt(xOffset, yOffset));
            }
        }
    }

    public bool IsTileReserved(Tile tile)
    {
        if (reservedTiles.Count > 0)
        {
            foreach (Tile rTile in reservedTiles)
            {
                if (tile == rTile)
                    return true;
            }
            return false;
        }
        return false;
    }
	public void RegisterJobCreatedCallback (Action<EstateJob> cbFunc){
		onJobCreatedCallback += cbFunc;
	}

	public void UnregisterJobCreatedCallback (Action<EstateJob> cbFunc){
		onJobCreatedCallback -= cbFunc;
	}
}
