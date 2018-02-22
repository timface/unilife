using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EstateJobType { HAUL, BUILD, CLEAN, GARDEN }

public class EstateJob {

	public Tile jobTile;
	public Tile pickupTile;
    public Character character = null;

	public float JobTime {
		get;
		protected set;
	}

	public EstateJobType JobType {
		get;
		protected set;
	}

	public float JobAge {
		get;
		protected set;
	}

	public Fixture FixturePrototype {
		get;
		protected set;
	}

	public HaulableItem[] requiredHaulableItems {
		get;
		protected set;
	}

	Action<EstateJob> jobOnCompletedCallback;
	Action<EstateJob> jobOnStoppedCallback;
	Action<EstateJob> jobOnWorkedCallback;

	public EstateJob (Tile jobTile, EstateJobType jobType, float jobTime, Action<EstateJob> jobOnCompletedCallback, Fixture fixturePrototype = null, Tile pickupTile = null){ 
		this.jobTile = jobTile;
		this.JobType = jobType;
		this.JobTime = jobTime;
        this.jobOnCompletedCallback = jobOnCompletedCallback;
		this.FixturePrototype = fixturePrototype;
        this.requiredHaulableItems = new HaulableItem[] { };

		//if (JobType == EstateJobType.BUILD)
			//this.requiredHaulableItems = FixturePrototype.requiredHaulableItems;
		
		if (JobType == EstateJobType.HAUL)
			this.pickupTile = pickupTile;
	}

	public void WorkJob(float workTime) {
		switch (JobType) {
		case EstateJobType.BUILD:
			WorkBuildJob (workTime);
			break;
		case EstateJobType.HAUL:
			WorkHaulJob (workTime);
			break;
		case EstateJobType.CLEAN:
		case EstateJobType.GARDEN:
			WorkCleanGardenJob (workTime);
			break;
		}
	}

	protected void WorkBuildJob(float workTime) {
        //Debug.Log("Working build job");
        if (requiredHaulableItems.Length > 0)
        {
            if (HasAllHaulableItems())
            {
                JobTime -= workTime;
                //Debug.Log(JobTime);
                if (jobOnWorkedCallback != null)
                    jobOnWorkedCallback(this);

                if (JobTime <= 0)
                {
                    Debug.Log("EJ::WorkBuildJob - Job is done, should be completing ");
                    if (jobOnCompletedCallback != null)
                        jobOnCompletedCallback(this);
                }
            }
        }
        else
        {
            JobTime -= workTime;
            //Debug.Log(JobTime);
            if (jobOnWorkedCallback != null)
                jobOnWorkedCallback(this);

            if (JobTime <= 0)
            {
               // Debug.Log("EJ::WorkBuildJob - Job is done, should be completing ");
                if (jobOnCompletedCallback != null)
                {
                    jobOnCompletedCallback(this);
                    jobOnStoppedCallback(this);
                }

            }
        }
	}

	protected void WorkHaulJob(float workTime)
    {
        JobTime -= workTime;
        //Debug.Log(JobTime);
        if (jobOnWorkedCallback != null)
            jobOnWorkedCallback(this);

        if (JobTime <= 0)
        {
            // Debug.Log("EJ::WorkBuildJob - Job is done, should be completing ");
            if (jobOnCompletedCallback != null)
            {
                jobOnCompletedCallback(this);
                jobOnStoppedCallback(this);
            }

        }
    }

	protected void WorkCleanGardenJob(float workTime){
	
	}

	public bool HasAllHaulableItems(){
		foreach (HaulableItem item in requiredHaulableItems) {
			if (item.StackSize != item.RequiredStackSize) {
				return false;
			}
		}

		return true;
	}

	public void RegisterJobCompletedCallback (Action<EstateJob> cbFunc){
		jobOnCompletedCallback += cbFunc;
	}

	public void UnregisterJobCompletedCallback (Action<EstateJob> cbFunc){
		jobOnCompletedCallback -= cbFunc;
	}

	public void RegisterJobStoppedCallback (Action<EstateJob> cbFunc){
		jobOnStoppedCallback += cbFunc;
	}

	public void UnregisterJobStoppedCallback (Action<EstateJob> cbFunc){
		jobOnStoppedCallback -= cbFunc;
	}

	public void RegisterJobWorkedCallback (Action<EstateJob> cbFunc){
		jobOnWorkedCallback += cbFunc;
	}

	public void UnregisterJobWorkedCallback (Action<EstateJob> cbFunc){
		jobOnWorkedCallback -= cbFunc;
	}
}
